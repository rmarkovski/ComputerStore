using ComputerStore.Application.Interfaces;
using ComputerStore.Application.Mapping;
using ComputerStore.Infrastructure.Data;
using ComputerStore.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Migration failed: {ex.Message}");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();

            builder.Services.AddAutoMapper(typeof(MappingProfile));

        }
    }
}
