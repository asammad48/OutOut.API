using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OutOut.Constants.Errors;
using OutOut.Core;
using OutOut.Helpers.Extensions;
using OutOut.Helpers.Middleware;
using OutOut.Infrastructure;
using OutOut.Models;
using OutOut.Models.Identity;
using OutOut.Persistence;
using OutOut.Persistence.Identity.Stores;
using OutOut.ViewModels.Wrappers;
using Serilog;
using Hangfire;
using System.Text;
using OutOut.Helpers.Authorization;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using OutOut.Constants;
using MongoDB.Bson.Serialization;
using OutOut.Persistence.Extensions;
using OutOut.Infrastructure.Services;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace OutOut
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
            BsonSerializer.RegisterSerializer(new AvailableTimeSerializer());
            BsonSerializer.RegisterSerializer(new EventOccurrenceTimeSerializer());
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration);
            var appSettings = Configuration.Get<AppSettings>();

            services.AddHttpContextAccessor();

            services.AddSignalR(config => config.EnableDetailedErrors = true);

            // Add HTTP Clients here

            services.AddPersistence(appSettings);
            services.AddCore();
            services.AddInfraStructure(appSettings);
            services.AddAutomapper(appSettings);

            //MongoDb Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;

            }).AddRoleStore<RoleStore<ApplicationRole>>()
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole>>()
            .AddUserManager<UserManager<ApplicationUser>>()
            .AddRoleManager<RoleManager<ApplicationRole>>()
            .AddDefaultTokenProviders();

            services.AddControllers();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromDays(2);
            });

            services.AddSingleton<IAuthenticationSchemeProvider, CustomAuthenticationSchemeProvider>();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errorsList = new List<string>();
                    foreach (var model in actionContext.ModelState)
                    {
                        foreach (var error in model.Value.Errors)
                        {
                            errorsList.Add(error.ErrorMessage);
                        }
                    }
                    var failedOperationResult = new FailedOperationResult<string>((int)ErrorCodes.ValidationErrors, ErrorCodes.ValidationErrors.ToMessage(), errorsList);
                    return new BadRequestObjectResult(failedOperationResult);
                };
            });

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.AppSecrets.JWTSecretKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, config =>
                   {
                       config.RequireHttpsMetadata = false;
                       config.SaveToken = true;
                       config.TokenValidationParameters = new TokenValidationParameters
                       {
                           ValidateIssuerSigningKey = true,
                           IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.AppSecrets.JWTSecretKey)),
                           ValidateIssuer = false,
                           ValidateAudience = false
                       };

                       config.BackchannelHttpHandler = WebHostEnvironment.IsProduction() ? new HttpClientHandler() : new HttpClientHandler
                       {
                           ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                       };

                       if (WebHostEnvironment.IsDevelopment())
                       {
                           config.TokenValidationParameters.ClockSkew = TimeSpan.FromDays(10);
                       }

                       config.Events = new JwtBearerEvents
                       {
                           OnMessageReceived = context =>
                           {
                               var accessToken = context.Request.Query["access_token"];

                               var path = context.HttpContext.Request.Path;
                               if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/NotificationHub"))
                               {
                                   context.Token = accessToken;
                               }

                               return Task.CompletedTask;
                           }
                       };
                   });

            services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.Only_SuperAdmin, builder => builder.AddRoleRequirement(Roles.SuperAdmin));
                options.AddPolicy(Policies.Only_VenueAdmins, builder => builder.AddRoleRequirement(Roles.VenueAdmin));
                options.AddPolicy(Policies.Only_EventAdmins, builder => builder.AddRoleRequirement(Roles.EventAdmin));
                options.AddPolicy(Policies.SuperAdmin_Or_VenueAdmins, builder => builder.AddRoleRequirement(Roles.SuperAdmin, Roles.VenueAdmin));
                options.AddPolicy(Policies.SuperAdmin_Or_EventAdmins, builder => builder.AddRoleRequirement(Roles.SuperAdmin, Roles.EventAdmin));
                options.AddPolicy(Policies.Admins, builder => builder.AddRoleRequirement(Roles.SuperAdmin, Roles.VenueAdmin, Roles.EventAdmin));
            });

            var migrationOptions = new MongoMigrationOptions { MigrationStrategy = new DropMongoMigrationStrategy(), BackupStrategy = new NoneMongoBackupStrategy() };
            var storageOptions = new MongoStorageOptions { MigrationOptions = migrationOptions };
            //services.AddHangfire(config =>
            //{
            //    config.UseMongoStorage(appSettings.Connections.NonSqlHangfireConnectionString, storageOptions);
            //});

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OutOut", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {{
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                }});
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<AppSettings> appSettings)
        {
            AppSettings _appSettings = appSettings.Value;

            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }

            //Swagger in non production environments
            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OutOut API v1");
                });
            }

            app.ConfigureExceptionHandler();

            app.UseStaticFiles();

            app.UseWhen(context => context.Request.Path.Value.StartsWith("/api"), subBranch =>
            {
                subBranch.UseAuthenticationOverride(JwtBearerDefaults.AuthenticationScheme);
            });

            app.UseHttpsRedirection();



            app.UseSerilogRequestLogging();

            app.UseRouting();

            if (env.IsProduction())
            {
                app.UseCors(x =>
                {
                    x.WithOrigins(_appSettings.FrontendOrigin)
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .AllowAnyHeader();
                });
            }
            else
            {
                app.UseCors(x =>
                {
                    x.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            }


            app.UseAuthentication();
            app.UseUserDetailsMiddleware();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "api/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapDefaultControllerRoute();

                endpoints.MapHub<NotificationHub>("/NotificationHub", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                });

                if (env.IsDevelopment())
                    endpoints.MapControllers().WithMetadata(new AllowAnonymousAttribute());
            });

            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthFilter() },
            }
            );
        }
    }
}
