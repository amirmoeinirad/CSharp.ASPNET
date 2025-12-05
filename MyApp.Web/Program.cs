using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyApp.Application.Interfaces;
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
            // 'UseServiceProviderFactory': This is a method that allows you to override the mechanism used to create
            // the final 'IServiceProvider' (the service locator that resolves dependencies at runtime).
            // 'AutofacServiceProviderFactory': This component acts as a bridge, taking all the service registrations
            // made with the standard .NET 'IServiceCollection' and populating them into an Autofac ContainerBuilder.
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());


            // AUTOFAC REGISTRATIONS
            // The ConfigureContainer<>() method is used to configure the Autofac container.
            // This method is part of .NET's generic host builder.
            // The ContainerBuilder class is provided by Autofac and is used to register services.
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                // Here, we are registering two implementations of IPersonRepository in the Autofac container.
                // We are not using the default ASP.NET IServiceCollection registrations for these repositories.
                // (1)
                // PersonRepositoryEF is registered with InstancePerLifetimeScope(),
                // which means a new instance will be created for each lifetime scope.
                // In web applications, a lifetime scope typically corresponds to a single HTTP request.
                containerBuilder.RegisterType<PersonRepositoryEF>().As<IPersonRepository>().InstancePerLifetimeScope();
                // (2)
                // PersonRepositoryDapper is registered with InstancePerDependency(),
                // which means a new instance will be created each time it is requested.
                containerBuilder.RegisterType<PersonRepositoryDapper>().InstancePerDependency();

                // RegisterType, InstancePerLifetimeScope and InstancePerDependency methods are
                // all part of Autofac's API for configuring service lifetimes.
            });


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
            // Authentication is the process of verifying the identity of a user or system.
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


            // AUTHORIZATION
            // Authorization is the process of determining what an authenticated user is allowed to do.
            // Configure authorization policies.
            // Here, we define a policy named "AdminOnly" that requires the user to have a claim "role" with the value "admin".
            // 'role' must be defined when the JWT token is created.
            // It is defined as a claim in the token payload.
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireClaim("role", "admin"));
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
            // Enable authentication capabilities.
            app.UseAuthentication();


            // AUTHORIZATION
            // Enable authorization capabilities.
            app.UseAuthorization();


            // STATIC FILES
            // Used in Minimal APIs or Blazor to map static assets explicitly to routes.
            // Minimal API is a lightweight way to build HTTP APIs with ASP.NET Core.          
            app.MapStaticAssets();


            // STATIC FILES
            // Serve static files from the wwwroot folder for MVC apps.
            app.UseStaticFiles();


            // ENDPOINTS
            // Map Razor Pages endpoints.
            // This will allow the application to respond to requests for Razor Pages.
            // Razor Pages are a page-based programming model for ASP.NET Core MVC.
            app.MapRazorPages().WithStaticAssets();


            // MAP CONTROLLER ROUTE
            // The MapControllerRoute method defines a route for MVC controllers.
            // The route template "{controller=Home}/{action=Index}/{id?}" means:
            // - "controller=Home": If no controller is specified in the URL, use "Home" as the default controller.
            // - "action=Index": If no action is specified in the URL, use "Index" as the default action method.
            // - "id?": The "id" parameter is optional (indicated by the "?").
            // This is an example of a conventional routing pattern in ASP.NET Core MVC,
            // not using attribute routing or a separate routing class or controller.
            app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");


            // MINIMAL API ENDPOINT
            // Map a simple GET endpoint that returns a string.
            // In minimal APIs, we don't use controllers.
            app.MapGet("/hello", () => "Hello from Minimal API!");


            ///////////////
            // Execution //
            ///////////////


            // RUN
            // Run the web application.
            app.Run();
        }
    }
}