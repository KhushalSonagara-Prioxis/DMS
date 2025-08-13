using System.Reflection;
using DMS.API.Helper;
using DMS.Model.SpDbContext;
using DMS.Models.Models.MyDoctorsDB;
using DMS.Service.Repository.Implementation;
using DMS.Service.Repository.Interface;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DMS.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("DBConnection");
        
        //SPCONTEXT CONFIGURATION
        builder.Services.AddDbContext<DoctorManagementSpContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
            {

                sqlOptions.EnableRetryOnFailure();

            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(true);
        }, ServiceLifetime.Transient);
        
        // Register the UserService AND Unit OF Work
        UnitOfWorkServiceCollectionExtentions.AddUnitOfWork<DoctorsDbContext>(builder.Services);
        
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        builder.Services.AddControllers()
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()));
        
        // prevent from duplicate logging
        builder.Logging.ClearProviders();
        // Serilog configuration to add logs in a new file
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();


        builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
        
        
        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        
        builder.Services.AddDbContext<DoctorsDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseMiddleware<Middleware>();
        
        app.MapControllers();

        app.Run();
    }
}