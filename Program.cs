using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProyectoSonia.Models;
using static Dropbox.Api.TeamLog.EventCategory;

var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddTransient<MySqlConnection>(_ => new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

//var mysqlConfiguration = new MySQLConfiguration(builder.Configuration.GetConnectionString("Default"));
//builder.Services.AddSingleton(mysqlConfiguration);

//builder.Services.AddSingleton(new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddSingleton(mysqlConfiguration);

builder.Services.AddDbContext<SoniaproyectContext>(options =>
       options.UseMySql(builder.Configuration.GetConnectionString("Default"), Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.23-mysql")));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
