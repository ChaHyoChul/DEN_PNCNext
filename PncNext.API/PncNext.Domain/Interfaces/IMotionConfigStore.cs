using PncNext.Domain.Models;

namespace PncNext.Domain.Interfaces
{
    /// <summary>
    /// 장비의 설정 데이터(옵셋, 티칭 등)를 관리하는 싱글톤 저장소 인터페이스
    /// </summary>
    public interface IMotionConfigStore
    {
        CoordinateOffsetData OffsetData { get; }
        TeachingData TeachingData { get; }

        /// <summary>
        /// 설정 변경 발생 시 저장(DB/파일) 또는 동기화를 트리거합니다.
        /// </summary>
        void SaveConfig();
    }
}
