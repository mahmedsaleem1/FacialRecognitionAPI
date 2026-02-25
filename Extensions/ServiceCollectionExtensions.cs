using FacialRecognitionAPI.Configuration;
using FacialRecognitionAPI.Data;
using FacialRecognitionAPI.Repositories;
using FacialRecognitionAPI.Repositories.Interfaces;
using FacialRecognitionAPI.Services;
using FacialRecognitionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FacialRecognitionAPI.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all application services, repositories, and configurations.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<EncryptionSettings>(configuration.GetSection(EncryptionSettings.SectionName));
        services.Configure<FaceRecognitionSettings>(configuration.GetSection(FaceRecognitionSettings.SectionName));
        services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
        services.Configure<AttendanceSettings>(configuration.GetSection("Attendance"));

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                }));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();

        // Services (singletons — stateless, thread-safe)
        services.AddSingleton<IFacialRecognitionService, FacialRecognitionService>();
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // Services (scoped — depend on DbContext / per-request state)
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        return services;
    }

    /// <summary>
    /// Configure CORS for React Native frontend.
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("ReactNativePolicy", builder =>
            {
                builder
                    .AllowAnyOrigin()  // React Native uses expo/device URLs
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Content-Disposition");
            });

            options.AddPolicy("ProductionPolicy", builder =>
            {
                builder
                    .WithOrigins("https://yourdomain.com") // Set your production domain
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
