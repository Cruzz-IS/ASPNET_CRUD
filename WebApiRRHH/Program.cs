using Microsoft.EntityFrameworkCore;
using WebApiRRHH.Context;

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

builder.Services.AddControllers();
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
