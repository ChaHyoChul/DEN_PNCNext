# NC 가공 피딩 및 복잡한 시퀀스 제어 전략 분석서

**작성일**: 2026-04-30  
**대상**: PncNext 프로젝트 동작 제어 및 시퀀스 관리 모듈

## 1. 개요
덴탈 밀링머신은 단순한 상태 모니터링 외에도 NC 파일 피딩(Feeding), 툴 포켓 티칭, 캘리브레이션, 오토로더 스캔 등 복잡하고 긴 시간이 소요되는 시퀀스 제어가 필요합니다. 본 문서에서는 이러한 동작들을 처리하기 위한 최적의 스레딩 모델과 제어 아키텍처를 분석합니다.

## 2. 동작 유형별 제어 전략

### 2.1. 지속적 모니터링 (Continuous Monitoring)
- **대상**: 제어기 상태 체크, 센서 데이터 수집.
- **전략**: **`BackgroundService` (IHostedService)** 활용.
- **이유**: 프로그램 시작부터 종료까지 항상 실행되어야 하며, 시스템의 생존 여부와 직결되는 작업입니다.

### 2.2. 데이터 스트리밍 (NC File Feeding)
- **대상**: 대용량 NC 코드를 제어기로 한 줄씩 전송.
- **전략**: **전용 Worker Thread 또는 Task + Producer-Consumer 패턴**.
- **이유**: 파일 읽기(Disk I/O)와 전송(Network/Serial I/O) 간의 속도 차이를 보정하기 위해 버퍼링이 필요하며, 가공 중단/일시정지 요청에 즉각 반응해야 합니다.

### 2.3. 일회성 시퀀스 제어 (Triggered Sequences)
- **대상**: Auto Teaching, Calibration, Autoloader Scan.
- **전략**: **상태 머신(State Machine) 기반의 Sequence Engine**.
- **이유**: 각 시퀀스는 여러 단계(Step)로 구성되며, 각 단계마다 장비의 피드백을 기다려야 합니다. `BackgroundService`보다는 요청 시점에 생성되어 실행되는 비동기 태스크가 적합합니다.

## 3. 시퀀스 제어 아키텍처 제안

### 3.1. BackgroundService vs Task
- **BackgroundService**: 전역적이고 지속적인 관리에 사용 (예: `MotionMonitor`, `SystemHealthCheck`).
- **Task (TAP)**: 특정 동작 수행 시 생성하고 완료 시 소멸. `CancellationToken`을 통해 외부에서 안전하게 취소 가능.

### 3.2. 상태 머신 (State Machine) 도입의 필요성
복잡한 시퀀스(예: Multi-origin Teaching)는 중간에 에러가 발생하거나 사용자가 정지했을 때 안전한 위치로 복귀(Recovery)하는 로직이 필수적입니다. 이를 위해 각 시퀀스를 다음과 같은 단계로 관리합니다.
1.  **Idle**: 대기 상태.
2.  **Initializing**: 초기 위치 복귀 및 파라미터 체크.
3.  **Executing (Step N)**: 실제 동작 수행.
4.  **Pausing / Resuming**: 일시 정지 및 재개 처리.
5.  **Finalizing / Cleanup**: 종료 후 안전 조치.

## 4. 상세 구현 방안

### 4.1. NC Feeding 구현 전략
- **Async Queue**: NC 데이터를 읽어 큐에 쌓고, 제어기가 준비될 때마다 하나씩 꺼내 전송.
- **Flow Control**: 제어기의 버퍼 상태를 확인하여 전송 속도를 조절하는 피드백 루프 구현.

### 4.2. Sequence Manager (코디네이터)
여러 비동기 동작이 충돌하지 않도록 관리하는 중앙 관리자가 필요합니다.
- **Locking**: 가공(Run) 중에는 티칭이나 캘리브레이션을 시작할 수 없도록 논리적 락(Lock)을 관리.
- **Progress Reporting**: 각 시퀀스의 진행률을 `ThreadState`에 업데이트하여 UI에 실시간 표시.

## 5. 결론 및 제안

| 동작 유형 | 권장 기술 | 관리 주체 |
| :--- | :--- | :--- |
| **상태 모니터링** | `BackgroundService` | 시스템 레벨 |
| **NC 피딩** | `Task` + `Channel<T>` | `JobService` |
| **티칭/캘리브레이션** | `State Machine` + `Task` | `SequenceService` |

**향후 단계:**
1.  가공 시퀀스의 각 단계별 예외 처리 로직(에러 시 장비 즉시 정지 및 후퇴) 설계.
2.  `CancellationTokenSource`를 활용한 전역 긴급 정지 메커니즘 구축.
3.  복잡한 시퀀스를 정의하기 위한 워크플로우 엔진(간이형) 인터페이스 설계.

이와 같은 구조를 통해 가공 중에도 시스템은 안정적으로 상태를 모니터링하며, 복잡한 사용자 명령에 유연하고 안전하게 대응할 수 있는 제어 시스템을 구축할 수 있습니다.
