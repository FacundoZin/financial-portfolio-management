using api.Application.Interfaces.Infrastructure.Auth;
using api.Application.Interfaces.Infrastructure.BackgrounServices;
using api.Application.Interfaces.Infrastructure.FMP_Client;
using api.Application.Interfaces.Infrastructure.Identity;
using api.Application.Interfaces.Infrastructure.Reposiories;
using api.Application.Interfaces.UseCases;
using api.Application.UseCases;
using api.Domain.Entities;
using api.Infrastructure.Auth;
using api.Infrastructure.BackgroundServices;
using api.Infrastructure.HostedServices;
using api.Infrastructure.Identity;
using api.Infrastructure.Persistence.Data;
using api.Infrastructure.Persistence.Repository;
using api.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;


var builder = WebApplication.CreateBuilder(args);

// DEBUG: Verificamos el entorno y si se est� leyendo correctamente la clave JWT
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"JWT Key: {builder.Configuration["JWT:Key"] ?? "NO DEFINIDA"}");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDBcontext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IHoldingRepository, HoldingRepository>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();


builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IHoldingService, HoldingService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IaccountService, AccountService>();

builder.Services.AddSingleton<IBackgroundTaskQueue, TaskQueue>();
builder.Services.AddHostedService<QueueHostedService>();

builder.Services.AddScoped<IFMPService, FMP_Client>();
builder.Services.AddHttpClient<IFMPService, FMP_Client>();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

builder.Services.AddIdentity<AppUser, IdentityRole>(Options =>
{
    Options.Password.RequireDigit = true;
    Options.Password.RequireLowercase = true;
    Options.Password.RequireUppercase = true;
    Options.Password.RequireNonAlphanumeric = true;
    Options.Password.RequiredLength = 12;
}).AddEntityFrameworkStores<ApplicationDBcontext>();

builder.Services.AddAuthentication(Options =>
{
    Options.DefaultAuthenticateScheme =
    Options.DefaultChallengeScheme =
    Options.DefaultForbidScheme =
    Options.DefaultScheme =
    Options.DefaultSignInScheme =
    Options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(Options =>
{
    Options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])
        )
    };
});

// Config RabbitMQ
var factory = new ConnectionFactory
{
    HostName = builder.Configuration["RabbitMQ:Host"],
    UserName = builder.Configuration["RabbitMQ:User"],
    Password = builder.Configuration["RabbitMQ:Password"]
};
var connection = await factory.CreateConnectionAsync();
builder.Services.AddSingleton<IConnection>(connection);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(x => x
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()
      //.WithOrigins("https://localhost:44351))
      .SetIsOriginAllowed(origin => true));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

