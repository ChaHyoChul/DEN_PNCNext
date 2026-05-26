using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;

namespace PncNext.Infrastructure.Motion.Services
{
    /// <summary>
    /// 장비 설정 데이터의 영속성 및 메모리 캐싱을 담당하는 싱글톤 저장소 구현체
    /// </summary>
    public class MotionConfigStore : IMotionConfigStore
    {
        public CoordinateOffsetData OffsetData { get; } = new CoordinateOffsetData();
        public TeachingData TeachingData { get; } = new TeachingData();

        public void SaveConfig()
        {
            // TODO: 추후 DB(AppDbContext) 또는 별도 XML/JSON 파일에 설정을 저장하는 로직 구현
            System.Diagnostics.Debug.WriteLine("장비 설정 데이터가 저장되었습니다.");
        }
    }
}
