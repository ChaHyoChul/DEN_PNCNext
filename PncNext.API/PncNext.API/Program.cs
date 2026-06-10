using Microsoft.EntityFrameworkCore;
using PncNext.Infrastructure.Persistence;
using PncNext.Domain.Interfaces;
using PncNext.Infrastructure.Motion.Services;
using PncNext.Infrastructure.Motion.Transports;
using PncNext.Infrastructure.Motion.Channels;
using PncNext.Infrastructure.SharedMemory;
using PncNext.Infrastructure.Services;
using PncNext.API.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. DB Context 설정
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. 모션 제어 관련 싱글톤 서비스 등록
builder.Services.AddSingleton<IMotionStateStore, MotionStateStore>();
builder.Services.AddSingleton<IMotionConfigStore, MotionConfigStore>();
builder.Services.AddSingleton<SharedMemoryService>();

// 2.1. 도메인 로직 서비스 등록
builder.Services.AddSingleton<ISignalRService, SignalRServiceStub>();
builder.Services.AddScoped<IDiskManagementService, DiskManagementService>();
builder.Services.AddScoped<INcFileService, NcFileService>();

// 3. 동적 채널 및 컨트롤 서비스 팩토리 등록
builder.Services.AddSingleton<IMotionControl>(sp =>
{
    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
    using var scope = scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var stateStore = sp.GetRequiredService<IMotionStateStore>();

    // 활성화된 제어기 설정 로드
    var config = dbContext.MotionControllerConfigs
        .Include(c => c.CommItems)
        .FirstOrDefault(c => c.IsActive);

    if (config == null) throw new InvalidOperationException("Active MotionController configuration not found.");

    // 채널 생성 (포트 + 전용 채널 클래스 결합)
    var channels = new Dictionary<string, IMotionChannel>();
    foreach (var item in config.CommItems)
    {
        ICommPort port = item.CommType switch
        {
            "TCP" or "Ethernet" => new TcpCommPort(item.IPAddress ?? "127.0.0.1", item.Port ?? 5000),
            "SERIAL" or "Serial" => new SerialCommPort(item.ComPort ?? "COM1", item.BaudRate ?? 115200),
            _ => throw new NotSupportedException($"Unsupported comm type: {item.CommType}")
        };

        // 제어기 타입에 따른 전용 채널 객체 생성
        IMotionChannel channel = config.ControllerType switch
        {
            "PA" => new PAMotionChannel(port),
            "INTH" => new INTHMotionChannel(port),
            "DUMMY" => new DummyMotionChannel(port),
            _ => throw new NotSupportedException($"Unsupported controller type: {config.ControllerType}")
        };
        
        channels[item.Purpose] = channel;
    }

    // 제어 서비스 생성
    return config.ControllerType switch
    {
        "PA" => new PAMotionControlService(channels, stateStore),
        "INTH" => new INTHMotionControlService(channels, stateStore),
        _ => throw new NotSupportedException($"Unsupported control service for: {config.ControllerType}")
    };
});

// 4. 백그라운드 상태 폴링 서비스 등록
builder.Services.AddHostedService<MotionStatusBackgroundService>();

// 5. NC 파일 자동 감지 서비스 등록
builder.Services.AddHostedService<NcFileWatcherService>();

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
