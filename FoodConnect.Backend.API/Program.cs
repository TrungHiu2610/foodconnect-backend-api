using Amazon.S3;
using Amazon;
using FluentValidation;
using FoodConnect.Backend.API.Middlewares;
using FoodConnect.Backend.Application.Commons.Behaviors;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Options;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Infrastructure.Authentication;
using FoodConnect.Backend.Infrastructure.Persistence;
using FoodConnect.Backend.Infrastructure.Repositories;
using FoodConnect.Backend.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Amazon.Runtime;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
});

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();

var services = builder.Services;
services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          if (allowedOrigins != null && allowedOrigins.Length > 0)
                          {
                              policy.WithOrigins(allowedOrigins)
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials(); 
                          }
                      });
});

var configuration = builder.Configuration;

// DbContext  
services.AddDbContext<AppDbContext>(options =>
   options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// Authen JWT
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidAudience = configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"])),
            ClockSkew = TimeSpan.Zero
        };
    });

// AWS S3
services.Configure<AwsOptions>(configuration.GetSection("AWS"));
var awsConfig = configuration.GetSection("AWS").Get<AwsOptions>();

var credentials = new BasicAWSCredentials(awsConfig.AccessKey, awsConfig.SecretKey);
var region = RegionEndpoint.GetBySystemName(awsConfig.Region);
var awsOptions = new Amazon.Extensions.NETCore.Setup.AWSOptions
{
    Credentials = credentials,
    Region = region
};

services.AddDefaultAWSOptions(awsOptions);
services.AddAWSService<IAmazonS3>();
services.AddScoped<IFileStorageService, AwsS3FileStorageService>();

// Repositories 
services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<IProductAssetRepository, ProductAssetRepository>();
services.AddScoped<IShopRepository, ShopRepository>();
services.AddScoped<ICategoryRepository, CategoryRepository>();
services.AddScoped<ICartRepository, CartRepository>();
services.AddScoped<ICartItemRepository, CartItemRepository>();
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IOrderItemRepository, OrderItemRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services  
services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUserService, CurrentUserService>();

// MediatR  
services.AddMediatR(cfg =>
   cfg.RegisterServicesFromAssembly(Assembly.Load("FoodConnect.Backend.Application")));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddValidatorsFromAssembly(Assembly.Load("FoodConnect.Backend.Application"));

// Automapper IMapperConfigurationExpression
services.AddAutoMapper(cfg => { }, typeof(Program).Assembly, Assembly.Load("FoodConnect.Backend.Application"));

// Configure Form Options for large file uploads
services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

services.AddControllers();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

// Configure the HTTP request pipeline.  
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }