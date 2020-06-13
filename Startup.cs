using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HPlusSport.API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HPlusSport.API
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
            services.AddDbContext<ShopContext>(Options=> Options.UseInMemoryDatabase("Shop"));
            //services.AddControllers();
            services.AddControllers() //This code automatically prevent application from giving automatically error message when argument prodided by the user in URL is not valid.
                .ConfigureApiBehaviorOptions(Options =>
                {
                    //Options.SuppressModelStateInvalidFilter = true;
                }
                );

            //protecting api by identity server
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options=>
                {
                    options.Authority = "http://localhost:58750"; //port of the identity server
                    options.RequireHttpsMetadata = false;

                    options.Audience = "hps-api";
                });

            //Configure CORS so that Web Api can be accessed by Web application. this are default policies
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder => 
                {
                    //here you can add more policies as you wish and provide unique name for each of them, or use default polcy, you dont have to provide name in default policies
                    builder.WithOrigins("https://localhost:44348") //here you specify the port of the project you allow to access this API, in our case port of HPlusSport.Web project in its lunchSetting.json folder
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                });
            });


            //Configuring URL versioning.
            services.AddApiVersioning(Options => {
                Options.ReportApiVersions = true;
                Options.DefaultApiVersion = new ApiVersion(1, 0);
                Options.AssumeDefaultVersionWhenUnspecified = true;
                //Options.ApiVersionReader = new HeaderApiVersionReader("X-API-Version"); //this line was for header vesrion, To implement Query string version remove this line only and access the version using https://localhost:44388/products?api-versio=2.0.
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection(); //force the user to use https, this is added autometically when yoy tick Configure for HTTP when you start to create project 

            app.UseRouting();

            app.UseCors(); //you can pass name if you want example app.UseCors("my-policy");, but here we leave it as default policy

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
