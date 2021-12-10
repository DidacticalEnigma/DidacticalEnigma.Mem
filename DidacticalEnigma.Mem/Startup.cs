using System.IO;
using System.Text.Json.Serialization;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Authentication;
using DidacticalEnigma.Mem.Configurations;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation;
using DidacticalEnigma.Mem.Translation.Categories;
using DidacticalEnigma.Mem.Translation.Contexts;
using DidacticalEnigma.Mem.Translation.Projects;
using DidacticalEnigma.Mem.Translation.Translations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using NMeCab;
using OpenIddict.Abstractions;
using Quartz;
using Swashbuckle.AspNetCore.Filters;

namespace DidacticalEnigma.Mem
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                .AddApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "DidacticalEnigma.Mem",
                    Version = "v1",
                    Description = 
@"Simple translation memory server

A single project contains many translation units, each one has source text and target text, and may have an associated context with it. Context stores a piece of textual or binary data, or both.

Each translation unit has a correlation id, which can store an identifier, unique to the project, which can be used to correlate a specific translation unit with an external resource or database."
                });
                c.EnableAnnotations();
                
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = HeaderNames.Authorization,
                    Type = SecuritySchemeType.ApiKey
                });
                
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                
                var filePath = Path.Combine(System.AppContext.BaseDirectory, "DidacticalEnigma.Mem.xml");
                c.IncludeXmlComments(filePath);
            });

            var databaseConfigurationSection = Configuration.GetSection("DatabaseConfiguration");
            services.Configure<DatabaseConfiguration>(databaseConfigurationSection);
            var databaseConfiguration = databaseConfigurationSection.Get<DatabaseConfiguration>();
            
            var mecabConfigurationSection = Configuration.GetSection("MeCabConfiguration");
            services.Configure<MeCabConfiguration>(mecabConfigurationSection);
            var mecabConfiguration = mecabConfigurationSection.Get<MeCabConfiguration>();
            
            var authConfigurationSection = Configuration.GetSection("AuthConfiguration");
            services.Configure<AuthConfiguration>(authConfigurationSection);
            var authConfiguration = authConfigurationSection.Get<AuthConfiguration>();

            services.AddSingleton<IMorphologicalAnalyzer<IpadicEntry>>(provider => new MeCabIpadic(new MeCabParam()
            {
                UseMemoryMappedFile = true,
                DicDir = mecabConfiguration.PathToDictionary
            }));

            services.AddSingleton<ICurrentTimeProvider, CurrentTimeProvider>();

            services.AddScoped<ITranslationMemory, TranslationMemory>();
            
            services.AddDbContext<MemContext>(options =>
            {
                options.UseNpgsql(databaseConfiguration.ConnectionString);
                
                options.UseOpenIddict();
            });

            services
                .AddDefaultIdentity<User>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<MemContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
            });

            services.AddScoped<AddTranslations>();
            services.AddScoped<QueryTranslations>();
            services.AddScoped<GetContexts>();
            services.AddScoped<DeleteContext>();
            services.AddScoped<UpdateTranslation>();
            services.AddScoped<AddProject>();
            services.AddScoped<AddContext>();
            services.AddScoped<GetContextData>();
            services.AddScoped<DeleteTranslation>();
            services.AddScoped<DeleteProject>();
            services.AddScoped<QueryCategories>();
            services.AddScoped<AddCategories>();
            services.AddScoped<DeleteCategory>();
            services.AddScoped<ListProjects>();
            
            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
                options.UseSimpleTypeLoader();
                options.UseInMemoryStore();
            });
            
            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "ReadTranslations",
                    policy =>
                        policy.Requirements.Add(
                            new CompositeOrRequirement(
                                new JwtPermissionRequirement("read:translations"),
                                new AuthConfigurationPolicyRequirement(config => config.AnonymousUsersCanReadTranslations))));
                
                options.AddPolicy(
                    "EnumerateProjects",
                    policy =>
                        policy.Requirements.Add(
                            new CompositeOrRequirement(
                                new JwtPermissionRequirement("read:listOfProjects"))));
                
                options.AddPolicy(
                    "ModifyTranslations",
                    policy =>
                        policy.Requirements.Add(
                            new CompositeOrRequirement(
                                new JwtPermissionRequirement("modify:translations"))));
                
                options.AddPolicy(
                    "ModifyProjects",
                    policy =>
                        policy.Requirements.Add(
                            new CompositeOrRequirement(
                                new JwtPermissionRequirement("modify:projects"))));
                
                options.AddPolicy(
                    "ModifyContexts",
                    policy =>
                        policy.Requirements.Add(
                            new CompositeOrRequirement(
                                new JwtPermissionRequirement("modify:contexts"))));
                
                options.AddPolicy(
                    "ModifyCategories",
                    policy =>
                        policy.Requirements.Add(
                            new CompositeOrRequirement(
                                new JwtPermissionRequirement("modify:categories"))));
                
                options.AddPolicy(
                    "ReadContexts",
                    policy => policy.Requirements.Add(
                        new CompositeOrRequirement(
                            new JwtPermissionRequirement("read:contexts"),
                            new AuthConfigurationPolicyRequirement(config => config.AnonymousUsersCanReadContexts))));
            });
            
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and models.
                    // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
                    options.UseEntityFrameworkCore()
                        .UseDbContext<MemContext>();

                    // Enable Quartz.NET integration.
                    options.UseQuartz();
                })
                .AddServer(options =>
                {
                    options.DisableAccessTokenEncryption();

                    options
                        .SetAuthorizationEndpointUris("/connect/authorize")
                        .SetLogoutEndpointUris("/connect/logout")
                        .SetTokenEndpointUris("/connect/token")
                        .SetUserinfoEndpointUris("/connect/userinfo");
                    
                    options.RegisterScopes(
                        OpenIddictConstants.Scopes.Email,
                        OpenIddictConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Roles);
                    
                    options
                        .AllowAuthorizationCodeFlow()
                        .AllowRefreshTokenFlow();
                    
                    options
                        .AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                    
                    options
                        .UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableStatusCodePagesIntegration()
                        .EnableTokenEndpointPassthrough()
                        .DisableTransportSecurityRequirement();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            services
                .AddRazorPages()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddSingleton<IAuthorizationHandler, MemAuthorizationHandler>();
            
            services.AddHostedService<Worker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DidacticalEnigma.Mem V1");
                c.RoutePrefix = "Api";
            });
            
            app.UseStaticFiles();
            
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}