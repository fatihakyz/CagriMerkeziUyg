using Microsoft.EntityFrameworkCore;
using CagriMerkeziUyg.Data;
using CagriMerkeziUyg.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CagriMerkeziUyg
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Loglama seviyesini artır
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            
            // Entity Framework DbContext ekle
            builder.Services.AddDbContext<CagriMerkeziDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            // Excel Export Service ekle
            builder.Services.AddScoped<SimpleExcelExportService>();
            
            // Operatör Durum Yönetimi Service ekle
            builder.Services.AddScoped<OperatorDurumService>();

            // Session servisi ekle
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(8);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            // Authentication servislerini ekle
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => 
                    policy.RequireRole("Admin"));
                
                options.AddPolicy("AdminOrSupervisor", policy => 
                    policy.RequireRole("Admin", "Supervisor"));
                
                options.AddPolicy("AllOperators", policy => 
                    policy.RequireRole("Admin", "Supervisor", "Operator"));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Session middleware (Authentication'dan önce olmalı)
            app.UseSession();

            // Authentication & Authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            // Global authentication check middleware
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value?.ToLower() ?? "";
                Console.WriteLine($"=== MIDDLEWARE === Path: {path}, Method: {context.Request.Method}");
                
                // Auth sayfaları ve static dosyalar için kontrol atlama
                if (path.StartsWith("/auth/") || 
                    path.StartsWith("/css/") || 
                    path.StartsWith("/js/") || 
                    path.StartsWith("/lib/") ||
                    path.StartsWith("/favicon"))
                {
                    Console.WriteLine($"Middleware: Skipping auth check for {path}");
                    await next();
                    return;
                }
                
                // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
                if (!context.User.Identity?.IsAuthenticated ?? true)
                {
                    Console.WriteLine($"Middleware: User not authenticated, redirecting to login");
                    context.Response.Redirect("/Auth/Login");
                    return;
                }
                
                Console.WriteLine($"Middleware: User authenticated, continuing");
                await next();
            });

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Login}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
