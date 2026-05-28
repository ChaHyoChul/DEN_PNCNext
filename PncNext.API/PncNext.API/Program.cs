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

// --- Core Services (Singletons) ---
builder.Services.AddSingleton<SharedMemoryService>();
builder.Services.AddSingleton<IMotionStateStore, MotionStateStore>();
builder.Services.AddSingleton<IMotionConfigStore, MotionConfigStore>();

// --- Dynamic Motion Control Registration (Changed to Singleton for Hardware Persistence) ---
builder.Services.AddSingleton<IMotionControl>(sp =>
{
    // Singleton 서비스에서 Scoped 서비스(AppDbContext)를 참조하기 위해 임시 스코프 생성
    using (var scope = sp.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stateStore = sp.GetRequiredService<IMotionStateStore>();
        
        // DB에서 활성화된 제어기 설정을 채널(CommItems) 정보와 함께 읽어옵니다.
        var config = dbContext.MotionControllerConfigs
            .Include(c => c.CommItems)
            .FirstOrDefault(c => c.IsActive);

        // 설정이 없을 경우의 기본값 (Fallback)
        if (config == null || !config.CommItems.Any())
        {
            var fallbackChannels = new Dictionary<string, IMotionChannel> {
                //{ "STS", new MotionChannel(new TcpCommPort("127.0.0.1", 10000), new PAMotionProtocol()) },
                //{ "CMD", new MotionChannel(new TcpCommPort("127.0.0.1", 10100), new PAMotionProtocol()) },
                //{ "ATL", new MotionChannel(new TcpCommPort("127.0.0.1", 10200), new PAMotionProtocol()) }
                { "STS", new MotionChannel(new TcpCommPort("192.6.94.1", 10000), new PAMotionProtocol()) },
                { "CMD", new MotionChannel(new TcpCommPort("192.6.94.1", 10100), new PAMotionProtocol()) },
                { "ATL", new MotionChannel(new TcpCommPort("192.6.94.1", 10200), new PAMotionProtocol()) }
            };
            return new PAMotionControlService(fallbackChannels, stateStore);
        }

        // 모든 CommItems로부터 프로토콜이 내장된 채널 맵 생성
        var channelMap = new Dictionary<string, IMotionChannel>();
        foreach (var item in config.CommItems)
        {
            ICommPort port = item.CommType switch
            {
                "Ethernet" => new TcpCommPort(item.IPAddress ?? "127.0.0.1", item.Port ?? 5000),
                "Serial" => new SerialCommPort(item.ComPort ?? "COM1", item.BaudRate ?? 9600),
                _ => new TcpCommPort("127.0.0.1", 5000)
            };

            IMotionProtocol protocol = item.ProtocolProvider switch
            {
                "PAMotionProtocol" => new PAMotionProtocol(),
                "INTHMotionProtocol" => new INTHMotionProtocol(),
                "Dummy" => new DummyProtocol(),
                _ => new DummyProtocol()
            };

            channelMap[item.Purpose] = new MotionChannel(port, protocol);
        }

        // 제어기 타입에 따른 서비스 반환
        if (config.ControllerName.Contains("INTH"))
        {
            return new INTHMotionControlService(channelMap, stateStore);
        }
        else
        {
            return new PAMotionControlService(channelMap, stateStore);
        }
    }
});

// Background Monitoring
builder.Services.AddHostedService<MotionStatusBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
