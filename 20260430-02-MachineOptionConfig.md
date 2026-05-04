# MachineOptionConfig 기반 장비 구성 아키텍처 분석서

**작성일**: 2026-04-30  
**대상**: PncNext 프로젝트 장비 구성 및 옵션 제어 모듈

## 1. 개요
현재 프로젝트는 `MotionControllerConfig`를 통해 모션 제어부를 동적으로 생성하는 단계에 있습니다. 하지만 실제 장비는 모션 컨트롤러 외에도 Autoloader, Spindle, 집진기, 워터펌프 등 다양한 옵션 장치들이 조합된 집합체입니다. 본 문서에서는 `MachineOptionConfig` 정보를 바탕으로 전체 장비를 어떻게 객체화하고 관리하는 것이 효율적인지 두 가지 아키텍처 모델을 비교 분석합니다.

## 2. 아키텍처 모델 비교

### 모델 A: 단일 장비 객체 모델 (Monolithic Machine Object)
장비 전체를 하나의 `Machine` 클래스로 정의하고, 내부 프로퍼티로 모든 옵션 객체들을 포함하는 방식입니다.

*   **구조**: `Machine` 객체가 `MotionController`, `Autoloader`, `Spindle` 등을 모두 소유.
*   **장점**:
    *   장비의 전체 상태(예: 가공 중, 대기 중, 에러)를 단일 지점에서 관리하기 용이함.
    *   상위 레이어(API 등)에서 `machine.Start()`와 같이 단순한 인터페이스로 접근 가능.
*   **단점**:
    *   클래스가 비대해져 유지보수가 어려움 (God Class 위험).
    *   장비 사양에 따라 포함되지 않는 옵션이 많을 경우 Null 체크 로직이 복잡해짐.
    *   개별 옵션만 독립적으로 테스트하기 어려움.

### 모델 B: 모듈형 서비스 모델 (Modular Service Model) - **권장**
각 옵션(Autoloader, Spindle 등)을 독립적인 도메인 서비스로 분리하고, 필요한 하위 의존성(`IMotionControl`)을 주입받아 사용하는 방식입니다.

*   **구조**: `AutoloaderService`, `SpindleService` 등이 각각 독립적으로 존재하며, 생성자 주입을 통해 `IMotionControl`을 공유함.
*   **장점**:
    *   **단일 책임 원칙(SRP)** 준수: 각 서비스는 자신의 고유 기능(예: 툴 교체 로직)에만 집중함.
    *   **유연한 확장성**: 새로운 옵션 장치가 추가되어도 기존 코드를 수정하지 않고 새로운 서비스를 추가하기 쉬움.
    *   **테스트 용이성**: `IMotionControl`을 모킹(Mocking)하여 개별 옵션 서비스의 로직만 단위 테스트할 수 있음.
    *   **DI 친화적**: ASP.NET Core의 표준 DI 패턴과 완벽히 일치함.
*   **단점**:
    *   여러 서비스 간의 실행 순서를 제어하거나 통합 상태를 관리할 상위 코디네이터(Orchestrator)가 필요함.

## 3. 상세 설계 전략: 모듈형 서비스 + 파사드(Facade) 패턴

장점은 극대화하고 단점을 보완하기 위해 **모듈형 서비스** 구조를 채택하되, 이를 통합 관리하는 **Orchestrator** 레이어를 두는 방식을 제안합니다.

### 3.1. 서비스 의존성 구조
1.  **Low Level**: `IMotionControl` (실제 장비 통신 담당)
2.  **Option Services**: 
    *   `IAutoloaderService` (IMotionControl 주입받음)
    *   `ISpindleService` (IMotionControl 주입받음)
    *   `IIoControlService` (입출력 접점 제어)
3.  **High Level (Orchestrator)**: `IMillingMachineAppService`
    *   위의 모든 옵션 서비스들을 주입받아 가공 프로세스(시퀀스)를 조율함.

### 3.2. MachineOptionConfig의 역할
`MachineOptionConfig`는 각 서비스를 활성화할지 여부를 결정하는 **기능 스위치(Feature Flag)** 역할을 합니다.

```csharp
// DI 등록 시 활용 예시
if (machineConfig.HasAutoloader) {
    builder.Services.AddScoped<IAutoloaderService, AutoloaderService>();
} else {
    builder.Services.AddScoped<IAutoloaderService, EmptyAutoloaderService>(); // Null Object Pattern
}
```

## 4. 결론 및 제안
장비의 전체 구성을 하나의 거대한 객체로 만드는 것보다, **각 옵션을 독립된 서비스로 구현하고 `IMotionControl`을 주입받아 사용**하는 것이 장기적인 유지보수와 확장성 측면에서 훨씬 유리합니다.

**향후 단계 제안:**
1.  각 옵션 장치의 인터페이스(`ISpindle`, `IAutoloader` 등) 정의.
2.  `MachineOptionConfig`를 읽어 해당 서비스들을 동적으로 활성화/비활성화하는 `MachineServiceFactory` 또는 DI 로직 보완.
3.  개별 서비스들을 조합하여 전체 가공 시퀀스를 관리하는 통합 서비스 개발.

이와 같은 모듈형 접근 방식을 통해, 다양한 사양의 덴탈 밀링머신 라인업에 유연하게 대응할 수 있는 소프트웨어 아키텍처를 구축할 수 있습니다.
