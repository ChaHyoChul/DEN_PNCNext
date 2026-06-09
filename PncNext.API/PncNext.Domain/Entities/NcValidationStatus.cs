namespace PncNext.Domain.Entities
{
    /// <summary>
    /// NC 파일의 유효성 검증 및 디스크 매핑 상태를 정의하는 열거형
    /// </summary>
    public enum NcValidationStatus
    {
        /// <summary>
        /// 디스크 식별 및 파일 검증이 완료되어 가공 대기열에 진입 가능한 정상 상태
        /// </summary>
        Ready = 0,

        /// <summary>
        /// G-Code 구문 오류 또는 파싱 실패 상태 (가공 차단)
        /// </summary>
        SyntaxError = 1,

        /// <summary>
        /// 파일명에 D0000- 형태의 패턴 접두사가 누락된 상태 (수동 매칭 대기)
        /// </summary>
        MissingDiskInfo = 2,

        /// <summary>
        /// 접두사 패턴은 존재하나, 해당 디스크 ID가 DB에 등록되지 않은 임시 고아 상태
        /// </summary>
        InvalidDiskId = 3,

        /// <summary>
        /// 외부 프로세스에 의해 파일이 잠겨있거나 복사 중인 상태
        /// </summary>
        FileLocked = 4,

        /// <summary>
        /// 현재 가공이 진행 중인 상태 (수정 불가 가드레일 적용 대상)
        /// </summary>
        Processing = 5,

        /// <summary>
        /// 가공이 정상적으로 완료된 상태
        /// </summary>
        Completed = 6,

        /// <summary>
        /// 가공 중 에러가 발생하여 중단된 상태
        /// </summary>
        Error = 7
    }
}
