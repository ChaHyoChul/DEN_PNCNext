using Microsoft.EntityFrameworkCore;
using PncNext.Infrastructure.Persistence;
using PncNext.Domain.Interfaces;
using PncNext.Infrastructure.Motion.Services;
using PncNext.Infrastructure.Motion.Transports;
using PncNext.Infrastructure.Motion.Protocols.PA;
using PncNext.Infrastructure.Motion.Protocols.INTH;
using PncNext.Infrastructure.Motion.Protocols.Dummy;
using PncNext.Infrastructure.Motion.Channels;
using PncNext.API.Services;
using PncNext.Infrastructure.SharedMemory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Shared Memory Service (Singleton for IPC)
builder.Services.AddSingleton<SharedMemoryService>();

// --- Dynamic Motion Control Registration (Channel-based Architecture) ---
builder.Services.AddScoped<IMotionControl>(sp =>
{
    var dbContext = sp.GetRequiredService<AppDbContext>();
    
    // DB에서 활성화된 제어기 설정을 채널(CommItems) 정보와 함께 읽어옵니다.
    var config = dbContext.MotionControllerConfigs
        .Include(c => c.CommItems)
        .FirstOrDefault(c => c.IsActive);

    // 설정이 없을 경우의 기본값 (Fallback)
    if (config == null || !config.CommItems.Any())
    {
        var fallbackChannels = new Dictionary<string, IMotionChannel> {
            { "CMD", new MotionChannel(new TcpCommPort("127.0.0.1", 5000), new DummyProtocol()) },
            { "STS", new MotionChannel(new TcpCommPort("127.0.0.1", 5000), new DummyProtocol()) }
        };
        return new PAMotionControlService(fallbackChannels);
    }

    // 모든 CommItems로부터 프로토콜이 내장된 채널 맵 생성
    var channelMap = new Dictionary<string, IMotionChannel>();
    foreach (var item in config.CommItems)
    {
        // 1. 통신 포트 생성
        ICommPort port = item.CommType switch
        {
            "Ethernet" => new TcpCommPort(item.IPAddress ?? "127.0.0.1", item.Port ?? 5000),
            "Serial" => new SerialCommPort(item.ComPort ?? "COM1", item.BaudRate ?? 9600),
            _ => new TcpCommPort("127.0.0.1", 5000)
        };

        // 2. 해당 채널 전용 프로토콜 생성
        IMotionProtocol protocol = item.ProtocolProvider switch
        {
            "PAMotionProtocol" => new PAMotionProtocol(),
            "INTHMotionProtocol" => new INTHMotionProtocol(),
            "Dummy" => new DummyProtocol(),
            _ => new DummyProtocol()
        };

        // 3. 포트와 프로토콜을 하나로 묶어 채널로 생성
        channelMap[item.Purpose] = new MotionChannel(port, protocol);
    }

    // 4. 제어기 타입에 따른 서비스 반환
    if (config.ControllerName.Contains("INTH"))
    {
        return new INTHMotionControlService(channelMap);
    }
    else
    {
        return new PAMotionControlService(channelMap);
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
