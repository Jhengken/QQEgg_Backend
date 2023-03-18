using Microsoft.EntityFrameworkCore;
using QQEgg_Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

string MyAllowOrigins = "AllowAny";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowOrigins, policy => policy.WithOrigins("").WithHeaders("").WithMethods("*"));
});

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
app.UseCors(MyAllowOrigins);
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
