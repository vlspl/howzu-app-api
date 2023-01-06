using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VLS_API.Model;
using VLS_API.Services;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Howzu_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("api", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Howzu API", Description = "Howzu Core API" });
                    var xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + @"Howzu_API.xml";
                    c.IncludeXmlComments(xmlPath);

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "JWT Authorization header using the Bearer scheme."
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
                         new string[] {}
                        }
                    
                     });

                });

            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // JWT Authentication

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Key);

            services.AddAuthentication(au =>
                {
                    au.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    au.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(jwt =>
                {
                    jwt.RequireHttpsMetadata = false;
                    jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    //ValidIssuer = "http://endpoint.visionarylifesciences.in/",
                    //ValidAudience = "http://endpoint.visionarylifesciences.in/",
                    ValidIssuer = "http://localhost:49930/",
                    ValidAudience = "http://localhost:49930/",

                    IssuerSigningKey = new SymmetricSecurityKey(key),
                };
                });
          //  services.AddMvc();
            services.AddMvc(options => options.EnableEndpointRouting = false);   // added for net 2.1 to net 3.1 migration 
            services.AddScoped<IAuthenticateService, AuthenticateService>();
            services.AddControllers().AddNewtonsoftJson(); // added for net 2.1 to net 3.1 migration 
            services.Configure<FormOptions>(o =>
            {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
        }
        //public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseExceptionHandler("/api/errors/200");
            app.UseStatusCodePagesWithReExecute("/api/errors/{0}");

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
                    RequestPath = new PathString("/Resources")
                });

            app.UseAuthentication();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/api/swagger.json", "Howzu API"); });

            app.UseCors(x => x
               .AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
        }
    }
}
