using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PostService.Data;

namespace PostService
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PostService", Version = "v1" });
            });

            services.AddDbContext<PostServiceContext>(options =>
                 options.UseSqlite(@"Data Source=post.db"));

            services.AddRouting();
            

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, PostServiceContext dbContext)
        {

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                dbContext.Database.EnsureCreated();
            }
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
}
