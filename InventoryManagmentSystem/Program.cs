
using Microsoft.EntityFrameworkCore;
using InventoryManagmentSystem.Data;
using InventoryManagmentSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagmentSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Concetion to database 
            builder.Services.AddDbContext<InventoryManagmentContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Write something about this service
            builder.Services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<InventoryManagmentContext>()
                    .AddDefaultTokenProviders();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}