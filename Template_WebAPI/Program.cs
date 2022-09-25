using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;
using Template_WebAPI;
using Template_WebAPI.Extensions;
using Template_WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Iniciar serilog
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

// Configuraci�n de SeriLog
builder.Logging.AddSerilog(logger: logger);

// Configuraci�n de CORS
builder.Services.ConfigureCors();

// Configuraci�n de inyecci�n de Dependencias
builder.Services.AddAplicationServices();

// Configuraci�n de JsonWebToken
builder.Services.AddJwt(configuration: builder.Configuration);

// HTTP HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configurar limite de peticiones
builder.Services.ConfigureRateLimiting();

// Configuraci�n AutoMapper
//builder.Services.AddAutoMapper(Assembly.GetEntryAssembly());


builder.Services.AddControllers()
.AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

//es importante que esto este despu�s de controladores para el manejo de validaciones de model state
builder.Services.AddValidationErrors();

// TODO: Configuraci�n Base de datos Entity FrameWork
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuraci�n de la base de datos : Dapper
builder.Services.AddSingleton<DapperContext>();
 
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//MiddleWare para manejo de excepciones de forma global
app.UseMiddleware<ExceptionMiddleware>();
app.UseStatusCodePagesWithReExecute("/errors/{0}");//lamamos a un controlador  //nos permite generar paginas de error personalizadas cuando ocurre un error 
app.UseIpRateLimiting(); // Usar el middleware para limitar las peticiones


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("CorsPolicy");


app.UseHttpsRedirection();

//siempre debe ir antes que authorization
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
