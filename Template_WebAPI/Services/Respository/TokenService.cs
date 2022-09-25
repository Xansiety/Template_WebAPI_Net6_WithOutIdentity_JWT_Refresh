using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Template_WebAPI.DTOs;
using Template_WebAPI.Entities.Users;
using Template_WebAPI.Helpers.AuthTools;
using Template_WebAPI.Services.Interfaces;

namespace Template_WebAPI.Services.Respository
{
    public class TokenService : ITokenService
    {
        private readonly JWT _jwt;
        private readonly IUserService userService;
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext _context;
        private readonly DapperContext dapperContext;

        public TokenService(IConfiguration configuration, ApplicationDbContext context, DapperContext dapperContext, IOptions<JWT> jwt, IUserService userService)
        {
            _jwt = jwt.Value;
            this.configuration = configuration;
            this._context = context;
            this.dapperContext = dapperContext;
            this.userService = userService;
        }

        public async Task SaveTokens(Usuario userInfo)
        {
            _context.Usuarios.Update(userInfo);
            //save this data in db   
            await _context.SaveChangesAsync();
        }


        public async Task<ResponseAuth> GenerateAccessTokenAsync(UserCredentials model)
        {
            ResponseAuth datosUsuarioDto = new ResponseAuth();
            //validamos al usuario en la base de datos
            var usuario = await userService.ValidateUserAsync(model);

            if (usuario == null)
            {
                datosUsuarioDto.EstaAutenticado = false;
                datosUsuarioDto.Mensaje = $"Credenciales incorrectas para el usuario {model.Email}.";
                return datosUsuarioDto;
            }

            datosUsuarioDto.EstaAutenticado = true;
            JwtSecurityToken jwtSecurityToken = CreateJwtToken(usuario); //creamos el token 
            datosUsuarioDto.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            datosUsuarioDto.DisplayName = usuario.Email;
            datosUsuarioDto.Roles = usuario.Roles
                                             .Select(u => u.Nombre)
                                             .ToList(); //arreglo de roles


            //lógica para refresh Token
            //si existe algún RefreshToken activo regreso el token
            if (usuario.IsActiveRefreshToken)
            {
                datosUsuarioDto.RefreshToken = usuario.RefreshToken;
                datosUsuarioDto.RefreshTokenExpiration = usuario.ExpireTimeRefreshToken;
            }
            else
            {
                //se crea un nuevo token
                var response = CreateRefreshToken();
                datosUsuarioDto.RefreshToken = response.RefreshToken;
                datosUsuarioDto.RefreshTokenExpiration = response.ExpireTimeRefreshToken;

                //to save in db
                usuario.RefreshToken = response.RefreshToken;
                usuario.ExpireTimeRefreshToken = response.ExpireTimeRefreshToken;
                usuario.CreatedRefreshToken = response.CreatedRefreshToken;
                await SaveTokens(usuario);
            }

            return datosUsuarioDto;

        }


        private JwtSecurityToken CreateJwtToken(Usuario usuario)
        {
            //añadimos los roles en una lista de claims
            var roles = usuario.Roles;
            var roleClaims = new List<Claim>();
            foreach (var role in roles)
            {
                roleClaims.Add(new Claim("roles", role.Nombre));
            }
            var claims = new[]
            {
              new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
              new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
              new Claim("uid", usuario.Id.ToString()),
              new Claim("username", usuario.Email)
            }.Union(roleClaims); //unimos los roles

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }



        private Usuario CreateRefreshToken()
        {
            var randonNumber = new byte[32];
            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(randonNumber);
            return new Usuario
            {
                RefreshToken = Convert.ToBase64String(randonNumber),
                ExpireTimeRefreshToken = DateTime.UtcNow.AddDays(7),
                CreatedRefreshToken = DateTime.UtcNow
            };
        }


        public async Task<ResponseAuth> RefreshTokenAsync(string refreshToken)
        {
            var datosUsuarioDto = new ResponseAuth();

            var usuario = await userService.GetByRefreshTokenAsync(refreshToken);

            if (usuario == null)
            {
                datosUsuarioDto.EstaAutenticado = false;
                datosUsuarioDto.Mensaje = $"El token no pertenece a ningún usuario.";
                return datosUsuarioDto;
            }

            if (!usuario.IsActiveRefreshToken)
            {
                datosUsuarioDto.EstaAutenticado = false;
                datosUsuarioDto.Mensaje = $"El token no está activo.";
                return datosUsuarioDto;
            }
            //Revocamos el Refresh Token actual y
            //usuario.RevokedRefreshToken = DateTime.UtcNow;
            //generamos un nuevo Refresh Token y lo guardamos en la Base de Datos
            var newRefreshToken = CreateRefreshToken();

            usuario.RefreshToken = newRefreshToken.RefreshToken;
            usuario.ExpireTimeRefreshToken = newRefreshToken.ExpireTimeRefreshToken;
            usuario.CreatedRefreshToken = newRefreshToken.CreatedRefreshToken;
            await SaveTokens(usuario);

            //Generamos un nuevo Json Web Token 😊
            datosUsuarioDto.EstaAutenticado = true;
            JwtSecurityToken jwtSecurityToken = CreateJwtToken(usuario);
            datosUsuarioDto.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            datosUsuarioDto.DisplayName = usuario.Email;
            datosUsuarioDto.Roles = usuario.Roles
                                            .Select(u => u.Nombre)
                                            .ToList(); //arreglo de roles
            datosUsuarioDto.RefreshToken = newRefreshToken.RefreshToken;
            datosUsuarioDto.RefreshTokenExpiration = newRefreshToken.ExpireTimeRefreshToken;

            return datosUsuarioDto;
        }



    }
}
