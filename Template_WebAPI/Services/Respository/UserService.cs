using Microsoft.EntityFrameworkCore;
using Template_WebAPI.DTOs;
using Template_WebAPI.Entities.Users;
using Template_WebAPI.Helpers.AuthTools;
using Template_WebAPI.Services.Interfaces;

namespace Template_WebAPI.Services.Respository
{
    public class UserService : IUserService
    {

        private readonly HttpContext httpContext;
        private readonly ApplicationDbContext context;
        private readonly DapperContext dapperContext;

        public UserService(IConfiguration configuration, ApplicationDbContext context, DapperContext dapperContext, IHttpContextAccessor httpContextAccessor/*Este método esta configurado en program*/)
        {
            httpContext = httpContextAccessor.HttpContext;
            this.context = context;
            this.dapperContext = dapperContext;
        }

        public async Task<Usuario> ValidateUserAsync(UserCredentials userCredentials)
        {
            //using var connection = dapperContext.CreateConnection();
            //var data = await connection.QueryFirstOrDefaultAsync<Usuario>("SELECT * FROM  DBJWTTest.TI.Usuario WHERE Email = @Email and Password = @Password and Activo = 1",
            //    userCredentials);
            var data = await context.Usuarios
                .Include(x => x.Roles)
                .Where(x => x.Email == userCredentials.Email && x.Password == userCredentials.Password && x.Activo).FirstOrDefaultAsync();

            return data;
        }


        public async Task<string> RegisterAsync(RegisterDTO registerDto)
        {
            //Asignación información del usuario
            var usuario = new Usuario
            {
                Nombres = registerDto.Nombres,
                ApellidoMaterno = registerDto.ApellidoMaterno,
                ApellidoPaterno = registerDto.ApellidoPaterno,
                Email = registerDto.Email,
                UserName = registerDto.Username,
                Password = registerDto.Password
            };

            //Validar Usuario existe username en DB
            var usuarioExiste = await context.Usuarios.Where(x => x.Email.ToLower() == registerDto.Email.ToLower()).FirstOrDefaultAsync();

            if (usuarioExiste == null)
            {
                //buscamos el rol predeterminado
                var rolPredeterminado = await context.Roles
                                        .Where(u => u.Nombre == Autorizacion.rol_predeterminado.ToString())
                                        .FirstAsync();
                try
                {
                    //agregamos el rol de la colección
                    usuario.Roles.Add(rolPredeterminado);
                    context.Usuarios.Add(usuario);
                    await context.SaveChangesAsync();

                    return $"El usuario {registerDto.Username} ha sido registrado exitosamente";
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                    return $"Error: {message}";
                }
            }
            else
            {
                return $"El usuario con {registerDto.Username} ya se encuentra registrado.";
            }
        }

        public async Task revokeRefreshToken()
        {
            var usuario = await context.Usuarios.Where(x => x.Id == 2).FirstOrDefaultAsync();
            usuario.RefreshToken = null;
            usuario.CreatedRefreshToken = null;
            usuario.ExpireTimeRefreshToken = null;
            context.Usuarios.Update(usuario);
            await context.SaveChangesAsync();

        }

        public async Task<Usuario> GetByRefreshTokenAsync(string refreshToken)
        {
            return await context.Usuarios
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public async Task<Usuario> GetByUserNameAsync(string username)
        {
            return await context.Usuarios
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());
        }


    }
}
