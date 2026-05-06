using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using PncNext.Domain.Models;

namespace PncNext.Infrastructure.SharedMemory
{
    /// <summary>
    /// Memory-Mapped File을 사용하여 프로세스 간 데이터를 공유하는 서비스입니다.
    /// </summary>
    public class SharedMemoryService : IDisposable
    {
        private const string MapName = "PncNext_MotionData";
        private readonly MemoryMappedFile _mmf;
        private readonly int _dataSize;

        public SharedMemoryService()
        {
            _dataSize = Marshal.SizeOf<MotionSharedData>();
            // 공유 메모리 생성 또는 오픈
            _mmf = MemoryMappedFile.CreateOrOpen(MapName, _dataSize);
        }

        /// <summary>
        /// 모션 데이터를 공유 메모리에 기록합니다.
        /// </summary>
        public void WriteMotionData(MotionSharedData data)
        {
            using (var accessor = _mmf.CreateViewAccessor(0, _dataSize))
            {
                accessor.Write(0, ref data);
            }
        }

        /// <summary>
        /// 공유 메모리에서 모션 데이터를 읽어옵니다.
        /// </summary>
        public MotionSharedData ReadMotionData()
        {
            MotionSharedData data;
            using (var accessor = _mmf.CreateViewAccessor(0, _dataSize))
            {
                accessor.Read(0, out data);
            }
            return data;
        }

        public void Dispose()
        {
            _mmf.Dispose();
        }
    }
}
