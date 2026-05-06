using System.Runtime.InteropServices;

namespace PncNext.Domain.Models
{
    /// <summary>
    /// 공유 메모리(Memory-Mapped File)에 저장될 모션 데이터 구조체.
    /// WPF 클라이언트 등과 고속으로 데이터를 공유하기 위해 Blittable 타입으로 구성합니다.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MotionSharedData
    {
        // 축별 현재 위치 (6축 기준)
        public double PositionX;
        public double PositionY;
        public double PositionZ;
        public double PositionA;
        public double PositionB;
        public double PositionC; 
    }
}
