# 비동기 통신, 자동 상태 업데이트 및 타임아웃 구현 아키텍처 분석

**작성일**: 2026-05-15  
**주제**: PncNext 프로젝트 - 비동기 통신 모듈(MotionChannel)과 제어 서비스(MotionControlService) 간의 역할 분담 및 구현 계획

## 1. 구현 위치 분석 (MotionChannel vs PAMotionControlService)

비동기 수신 루프, 요청-응답 매칭, 그리고 상태 업데이트 로직을 어디에 배치할 것인가는 **관심사의 분리(Separation of Concerns)** 원칙에 따라 결정해야 합니다.

### 1.1. `MotionChannel` (통신 및 프로토콜 래퍼 계층)
- **적합한 역할**: 비동기 수신 루프(Background Receive Loop), 요청-응답 매칭(TaskCompletionSource 관리), 타임아웃 감지.
- **이유**: `MotionChannel`은 하부의 `ICommPort`를 직접 제어하고 `IMotionProtocol`을 알고 있는 유일한 곳입니다. 바이트 스트림을 읽고, 프로토콜 단위(라인 단위 등)로 쪼개는 작업은 이곳에서 이루어져야 합니다.
- **부적합한 역할**: `IMotionStateStore`에 접근하여 파싱된 데이터를 직접 업데이트하는 것. 채널은 자신이 어떤 장비(PA인지 INTH인지)의 상태를 업데이트하는지 알 필요가 없는 순수 통신 파이프라인이어야 합니다.

### 1.2. `PAMotionControlService` (비즈니스 제어 계층)
- **적합한 역할**: `MotionChannel`에서 발생하는 "데이터 수신 이벤트"를 구독(Subscribe)하고, 응답 데이터를 분석하여 `IMotionStateStore`에 반영하는 **자동 상태 업데이트**.
- **이유**: 이 서비스는 자신이 다루는 도메인(`PA` 제어기)과 저장소(`StateStore`)를 정확히 알고 있습니다. 통신 계층에서 올라온 정제된 메시지를 비즈니스 데이터로 매핑하기에 완벽한 위치입니다.

### 1.3. 결론 (역할 분담)
*   **`MotionChannel`**: 데이터를 백그라운드에서 읽어 들이고, 응답을 대기 중인 Task에 연결해 주며, 타임아웃 시 통신 에러를 발생시키는 **엔진 역할**. (이벤트나 `Channel<T>`로 상위에 데이터 전달)
*   **`PAMotionControlService`**: 채널 엔진에서 올라오는 데이터를 받아 **상태 저장소(StateStore)를 자동으로 갱신하는 역할**.

---

## 2. 타임아웃 감지 및 에러 상태 전환 메커니즘

명령을 보낸 후 일정 시간 응답이 없을 때 채널을 에러 상태로 만드는 구조입니다.

### 2.1. `MotionChannel`의 타임아웃 처리 로직
1.  **명령 전송 및 TCS 등록**: `SendCustomCommandAsync` 등이 호출되면 고유 ID(또는 프로토콜 특성)로 `TaskCompletionSource<byte[]>`(TCS)를 생성하고 딕셔너리에 보관합니다.
2.  **CancellationTokenSource (CTS) 활용**: TCS 대기 시 `CancellationTokenSource(타임아웃 시간)`을 연결합니다.
3.  **타임아웃 발생 (TimeoutException)**:
    - 지정된 시간(예: 3초) 내에 수신 루프에서 응답을 찾아 TCS를 완료시키지 못하면 예외가 발생합니다.
    - 예외가 발생(catch)하면 `MotionChannel`은 내부 상태를 **`Faulted` (에러 상태)**로 변경합니다.
4.  **에러 상태 전환 (Fail-Fast)**:
    - 상태가 `Faulted`가 되면 즉시 `ICommPort.CloseAsync()`를 호출하여 좀비 연결을 끊습니다.
    - 이후 해당 채널로 들어오는 모든 새로운 명령 요청(`MoveAsync` 등)은 즉시 `InvalidOperationException`을 던지며 차단됩니다. (복구 로직이 실행되기 전까지 하드웨어 보호)

---

## 3. 프로그램 수정 세부 계획 (Action Plan)

현재 코드를 기반으로 위의 기능들을 추가하기 위한 단계별 작업 내용입니다.

### Step 1: `IMotionChannel` 인터페이스 및 이벤트 추가
*   `IMotionChannel`에 수신 데이터를 상위로 알려줄 이벤트(`event EventHandler<byte[]> MessageReceived;`) 추가.
*   채널의 상태를 확인할 수 있는 `IsFaulted` 속성 추가.

### Step 2: `MotionChannel` 비동기 수신 루프 및 타임아웃 구현
*   `OpenAsync` 호출 시 `Task.Run(ReceiveLoopAsync)`를 통해 백그라운드 무한 읽기 루프 시작.
*   `SendCustomCommandAsync` 등에서 `TaskCompletionSource`와 5초 타임아웃을 적용하여, 응답 지연 시 `CloseAsync()`를 호출하고 `IsFaulted = true`로 변경.
*   수신 루프에서 데이터가 파싱되면 `MessageReceived` 이벤트를 발생시키고, 매칭되는 TCS가 있으면 결과를 세팅.

### Step 3: `PAMotionControlService` 자동 업데이트 연동
*   생성자에서 주입받은 각 `MotionChannel`들의 `MessageReceived` 이벤트에 핸들러(구독)를 등록.
*   이벤트가 발생할 때마다 백그라운드로 `PAMotionProtocol.UpdateStateFromResponse`를 호출하여 `IMotionStateStore`를 최신화.
*   기존 `GetStatusAsync()`는 더 이상 하드웨어에 직접 요청(Receive)하지 않고, 단순히 캐싱된 최신 상태를 리턴하거나 주기적 Polling 명령("RND_CDT")만 채널로 밀어 넣는(Fire-and-Forget) 구조로 변경.

---
이 설계를 적용하면 네트워크 지연이나 장비 이상으로 인한 "소프트웨어 행(Hang)" 현상을 완벽히 차단하고, 수신된 데이터가 어떤 경로를 거치든 자동으로 UI까지 도달하는 **고성능 데이터 파이프라인**을 완성할 수 있습니다.
