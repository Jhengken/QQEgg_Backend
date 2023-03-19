using Microsoft.EntityFrameworkCore;
using QQEgg_Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the DI container.
string dbXConnectionString = builder.Configuration.GetConnectionString("dbXConnection");
builder.Services.AddDbContext<dbXContext>(options => { 
    options.UseSqlServer(connectionString: dbXConnectionString);
});

builder.Services.AddControllers();

builder.Services.AddDbContext<dbXContext>(
options => options.UseSqlServer(
builder.Configuration.GetConnectionString("dbXConnection")
));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
