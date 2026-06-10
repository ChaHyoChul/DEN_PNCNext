using PncNext.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PncNext.Domain.Interfaces
{
    /// <summary>
    /// 가공(Milling) 작업의 라이프사이클 관리 및 이력을 기록하는 서비스 인터페이스
    /// </summary>
    public interface IJobManagementService
    {
        /// <summary>
        /// 새로운 가공 작업을 시작합니다.
        /// </summary>
        /// <param name="ncFileId">가공할 NC 파일 ID</param>
        /// <param name="diskSeq">가공에 사용할 디스크 대리키(Seq)</param>
        /// <returns>생성된 작업 이력 객체</returns>
        Task<JobHistory> StartJobAsync(int ncFileId, int diskSeq);
        
        /// <summary>
        /// 가공 작업을 성공적으로 완료 처리합니다.
        /// </summary>
        /// <param name="jobId">작업 이력 ID</param>
        /// <param name="newUsedAreaJson">업데이트할 디스크 사용 영역 레이아웃(JSON)</param>
        Task CompleteJobAsync(int jobId, string newUsedAreaJson);
        
        /// <summary>
        /// 가공 작업 중 발생한 에러를 기록하고 실패 처리합니다.
        /// </summary>
        /// <param name="jobId">작업 이력 ID</param>
        /// <param name="errorCode">발생한 에러 코드</param>
        /// <param name="errorLineNumber">에러가 발생한 G-Code 라인 번호</param>
        Task FailJobAsync(int jobId, string errorCode, int errorLineNumber);
        
        /// <summary>
        /// 사용자에 의해 가공 작업을 취소 처리합니다.
        /// </summary>
        /// <param name="jobId">작업 이력 ID</param>
        Task CancelJobAsync(int jobId);
        
        /// <summary>
        /// 최근 가공 작업 이력 목록을 조회합니다.
        /// </summary>
        /// <param name="count">조회할 레코드 개수</param>
        Task<IEnumerable<JobHistory>> GetRecentJobsAsync(int count = 50);
    }
}
