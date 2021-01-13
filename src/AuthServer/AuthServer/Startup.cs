using AuthServer.Extensions;
using AuthServer.Infrastructure.Data.Identity;
using AuthServer.Infrastructure.Services;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Net;
 


namespace AuthServer
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
            services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default"), o => o.MigrationsAssembly("AuthServer.Infrastructure")));

            //services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
            //services.AddAuthentication("idsrv")
            //    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            //    {
            //        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            //        {
            //            ValidateActor = false,
            //            ValidateAudience = false,
            //            ValidateIssuer = false,
            //            ValidateIssuerSigningKey = false,
            //            ValidateLifetime = false,
            //            ValidateTokenReplay = false,
            //        };
            //    });

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "https://localhost:44348";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "resourceapi";
                });

            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(Configuration.GetConnectionString("Default"));
                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30; // interval in seconds
                })
                //.AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddAspNetIdentity<AppUser>();

                /* We'll play with this down the road... 
                    services.AddAuthentication()
                    .AddGoogle("Google", options =>
                    {
                        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                        options.ClientId = "<insert here>";
                        options.ClientSecret = "<insert here>";
                    });*/

            services.AddTransient<IProfileService, IdentityClaimsProfileService>();

            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()));

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            }).SetCompatibilityVersion(CompatibilityVersion.Latest)
            .AddNewtonsoftJson();

            services.AddSingleton<IdentityModel.Client.IDiscoveryCache>((ser) =>
            {
                return new IdentityModel.Client.DiscoveryCache("https://localhost:44348", null);
            });

            //services
            //    .Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            //    {
            //        options.Audience = Configuration["JwtIssuerOptions:Audience"];
            //        options.ClaimsIssuer = Configuration["JwtIssuerOptions:Issuer"];
            //        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            //        {
            //            ValidateAudience = true,
            //            ValidAudience = Configuration["JwtIssuerOptions:Audience"],
            //            ValidateIssuer = true,
            //            ValidIssuer = Configuration["JwtIssuerOptions:Issuer"],
            //            ValidateIssuerSigningKey = true,
            //            IssuerSigningKey = new Func<Microsoft.IdentityModel.Tokens.SymmetricSecurityKey>(() =>
            //            {
            //                var key = Configuration["JwtIssuerOptions:Key"];
            //                var bytes = System.Text.Encoding.UTF8.GetBytes(key);
            //                var signing_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(bytes);
            //                return signing_key;
            //            }).Invoke(),
            //            ValidateLifetime = true
            //        };
            //        options.SaveToken = false;
            //    })
            //    .Configure<Models.JwtIssuerOptions>(options =>
            //    {
            //        options.Audience = Configuration["JwtIssuerOptions:Audience"];
            //        options.Issuer = Configuration["JwtIssuerOptions:Issuer"];
            //        options.Key = Configuration["JwtIssuerOptions:Key"];
            //        options.Subject = Configuration["JwtIssuerOptions:Subject"];
            //        options.ValidFor = Configuration.GetValue<TimeSpan>("JwtIssuerOptions:ValidFor");
            //    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        context.Response.AddApplicationError(error.Error.Message);
                        await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                    }
                });
            });

            var serilog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(@"authserver_log.txt");

            loggerFactory.WithFilter(new FilterLoggerSettings
                {
                    { "IdentityServer4", LogLevel.Debug },
                    { "Microsoft", LogLevel.Warning },
                    { "System", LogLevel.Warning },
                }).AddSerilog(serilog.CreateLogger());

            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});
        }
    }
}
