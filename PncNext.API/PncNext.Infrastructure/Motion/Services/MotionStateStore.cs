using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;
using PncNext.Infrastructure.SharedMemory;

namespace PncNext.Infrastructure.Motion.Services
{
    /// <summary>
    /// 장비의 최신 상태 데이터를 메모리에 유지하는 싱글톤 저장소 구현체.
    /// 변경 발생 시 공유 메모리(MMF)를 업데이트합니다.
    /// </summary>
    public class MotionStateStore : IMotionStateStore
    {
        private readonly SharedMemoryService _sharedMemoryService;
        public PAMotionControllerState PaState { get; } = new PAMotionControllerState();

        public MotionStateStore(SharedMemoryService sharedMemoryService)
        {
            _sharedMemoryService = sharedMemoryService;
        }

        public void NotifyStateChanged(string controllerType)
        {
            // 1. 통합 공유 데이터 구조체 생성
            var sharedData = new MotionSharedData();

            // 2. 제어기 타입에 따른 데이터 정규화 및 매핑
            if (controllerType == "PA")
            {
                sharedData.PositionX = PaState.Position[0];
                sharedData.PositionY = PaState.Position[1];
                sharedData.PositionZ = PaState.Position[2];
                sharedData.PositionA = PaState.Position[3];
                sharedData.PositionB = PaState.Position[4];
                sharedData.PositionC = PaState.Position[5];
            }
            // else if (controllerType == "INTH") { ... }

            // 3. 공유 메모리에 기록 (WPF UI용)
            _sharedMemoryService.WriteMotionData(sharedData);
        }
    }
}
