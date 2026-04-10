using Microsoft.EntityFrameworkCore;
using WebApiRRHH.Context;
using WebApiRRHH.Repositories;
using WebApiRRHH.Repositories.Interfaces;
using WebApiRRHH.Services;

var builder = WebApplication.CreateBuilder(args);

//Agregar la configuracion de UserSecrets para manejar la cadena de conexion a la base de datos de forma segura
builder.Configuration.AddUserSecrets<Program>();

// Add services to the container.

// Crear variiable para la cadena de conexion a la base de datos
var connectionString = builder.Configuration.GetConnectionString("SqlServer");

//Registrar nuestro servicio para la conexion a la base de datos

builder.Services.AddDbContext<AppDBContext>(
    options => options.UseSqlServer(connectionString)
);

// Repositorios
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Servicios
builder.Services.AddScoped<IUserService, UserService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") 
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();                    
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsProduction())
{
    // En producción, configurar niveles más restrictivos
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
}

//builder.Services.AddHealthChecks()
//    .AddDbContextCheck<AppDBContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
