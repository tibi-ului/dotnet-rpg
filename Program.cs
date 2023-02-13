global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;
global using dotnet_rpg.Models;
global using AutoMapper;
global using Microsoft.AspNetCore.Authorization;
global using System.Security.Claims;
global using dotnet_rpg.Data;
global using dotnet_rpg.Dtos;
global using dotnet_rpg.Dtos.Character;
global using dotnet_rpg.Dtos.User;
global using dotnet_rpg.Dtos.Weapon;
global using dotnet_rpg.Dtos.Skill;
global using dotnet_rpg.Dtos.Fight;


global using dotnet_rpg.Services.CharacterService;
global using dotnet_rpg.Services.WeaponsService;
global using dotnet_rpg.Services.FightService;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
// o componenta reutilizabila care ofera functionalitate aplicatiei
// creează canalul de procesare a cererilor din aplicații - cum raspunde aplicatia la http requsts
// Add services to the container.

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme {
        Description = """Standard Authorization header using the Bearer scheme. Example: "bearer {token}" """,
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<IFightService, FightService>();
builder.Services.AddScoped<IWeaponService, WeaponService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();   // inregistram AuthRepository
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
                ValidateIssuer = false,
                ValidateAudience = false
        };
    });
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
    app.UseSwagger();
    app.UseSwaggerUI();
}

// extension methods pentru a adauga componente 'middleware' la canalul de cereri
app.UseHttpsRedirection();    // face https redirection de la http
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
