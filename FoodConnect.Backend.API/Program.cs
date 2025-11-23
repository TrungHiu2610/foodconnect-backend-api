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
using StackExchange.Redis;
using Resend;
using FoodConnect.Backend.Infrastructure.Hubs;
using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Features.Complaint.Services;
using FoodConnect.Backend.Application.Commons.Services;
using Hangfire;
using Hangfire.PostgreSql;
using FoodConnect.Backend.Application.Features.Promotion.Jobs;
using FoodConnect.Backend.Application.Features.Promotion.Services;
using FoodConnect.Backend.Application.Features.Complaint.Jobs;
using FoodConnect.Backend.Application.Features.Order.Jobs;
using FoodConnect.Backend.Application.Features.Order.Services;

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

services.AddHttpClient();

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

        // Configure JWT authentication for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
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

// Redis
services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var host = builder.Configuration["Redis:Host"];
    var port = builder.Configuration["Redis:Port"];

    var configuration = ConfigurationOptions.Parse($"{host}:{port}");
    configuration.User = builder.Configuration["Redis:Username"];
    configuration.Password = builder.Configuration["Redis:Password"];
    return ConnectionMultiplexer.Connect(configuration);
});

services.AddScoped<IRedisService, RedisService>();

// SMS Service
//services.AddScoped<ISmsService, AwsSnsService>();
services.AddScoped<ISmsService, SpeedSmsService>();
//services.AddScoped<ISmsService, VonageSMSService>();

// Email Service
services.AddOptions();
services.AddHttpClient<ResendClient>();
services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = configuration["Resend:ApiKey"]!;
});
services.AddTransient<IResend, ResendClient>();
services.AddScoped<IEmailService, ResendEmailService>();

// Google Auth Service
services.AddScoped<IGoogleAuthService, GoogleAuthService>();

// Firebase Auth Service
services.AddSingleton<FoodConnect.Backend.Application.Services.FirebaseService.IFirebaseAuthService, FoodConnect.Backend.Application.Services.FirebaseService.FirebaseAuthService>();

// Storage Service
services.AddDefaultAWSOptions(awsOptions);
services.AddAWSService<IAmazonS3>();
services.AddScoped<IFileStorageService, AwsS3FileStorageService>();

// Repositories 
services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<IProductAssetRepository, ProductAssetRepository>();
services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
services.AddScoped<IShopRepository, ShopRepository>();
services.AddScoped<ICategoryRepository, CategoryRepository>();
services.AddScoped<ICartRepository, CartRepository>();
services.AddScoped<ICartItemRepository, CartItemRepository>();
services.AddScoped<IAddressRepository, AddressRepository>();
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IOrderItemRepository, OrderItemRepository>();
services.AddScoped<INotificationRepository, NotificationRepository>();
services.AddScoped<IPromotionRepository, PromotionRepository>();
services.AddScoped<IPromotionProductRepository, PromotionProductRepository>();
services.AddScoped<IPromotionUsageRepository, PromotionUsageRepository>();
services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();
services.AddScoped<ISellerWalletRepository, SellerWalletRepository>();
services.AddScoped<ISellerWalletTransactionRepository, SellerWalletTransactionRepository>();
services.AddScoped<IWalletRepository, WalletRepository>();
services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
services.AddScoped<IWithdrawalRequestRepository, WithdrawalRequestRepository>();
services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
services.AddScoped<IOrderComplaintRepository, OrderComplaintRepository>();
services.AddScoped<IOrderComplaintAssetRepository, OrderComplaintAssetRepository>();
services.AddScoped<IConversationRepository, ConversationRepository>();
services.AddScoped<IMessageRepository, MessageRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services  
services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddScoped<IDistanceCalculatorService, DistanceCalculatorService>();
services.AddScoped<IShippingFeeCalculatorService, ShippingFeeCalculatorService>();
services.AddScoped<IVNPayService, VNPayService>();
services.AddScoped<WalletService>();

// SignalR & Notification Services
services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; 
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});
services.AddScoped<INotificationService, NotificationService>();
services.AddScoped<IChatNotificationService, ChatNotificationService>();
services.AddScoped<OrderNotificationService>();
services.AddScoped<PromotionNotificationService>();
services.AddScoped<ComplaintNotificationService>();

// Hangfire
services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => 
        options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))));

services.AddHangfireServer();
services.AddScoped<PromotionStatusJob>();
services.AddScoped<ComplaintEscalationJob>();
services.AddScoped<OrderAutoCompletionService>();
services.AddScoped<OrderStatusJob>();

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

app.UseWebSockets();

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

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Schedule recurring jobs
RecurringJob.AddOrUpdate<PromotionStatusJob>(
    "auto-activate-promotions",
    job => job.AutoActivatePromotionsAsync(),
    Cron.Minutely);

RecurringJob.AddOrUpdate<PromotionStatusJob>(
    "auto-expire-promotions",
    job => job.AutoExpirePromotionsAsync(),
    Cron.Minutely);

RecurringJob.AddOrUpdate<ComplaintEscalationJob>(
    "escalate-expired-complaints",
    job => job.EscalateExpiredComplaintsAsync(),
    Cron.Hourly);
// Order management jobs
RecurringJob.AddOrUpdate<OrderStatusJob>(
    "auto-cancel-unconfirmed-orders",
    job => job.AutoCancelUnconfirmedOrdersAsync(),
    Cron.Minutely);

RecurringJob.AddOrUpdate<OrderStatusJob>(
    "auto-complete-delivered-orders",
    job => job.AutoCompleteDeliveredOrdersAsync(),
    Cron.Minutely);

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
public partial class Program { }