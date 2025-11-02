using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization;
using OutOut.Constants;
using OutOut.Constants.Errors;
using OutOut.Core;
using OutOut.Core.BackgroundServices;
using OutOut.Core.Services;
using OutOut.Helpers;
using OutOut.Helpers.Authorization;
using OutOut.Helpers.Extensions;
using OutOut.Helpers.Middleware;
using OutOut.Infrastructure;
using OutOut.Infrastructure.Services;
using OutOut.Models;
using OutOut.Models.Identity;
using OutOut.Models.Utils;
using OutOut.Persistence;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Identity.Stores;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Wrappers;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/outout-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

//------------------------------------------------------
// 1. App configuration & logging
//------------------------------------------------------
builder.WebHost.UseUrls("http://0.0.0.0:8080"); // Needed for Docker
builder.Host.UseSerilog();

builder.Services.Configure<AppSettings>(builder.Configuration);
var appSettings = builder.Configuration.Get<AppSettings>() ?? new AppSettings();

//------------------------------------------------------
// 2. Core Services Registration
//------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR(c => c.EnableDetailedErrors = true);
builder.Services.AddPersistence(appSettings);
builder.Services.AddCore();
builder.Services.AddInfraStructure(appSettings);
builder.Services.AddAutomapper(appSettings);

//------------------------------------------------------
// 3. Mongo Identity
//------------------------------------------------------
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
{
    opts.SignIn.RequireConfirmedAccount = true;
    opts.User.RequireUniqueEmail = true;
    opts.Password.RequiredLength = 8;
    opts.Password.RequireLowercase = true;
    opts.Password.RequireUppercase = true;
    opts.Password.RequireNonAlphanumeric = true;
    opts.Password.RequireDigit = true;
})
.AddRoleStore<RoleStore<ApplicationRole>>()
.AddUserStore<UserStore<ApplicationUser, ApplicationRole>>()
.AddUserManager<UserManager<ApplicationUser>>()
.AddRoleManager<RoleManager<ApplicationRole>>()
.AddDefaultTokenProviders();

builder.Services.AddControllers();

// Token lifespan
builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
{
    opt.TokenLifespan = TimeSpan.FromDays(2);
});

builder.Services.AddSingleton<IAuthenticationSchemeProvider, CustomAuthenticationSchemeProvider>();

builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.InvalidModelStateResponseFactory = ctx =>
    {
        var errors = ctx.ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
        var result = new FailedOperationResult<string>(
            (int)ErrorCodes.ValidationErrors,
            ErrorCodes.ValidationErrors.ToMessage(),
            errors);
        return new BadRequestObjectResult(result);
    };
});

//------------------------------------------------------
// 4. Authentication / Authorization
//------------------------------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, config =>
    {
        config.RequireHttpsMetadata = false;
        config.SaveToken = true;
        config.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(appSettings.AppSecrets.JWTSecretKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        config.BackchannelHttpHandler = builder.Environment.IsProduction()
            ? new HttpClientHandler()
            : new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

        if (builder.Environment.IsDevelopment())
            config.TokenValidationParameters.ClockSkew = TimeSpan.FromDays(10);

        config.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/NotificationHub"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy(Policies.Only_SuperAdmin, p => p.AddRoleRequirement(Roles.SuperAdmin));
    opts.AddPolicy(Policies.Only_VenueAdmins, p => p.AddRoleRequirement(Roles.VenueAdmin));
    opts.AddPolicy(Policies.Only_EventAdmins, p => p.AddRoleRequirement(Roles.EventAdmin));
    opts.AddPolicy(Policies.SuperAdmin_Or_VenueAdmins, p => p.AddRoleRequirement(Roles.SuperAdmin, Roles.VenueAdmin));
    opts.AddPolicy(Policies.SuperAdmin_Or_EventAdmins, p => p.AddRoleRequirement(Roles.SuperAdmin, Roles.EventAdmin));
    opts.AddPolicy(Policies.Admins, p => p.AddRoleRequirement(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin));
});

//------------------------------------------------------
// 5. Hangfire
//------------------------------------------------------
var migrationOptions = new MongoMigrationOptions
{
    MigrationStrategy = new DropMongoMigrationStrategy(),
    BackupStrategy = new NoneMongoBackupStrategy()
};
var storageOptions = new MongoStorageOptions { MigrationOptions = migrationOptions };
builder.Services.AddHangfire(cfg =>
    cfg.UseMongoStorage(appSettings.Connections.NonSqlHangfireConnectionString, storageOptions));

//------------------------------------------------------
// 6. Swagger
//------------------------------------------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OutOut", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.\r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

//------------------------------------------------------
// 7. Hosted & background services
//------------------------------------------------------
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddHostedService<ReminderService>();
builder.Services.AddHostedService<UnholdEventTicketsService>();

//------------------------------------------------------
// 8. Build the app
//------------------------------------------------------
var app = builder.Build();

// Bson serializers (Mongo)
BsonSerializer.RegisterSerializer(new AvailableTimeSerializer());
BsonSerializer.RegisterSerializer(new EventOccurrenceTimeSerializer());

//------------------------------------------------------
// 9. Middleware pipeline
//------------------------------------------------------
if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OutOut API v1"));
}

app.ConfigureExceptionHandler();
app.UseStaticFiles();

app.UseWhen(ctx => ctx.Request.Path.Value?.StartsWith("/api") == true,
    branch => branch.UseAuthenticationOverride(JwtBearerDefaults.AuthenticationScheme));

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseRouting();

if (app.Environment.IsProduction())
{
    app.UseCors(x => x.WithOrigins(appSettings.FrontendOrigin)
        .AllowAnyMethod()
        .AllowCredentials()
        .AllowAnyHeader());
}
else
{
    app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}

app.UseAuthentication();
app.UseUserDetailsMiddleware();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "api/{controller=Home}/{action=Index}/{id?}");

    endpoints.MapHub<NotificationHub>("/NotificationHub", options =>
    {
        options.Transports = HttpTransportType.WebSockets;
    });

    if (app.Environment.IsDevelopment())
        endpoints.MapControllers().WithMetadata(new AllowAnonymousAttribute());
});

app.UseHangfireServer();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthFilter() },
});

//------------------------------------------------------
// 🔥 Initialize Firebase / Hangfire recurring jobs
//------------------------------------------------------
try
{
    var sp = app.Services.CreateScope().ServiceProvider;
    var environment = sp.GetRequiredService<IWebHostEnvironment>();

    UAEDateTime.InitializeUAEDateTime(sp.GetRequiredService<IOptions<AppSettings>>());

    if (!environment.IsProduction())
        IdentityModelEventSource.ShowPII = true;

    var googleCredentialFile = Path.Combine(environment.ContentRootPath, appSettings.FCMConfigurations.ClientConfigurationFileName);
    if (File.Exists(googleCredentialFile))
    {
        var credential = GoogleCredential.FromFile(googleCredentialFile);
        FirebaseApp.Create(new AppOptions { Credential = credential });
    }

    RecurringJob.AddOrUpdate<NotificationService>(
        "NotificationService",
        x => x.SendUseAppReminder(),
        "0 16 * * *");
}
catch (Exception ex)
{
    Log.Error(ex, "Startup initialization failed.");
}

//------------------------------------------------------
// ✅ 10. Run the app — keeps container alive
//------------------------------------------------------
try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
