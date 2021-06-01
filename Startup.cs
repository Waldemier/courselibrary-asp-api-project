using System;
using System.Linq;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace CourseLibrary.API
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
            services.AddControllers(setupAction =>
            {
                // By default is false
                setupAction.ReturnHttpNotAcceptable = true; // Adding supported the 406 http status if requested 'Accept' type unsupported or not indicated.
            })
                .AddXmlDataContractSerializerFormatters() // Adding support the xml format.
                .ConfigureApiBehaviorOptions(setupAction => // Adding additional information for errors message
                {
                    setupAction.InvalidModelStateResponseFactory = ActionContext =>
                    {
                        // create a problem details object
                        var problemDetailsFactory = ActionContext.HttpContext.RequestServices
                            .GetRequiredService<ProblemDetailsFactory>();
                        var problemDetails =
                            problemDetailsFactory.CreateValidationProblemDetails(ActionContext.HttpContext,
                                ActionContext.ModelState);
                        
                        // add additional info not added by default
                        problemDetails.Detail = "See the errors field for details.";
                        problemDetails.Instance = ActionContext.HttpContext.Request.Path;
                        
                        // find out which status code to use
                        var actionExecutingContext =
                            ActionContext as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;
                        
                        // if there are modelstate errors & all arguments were correctly 
                        // found / parsed we are dealing with validation errors
                        if ((ActionContext.ModelState.ErrorCount > 0) &&
                            (actionExecutingContext?.ActionArguments.Count ==
                             ActionContext.ActionDescriptor.Parameters.Count))
                        {
                            problemDetails.Type = "http://courselibrary.com/modelvalidationproblem"; // For example
                            problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                            problemDetails.Title = "One or more validation errors occurred.";

                            return new UnprocessableEntityObjectResult(problemDetails)
                            {
                                ContentTypes = { "application/problem+json" }
                            };
                        }
                        
                        // if one of the arguments was not correctly found  / could not be parsed
                        // we are dealing with null/unparseable input
                        problemDetails.Status = StatusCodes.Status400BadRequest;
                        problemDetails.Title = "One or more errors on input occurred.";
                        
                        return new BadRequestObjectResult(problemDetails)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };
                });
            
            //Повернення:
            //Масив збірок у цьому домені програми.
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); //Аргумент дає додаткові переваги при скануванні проекту на знаходження Профілів

            services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

            services.AddDbContext<CourseLibraryDbContext>(options => 
                options.UseSqlServer(   
        @"Server=WIN-OBDH18C5VTL;Database=CourseLibrary.API.DB;Trusted_Connection=True;") 
            );
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "CourseLibrary.API", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CourseLibrary.API v1"));
            }
            else
            {
                // Error handler for production mode.
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again latest.");
                    });
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}