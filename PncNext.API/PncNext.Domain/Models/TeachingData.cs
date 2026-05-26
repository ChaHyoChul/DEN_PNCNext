namespace PncNext.Domain.Models
{
    /// <summary>
    /// 장비의 주요 티칭 포인트(최대 100개)를 저장하는 클래스
    /// </summary>
    public class TeachingData
    {
        // Key: 티칭 인덱스(0~99) 또는 식별 이름, Value: 6축 좌표 배열
        public Dictionary<string, double[]> Positions { get; set; } = new();

        public TeachingData()
        {
            // 0~99번까지 기본 티칭 슬롯 초기화
            for (int i = 0; i < 100; i++)
            {
                Positions[i.ToString()] = new double[6];
            }
        }
    }
}
