using Microsoft.EntityFrameworkCore;
using PncNext.Infrastructure.Persistence;
using PncNext.Domain.Interfaces;
using PncNext.Infrastructure.Motion.Services;
using PncNext.Infrastructure.Motion.Transports;
using PncNext.Infrastructure.Motion.Protocols.PA;
using PncNext.Infrastructure.Motion.Protocols.INTH;
using PncNext.Infrastructure.Motion.Protocols.Dummy;
using PncNext.API.Services;
using PncNext.Infrastructure.SharedMemory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Shared Memory Service (Singleton for IPC)
builder.Services.AddSingleton<SharedMemoryService>();

// --- Dynamic Motion Control Registration (Factory Pattern) ---
builder.Services.AddScoped<IMotionControl>(sp =>
{
    var dbContext = sp.GetRequiredService<AppDbContext>();
    
    // DB에서 활성화된(IsActive) 제어기 설정을 읽어옵니다.
    var config = dbContext.MotionControllerConfigs.FirstOrDefault(c => c.IsActive);

    // 설정이 없을 경우의 기본값 (Fallback)
    if (config == null)
    {
        return new PAMotionControlService(
            new TcpCommPort("127.0.0.1", 5000), 
            new PAMotionProtocol());
    }

    // 1. 통신 방식(Transport) 결정
    ICommPort commPort = config.CommType switch
    {
        "Ethernet" => new TcpCommPort(config.IPAddress ?? "127.0.0.1", config.Port ?? 5000),
        "Serial" => new SerialCommPort(config.ComPort ?? "COM1", config.BaudRate ?? 9600),
        _ => new TcpCommPort("127.0.0.1", 5000)
    };

    // 2. 프로토콜(Protocol) 결정
    IMotionProtocol protocol = config.ProtocolProvider switch
    {
        "PAMotionProtocol" => new PAMotionProtocol(),
        "INTHMotionProtocol" => new INTHMotionProtocol(),
        "Dummy" => new DummyProtocol(),
        _ => new DummyProtocol()
    };

    // 3. 서비스 구현체 결정
    // ControllerName에 INTH가 포함되어 있거나 특정 조건에 따라 서비스를 선택합니다.
    if (config.ControllerName.Contains("INTH"))
    {
        return new INTHMotionControlService(commPort, protocol);
    }
    else
    {
        return new PAMotionControlService(commPort, protocol);
    }
});

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
