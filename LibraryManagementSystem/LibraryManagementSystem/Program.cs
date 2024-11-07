using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register your custom DbContext for the library database
            builder.Services.AddDbContext<LibraryDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionString")));

            // Register Identity with your custom User class and IdentityRole
            builder.Services.AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedEmail  =false) // Using your custom User model
                .AddEntityFrameworkStores<LibraryDbContext>()
                .AddDefaultTokenProviders();
    


            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Add MVC controllers with views
            builder.Services.AddControllersWithViews(); // This is for MVC, not Razor Pages

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // Ensure Authentication is enabled
            app.UseAuthorization();


            // Set up the routes for MVC controllers (without Razor Pages)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
