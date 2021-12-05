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
                    options.SignIn.RequireConfirmedAccount = true;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<MemContext>()
                .AddTokenProvider<DataProtectorTokenProvider<User>>("DidacticalEnigma");

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

            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                        .UseDbContext<MemContext>();
                })
                .AddServer(options =>
                {
                    options.SetAuthorizationEndpointUris("/connect/authorize")
                        .SetLogoutEndpointUris("/connect/logout")
                        .SetTokenEndpointUris("/connect/token")
                        .SetUserinfoEndpointUris("/connect/userinfo");
                    options.AllowAuthorizationCodeFlow();

                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                    
                    options.UseAspNetCore()
                        .EnableTokenEndpointPassthrough();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    
                    options.UseAspNetCore();
                });;
            
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
            
            services
                .AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddRazorPages();
            
            services.AddSingleton<IAuthorizationHandler, MemAuthorizationHandler>();
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}