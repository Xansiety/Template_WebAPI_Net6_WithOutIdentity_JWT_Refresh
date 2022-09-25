using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Template_WebAPI.Helpers.AuthTools;
using Template_WebAPI.Helpers.Errors;
using Template_WebAPI.Services.Interfaces;
using Template_WebAPI.Services.Respository;

namespace Template_WebAPI.Extensions
{
    public static class ApplicationServiceExtensions
    {
        // Configuración de CORS
        public static void ConfigureCors(this IServiceCollection services) =>
           services.AddCors(options =>
           {
               options.AddPolicy("CorsPolicy", builder =>
               builder
               .WithOrigins("http://127.0.0.1:5173", "*")
               //.AllowAnyOrigin() //
               .AllowAnyMethod() // With Methods("Get,Post")
               .AllowAnyHeader()
               .AllowCredentials()
               ); //WithHeaders("accept", "content-type") 
           });


        // Inyección de Dependencias
        public static void AddAplicationServices(this IServiceCollection services)
        {
            //services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            //services.AddScoped<IMarcaRepository, MarcaRepository>();
            //services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
        }

        // Rate Limit
        //Establecer un limite de peticiones 
        // https://github.com/stefanprodan/AspNetCoreRateLimit#readme
        public static void ConfigureRateLimiting(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddInMemoryRateLimiting();

            //configuración
            services.Configure<IpRateLimitOptions>(options =>
            {
                options.EnableEndpointRateLimiting = true;
                options.StackBlockedRequests = true;
                options.HttpStatusCode = 429; // cuando se sobrepase el limite de peticiones
                options.RealIpHeader = "X-Real-IP";  //encabezado que leeremos
                options.GeneralRules = new List<RateLimitRule>
            {
                new RateLimitRule {
                    Endpoint = "*", // a todos los endpoints
                    Period = "10s", // por un periodo de cada 10 segundos
                    Limit = 2 //un limite de 2 peticiones en ese intervalo
                }
            };
            });
        }



        //Validación de errores del modelState
        public static void AddValidationErrors(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {

                    var errors = actionContext.ModelState.Where(u => u.Value.Errors.Count > 0)
                                                    .SelectMany(u => u.Value.Errors)
                                                    .Select(u => u.ErrorMessage).ToArray();

                    var errorResponse = new ValidationAPI()
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(errorResponse);
                };
            });
        }


        // Json Web Token Configuración
        public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
            //configuración desde AppSettings y se lo asignamos a la clase rol
            services.Configure<JWT>(configuration.GetSection("JwtSettings"));

            //Se añade configuración Autentificación
            //método de extension que añade el método de autentificación
            services.AddAuthentication(options =>
            {
                //definimos el tipo de autentificación que necesitamos
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                //configuramos la información del token
                .AddJwtBearer(opt =>
                {
                    opt.RequireHttpsMetadata = false; //definimos si necesitamos una conexión https //e prod se debe pasar a true
                    opt.SaveToken = false;
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = configuration["JwtSettings:Issuer"], //asignamos los valores desde al appsetting
                        ValidAudience = configuration["JwtSettings:Audience"],//asignamos los valores desde al appsetting
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"])) //asignamos los valores desde al appsetting
                    };
                });
        }

    }
}
