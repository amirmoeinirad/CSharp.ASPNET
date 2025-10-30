using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyApp.Infrastructure.Data;
using NodaTime;
using RabbitMQ.Client;
using System.Data;
using System.Text;

namespace MyApp.Web
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // WEB APPLICATION BUILDER
            // CreateBuilder() returns a WebApplicationBuilder instance.
            // WebApplicationBuilder instance is used to configure a web application.
            var builder = WebApplication.CreateBuilder(args);


            // CONFIGURATION
            // Access the configuration settings in appsettings.json.
            var configuration = builder.Configuration;


            //////////////
            // SERVICES //
            //////////////


            // AUTOFAC
            // Use Autofac as the DI container instead of the default Microsoft.Extensions.DependencyInjection.
            // Services are registered by default in the IServiceCollection interface.
            // But, when the application is built, Autofac will be used as the service provider.
            // Autofac will take over and integrate those registrations.
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());


            // EF CORE
            // Add the ApplicationDbContext to the DI container.
            // This enables the use of Entity Framework Core for database operations.
            // EF Core connection type? Scoped.
            // Why scoped for EF Core? Because DbContext is designed to be used per request.
            builder.Services.AddDbContext<ApplicationDbContext>(opt =>
                opt.UseSqlServer(configuration.GetConnectionString("Default")));


            // DAPPER
            // Register IDbConnection with a transient lifetime, which means
            // a new instance will be created each time it is requested.
            // IDbConnection is implemented by SqlConnection for SQL Server.
            // It is part of ADO.NET used for database operations.
            // Why transient for Dapper? Because database connections are typically short-lived.
            // Why don't we use a singleton service for database connections? 
            // Because a singleton would share the same connection instance across multiple requests,
            // which can lead to concurrency issues and connection state problems.
            // For example, if one request closes the connection, it would affect other requests using the same connection.
            builder.Services.AddTransient<IDbConnection>(sp =>
                new SqlConnection(configuration.GetConnectionString("Default")));


            // RABBITMQ
            // Register ConnectionFactory as a singleton service.
            builder.Services.AddSingleton(sp =>
            {
                var cfg = configuration.GetSection("RabbitMQ");
                var factory = new ConnectionFactory { HostName = cfg["Host"] ?? "", UserName = cfg["User"] ?? "", Password = cfg["Password"] ?? "" };
                return factory;
            });


            // JWT AUTHENTICATION
            var jwt = configuration.GetSection("Jwt");
            // Convert the secret key string in configuration to a byte array.
            var key = Encoding.UTF8.GetBytes(jwt["Secret"] ?? "");
            // Configure authentication services to use JWT Bearer tokens.
            builder.Services.AddAuthentication(options =>
            {
                // Set the default authentication scheme to JWT Bearer.
                // Authentication scheme is a way to identify how the user will be authenticated.
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                // Set the default challenge scheme to JWT Bearer.
                // A challenge is issued when authentication is required but not provided.
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // In production, this should be true to ensure tokens are only sent over HTTPS.
                options.SaveToken = true; // Save the token in the AuthenticationProperties after a successful authorization.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwt["Issuer"],
                    ValidAudience = jwt["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key) // Use the secret key for signing the token.
                };
            });


            // NODA TIME
            // AddSingleton means that a single instance of IClock will be created and shared
            // throughout the application's lifetime.
            builder.Services.AddSingleton<IClock>(SystemClock.Instance);


            // MEDIATR
            // RegisterServicesFromAssemblyContaining<>() scans the assembly containing the specified type (Program in this case).
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());


            // AUTOMAPPER
            // AddAutoMapper() scans the assembly containing the specified type (Program in this case).
            // In simple terms, it tells AutoMapper to look for mapping profiles in the same assembly as the Program class.
            builder.Services.AddAutoMapper(typeof(Program));


            // FLUENT VALIDATION
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();


            // MVC PATTERN
            builder.Services.AddControllersWithViews();


            // RAZOR PAGES
            // Add Razor Pages services to the DI container.
            // All services are registered in the IServiceCollection interface.
            builder.Services.AddRazorPages();


            // WEB APPLICATION
            // Build the configured WebApplication instance.
            var app = builder.Build();


            ////////////////
            // MIDDLEWARE //
            ////////////////
            

            if (!app.Environment.IsDevelopment())
            {
                // EXCEPTION HANDLING
                // Configure a middleware to handle exceptions.
                app.UseExceptionHandler("/Error");

                // HTTP STRICT TRANSPORT SECURITY (HSTS)
                // HSTS is a web security policy mechanism that helps to protect
                // websites against protocol downgrade attacks and cookie hijacking.
                app.UseHsts();
            }


            // HTTPS REDIRECTION
            // Redirect HTTP requests to HTTPS.
            app.UseHttpsRedirection();


            // ROUTING
            // Enable routing capabilities.
            app.UseRouting();


            // AUTHENTICATION
            // Enable authorization capabilities.
            app.UseAuthorization();


            // STATIC FILES
            // Serve static files from the wwwroot folder.
            app.MapStaticAssets();


            // ENDPOINTS
            // Map Razor Pages endpoints.
            app.MapRazorPages().WithStaticAssets();


            ///////////////
            // Execution //
            ///////////////


            // RUN
            // Run the web application.
            app.Run();
        }
    }
}