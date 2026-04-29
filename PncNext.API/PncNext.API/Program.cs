using Microsoft.EntityFrameworkCore;
using PncNext.Infrastructure.Persistence;
using PncNext.Domain.Interfaces;
using PncNext.Infrastructure.Motion;
using PncNext.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Motion Control Strategy Pattern Registration
builder.Services.AddScoped<ICommPort>(sp => new TcpCommPort("127.0.0.1", 5000)); // Default for now
builder.Services.AddScoped<IMotionProtocol, DummyProtocol>();
builder.Services.AddScoped<IMotionControl, MotionControlService>();

// Background Monitoring
builder.Services.AddHostedService<MotionStatusBackgroundService>();

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
