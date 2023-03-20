using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QQEgg_Backend.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string MyAllowOrigins = "AllowAny";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowOrigins, policy => policy.WithOrigins("*").WithHeaders("*").WithMethods("*"));
});

builder.Services.AddControllers();
builder.Services.AddDbContext<dbXContext>(
 options => options.UseSqlServer(
 builder.Configuration.GetConnectionString("dbXConnection")
));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
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
app.UseStaticFiles();
app.UseRouting();

//app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// �[�J TokenMiddleware �����n��
app.UseMiddleware<TokenMiddleware>();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.MapControllers();

app.Run();


