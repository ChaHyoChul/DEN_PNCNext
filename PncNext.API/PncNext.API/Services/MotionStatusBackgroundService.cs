using PncNext.Domain.Interfaces;

namespace PncNext.API.Services
{
    /// <summary>
    /// 장비의 상태를 주기적으로 모니터링하고, 단절 시 자동 재연결을 수행하는 백그라운드 서비스
    /// </summary>
    public class MotionStatusBackgroundService : BackgroundService
    {
        private readonly ILogger<MotionStatusBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private DateTime _lastRetryTime = DateTime.MinValue;
        private readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(5);

        public MotionStatusBackgroundService(
            ILogger<MotionStatusBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Motion Status Monitoring Service is starting.");

            // [추가] 초기화 가속: 서비스 시작 직후 최초 1회 명시적 연결 시도
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var motionControl = scope.ServiceProvider.GetRequiredService<IMotionControl>();
                    await motionControl.OpenAsync();
                    _logger.LogInformation("Initial connection attempt completed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Initial connection attempt failed: {ex.Message}");
            }

            // 100ms 주기의 고성능 타이머 (.NET 8)
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var motionControl = scope.ServiceProvider.GetRequiredService<IMotionControl>();

                        continue;
                        
                        // 1. 현재 상태 조회
                        var status = await motionControl.GetStatusAsync();

                        // 2. 미연결 상태일 경우 자동 재연결 시도
                        if (status == MotionStatus.NotConnected)
                        {
                            if (DateTime.Now - _lastRetryTime > _retryInterval)
                            {
                                _logger.LogWarning("장비 연결 단절 감지. 재연결을 시도합니다...");
                                _lastRetryTime = DateTime.Now;

                                try
                                {
                                    // 기존 채널들을 닫고 새로 오픈 시도 (MotionChannel 내부에서 멱등성 처리됨)
                                    await motionControl.OpenAsync();
                                    _logger.LogInformation("장비 재연결 성공.");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"재연결 시도 실패: {ex.Message}");
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // 서비스 종료 시 무시
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "상태 모니터링 루프 중 예기치 않은 오류 발생.");
                }
            }

            _logger.LogInformation("Motion Status Monitoring Service is stopping.");
        }
    }
}
