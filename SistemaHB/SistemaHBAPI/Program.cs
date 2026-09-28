using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SistemaHBAPI.Models;
using SistemaHBAPI.Services;

namespace SistemaHBAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            // 1. Configurar el esquema de autenticación JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "https://localhost:7176",
                        ValidAudience = "SistemaHBAutenticacionApi",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TuClaveSecretaSuperSeguraDeAlMenos32Bytes!"))
                    };
                });

            // 2. Configurar los servicios de Autorización
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Configuración de la conexión a MySQL/MariaDB en el puerto 3307
            var connectionString = "server=localhost;port=3307;database=sistemahb;uid=root;password=";

            builder.Services.AddDbContext<SistemaHBContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            //builder.Services.AddHostedService<FirebaseSincronizadorWorker>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("PermitirTodo", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // 1. Primero se mapea el endpoint que genera el JSON
                app.MapOpenApi();

                // 2. Luego se inicializa Scalar apuntando explícitamente a ese documento JSON
                app.MapScalarApiReference(options =>
                {
                    options.WithOpenApiRoutePattern("/openapi/v1.json");
                });
            }

            app.UseHttpsRedirection();

            // CORS siempre debe ejecutarse antes de la autenticación para permitir pre-requests (OPTIONS)
            app.UseCors("PermitirTodo");

            // 3. Middlewares de seguridad indispensables en orden estricto
            app.UseAuthentication(); // Primero identifica quién es el usuario (Verifica el Token JWT)
            app.UseAuthorization();  // Luego verifica si tiene permisos de acceso

            app.MapControllers();

            app.Run();
        }
    }
}
