using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Infrastructure.Services;

namespace FoodConnect.Backend.Infrastructure.Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var apiProjectName = "FoodConnect.Backend.API";
            var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", apiProjectName);

            var builder = new ConfigurationBuilder();
            var configRoot = builder
                .SetBasePath(apiProjectPath) 
                .AddJsonFile("appsettings.json") 
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .AddUserSecrets(Assembly.Load(apiProjectName))
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = configRoot.GetConnectionString("DefaultConnection");

            optionsBuilder.UseNpgsql(connectionString, o => o.UseVector());

            return new AppDbContext(optionsBuilder.Options, currentUserService:null);
        }
    }
}
