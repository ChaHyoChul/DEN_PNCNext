namespace PncNext.Domain.Models
{
    /// <summary>
    /// 워크 좌표계 옵셋(G54~G59) 및 기계 원점 옵셋 데이터를 저장하는 클래스
    /// </summary>
    public class CoordinateOffsetData
    {
        // Key: "G54", "G55" 등, Value: 6축(X,Y,Z,A,B,C) 옵셋 배열
        public Dictionary<string, double[]> WorkOffsets { get; set; } = new();
        
        // 기계 원점 기준 옵셋 (G53)
        public double[] MachineOffset { get; set; } = new double[6];

        public CoordinateOffsetData()
        {
            // 기본 좌표계 초기화
            string[] defaultSystems = { "G54", "G55", "G56", "G57", "G58", "G59" };
            foreach (var sys in defaultSystems)
            {
                WorkOffsets[sys] = new double[6];
            }
        }
    }
}
