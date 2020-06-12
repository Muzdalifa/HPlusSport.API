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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
