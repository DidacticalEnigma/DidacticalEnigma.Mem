using System;
using System.IO;
using System.Text.Json.Serialization;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Configurations;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.Categories;
using DidacticalEnigma.Mem.Translation.Contexts;
using DidacticalEnigma.Mem.Translation.Projects;
using DidacticalEnigma.Mem.Translation.Translations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
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
using OpenIddict.Validation.AspNetCore;
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
            
            var quartzConfigurationSection = Configuration.GetSection("QuartzConfiguration");
            services.Configure<QuartzConfiguration>(quartzConfigurationSection);
            var quartzConfiguration = authConfigurationSection.Get<QuartzConfiguration>();
            
            var githubLoginProviderConfigurationSection = Configuration.GetSection("GithubLoginProviderConfiguration");
            services.Configure<GithubLoginProviderConfiguration>(githubLoginProviderConfigurationSection);
            var githubLoginProviderConfiguration = authConfigurationSection.Get<GithubLoginProviderConfiguration>();

            services.AddSingleton<IMorphologicalAnalyzer<IpadicEntry>>(provider => new MeCabIpadic(new MeCabParam()
            {
                UseMemoryMappedFile = true,
                DicDir = mecabConfiguration.PathToDictionary
            }));

            services.AddSingleton<ICurrentTimeProvider, CurrentTimeProvider>();

            services.AddDbContext<MemContext>(options =>
            {
                options.UseNpgsql(databaseConfiguration.ConnectionString);
                
                options.UseOpenIddict();
            });

            var authBuilder = services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                });

            if (githubLoginProviderConfiguration?.ClientId != null &&
                githubLoginProviderConfiguration?.ClientSecret != null)
            {
                authBuilder = authBuilder
                    .AddGitHub(options =>
                    {
                        options.ClientId = githubLoginProviderConfiguration.ClientId;
                        options.ClientSecret = githubLoginProviderConfiguration.ClientSecret;
                        options.SignInScheme = IdentityConstants.ExternalScheme;
                    });
            }

            authBuilder
                .AddIdentityCookies(o => { });

            services.AddIdentityCore<User>(options =>
                {
                    options.Stores.MaxLengthForKeys = 128;
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<MemContext>();

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
            });

            services.AddScoped<AddTranslationsHandler>();
            services.AddScoped<QueryTranslationsHandler>();
            services.AddScoped<GetContextsHandler>();
            services.AddScoped<DeleteContextHandler>();
            services.AddScoped<UpdateTranslationHandler>();
            services.AddScoped<AddProjectHandler>();
            services.AddScoped<AddContextHandler>();
            services.AddScoped<GetContextDataHandler>();
            services.AddScoped<DeleteTranslationHandler>();
            services.AddScoped<DeleteProjectHandler>();
            services.AddScoped<QueryCategoriesHandler>();
            services.AddScoped<AddCategoriesHandler>();
            services.AddScoped<DeleteCategoryHandler>();
            services.AddScoped<ListProjectsHandler>();
            services.AddScoped<ListInvitationsHandler>();
            services.AddScoped<SendInvitationHandler>();
            services.AddScoped<AcceptInvitationHandler>();
            services.AddScoped<RejectInvitationHandler>();
            services.AddScoped<CancelInvitationHandler>();
            services.AddScoped<QueryProjectShowcaseHandler>();

            if (quartzConfiguration.EnableQuartz)
            {
                services.AddQuartz(options =>
                {
                    options.UseMicrosoftDependencyInjectionJobFactory();
                    options.UseSimpleTypeLoader();
                    options.UseInMemoryStore();
                });
                
                services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
            }

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiAllowAnonymous",
                    new AuthorizationPolicy(
                        new []{ new AssertionRequirement(auth => true) },
                        new []{ OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme }));
                
                options.AddPolicy("ApiRejectAnonymous",
                    new AuthorizationPolicy(
                        new []{ new DenyAnonymousAuthorizationRequirement() },
                        new []{ OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme }));
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