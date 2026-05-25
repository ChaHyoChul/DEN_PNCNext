# PA 제어기 전용 MotionChannel 및 프로토콜 고도화 요약

**작성일**: 2026-05-24  
**주제**: PncNext 프로젝트 - PA 제어기 비동기 통신 및 정밀 상태 관리 구현

## 1. 비동기 통신 엔진 (`MotionChannel`) 고도화
- **요청-응답 정밀 매칭**: 
  - `MotionCommandInfo` DTO를 도입하여 명령어 키(Key), 데이터(Payload), 타임아웃(Timeout)을 패키지화했습니다.
  - 비동기 수신 루프에서 응답 메시지의 키를 추출하여 대기 중인 `TaskCompletionSource`와 정확히 매칭시키는 구조를 완성했습니다.
- **동적 타임아웃 및 안정성**:
  - 명령별로 상이한 타임아웃(예: RND_CDT 1초, MOV 5초)을 적용했습니다.
  - 타임아웃 발생 시 채널을 즉시 **Fault 상태**로 전환하고 포트를 닫아 장비를 보호하는 Fail-Fast 전략을 구현했습니다.
- **명칭 개선**: `ReceiveRawAsync()`를 기능을 더 명확히 표현하는 **`ReadFullStatusAsync()`**로 변경했습니다.

## 2. PA 전용 프로토콜 (`PAMotionProtocol`) 정교화
- **RND_CDT 완벽 파싱**:
  - 쉼표로 구분된 21개 필드(좌표, I/O, 스핀들, 서보 상태 등)를 해석하는 전용 엔진을 구현했습니다.
  - Hex 형태의 I/O 데이터를 64개의 `bool` 배열로 자동 변환하는 기능을 추가했습니다.
- **신규 MotionStatus 및 NotReady 적용**:
  - **`NotReady`** 상태를 도입하여, 서보의 원점 복귀 완료 여부(`HomeState`)를 최우선적으로 판단하도록 로직을 강화했습니다.
  - 원점 복귀 전에는 `NotReady`, 완료 후에는 제어기 상태에 따라 `Ready`, `Running`, `Pause`, `Error`로 정밀하게 전이됩니다.

## 3. 실시간 상태 업데이트 파이프라인
- **이벤트 기반 동기화**: `PAMotionControlService`가 채널의 `MessageReceived` 이벤트를 구독하여 수신 즉시 전역 저장소(`MotionStateStore`)를 최신화합니다.
- **데이터 정규화 및 공유**: 
  - `MotionStateStore`에서 PA 전용 데이터를 공통 규약인 `MotionSharedData`로 매핑하여 공유 메모리(MMF)에 기록합니다.
  - 이를 통해 WPF UI는 제어기 종류에 상관없이 표준화된 좌표 및 상태 데이터를 100ms 미만의 지연 시간으로 표시할 수 있습니다.

## 4. 아키텍처 건전성 확보
- **인터페이스 순수성**: PA 전용 파싱 로직(`UpdateStateFromResponse`)을 인터페이스가 아닌 구현 클래스에 캡슐화하여 타 제어기(INTH 등)와의 의존성을 분리했습니다.
- **타입 안정성**: 전역적으로 `MotionStatus` Enum과 가독성이 개선된 필드명(`GPLErrorCode` 등)을 사용하여 코드 유지보수성을 높였습니다.

---
**최종 확인 결과:**
- 전체 솔루션 정상 빌드 및 단위 기능 구현 완료.
- 하드웨어 통신 → 프로토콜 파싱 → 전역 상태 저장 → 공유 메모리 전파로 이어지는 통합 데이터 고속도로 구축 완료.
