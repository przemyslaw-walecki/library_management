using LibraryManagementSystem.Data;
using LibraryManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace LibraryManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddDbContext<LibraryDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionString")));

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = ".LibraryManagementSystem.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(10);
            });

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddHostedService<ExpiredReservationCleaner>();

            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            });
            builder.Services.AddEndpointsApiExplorer();

            SwaggerService.AddSwagger(builder.Services);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (context.Request.Cookies.TryGetValue("AuthToken", out var token))
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        }
                    };
        });

            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    policy => policy.WithOrigins("http://localhost:3000")
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials());
            });


            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("AllowReactApp");

            SwaggerService.UseSwagger(app);

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseSession();
            app.UseAuthorization();
         
            app.MapControllers();

            app.Run();
        }
    }
}
