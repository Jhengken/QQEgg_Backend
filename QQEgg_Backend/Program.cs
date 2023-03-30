using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QQEgg_Backend.Models;
using QQEgg_Backend.Serivce;
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
builder.Services.AddHttpClient();
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
}).AddGoogle(options =>
{
    options.ClientId = builder.Configuration.GetValue<string>("Google:ClientId");
    options.ClientSecret = builder.Configuration.GetValue<string>("Google:ClientSecret");
}).AddFacebook(options =>
{
    options.ClientId = builder.Configuration.GetValue<string>("FacebookAuthSettings:AppId");
    options.ClientSecret = builder.Configuration.GetValue<string>("FacebookAuthSettings:AppSecret");
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<QrcodeService>();
builder.Services.AddScoped<ForgetPasswordSerivce>();

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

// 加入 TokenMiddleware 中介軟體
//app.UseMiddleware<TokenMiddleware>();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.MapControllers();

app.Run();


