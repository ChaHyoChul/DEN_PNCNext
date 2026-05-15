# 통신 채널별 프로토콜 할당 구조 분석 및 변경 방안

**작성일**: 2026-05-08  
**주제**: PncNext 프로젝트 - 채널 독립적 프로토콜 할당 및 IMotionChannel 도입

## 1. 개요
기존 구조에서는 `MotionControllerConfig` 레벨에 프로토콜(`ProtocolProvider`)이 정의되어 있어, 하나의 제어기에 속한 모든 통신 채널(`CommItems`)이 동일한 프로토콜을 사용해야 했습니다. 본 문서는 각 통신 채널(IP/Port 또는 Serial)이 서로 다른 프로토콜을 독립적으로 사용할 수 있도록 구조를 개선하는 방안을 분석합니다.

## 2. 제안된 아키텍처 분석

### 2.1. 타당성 및 장점
- **완벽한 채널 독립성**: 단일 장비 내에 이기종 장치(예: TCP 통신 기반의 메인 제어기, Serial 기반의 I/O 보드 등)가 혼합된 환경을 완벽하게 지원합니다.
- **관심사의 분리(Seperation of Concerns)**: 제어기 서비스는 각 통신 포트의 내부 프로토콜을 알 필요 없이, 추상화된 명령(예: Move, Stop)만 전달하게 되어 코드가 단순해지고 결합도가 낮아집니다.

### 2.2. 잠재적 문제점 및 해결 방안
- **문제점**: 통신 포트(`ICommPort`)에 프로토콜(`IMotionProtocol`)을 직접 주입할 경우, 순수 전송 계층인 `ICommPort`가 비즈니스 로직(인코딩/디코딩)까지 처리하게 되어 단일 책임 원칙(SRP)에 위배됩니다.
- **해결 방안 (IMotionChannel 래퍼 도입)**: 순수 통신을 담당하는 `ICommPort`와 인코딩 로직을 담당하는 `IMotionProtocol`을 결합한 새로운 `IMotionChannel` 인터페이스를 정의합니다. 제어기 서비스는 이 `IMotionChannel`을 주입받아 사용합니다.

## 3. 프로그램 구조 변경 방안

### 3.1. 데이터베이스(DB) 엔티티 수정
- **`MotionControllerConfig`**: `ProtocolProvider` 필드 삭제.
- **`CommItemConfig`**: `ProtocolProvider` 필드 추가 (각 통신 아이템별로 프로토콜 정의).

### 3.2. IMotionChannel 인터페이스 도입 (Decorator 패턴)
통신과 프로토콜을 하나로 묶어주는 래퍼(Wrapper) 계층을 도입합니다.

```csharp
public interface IMotionChannel
{
    bool IsOpen { get; }
    Task OpenAsync();
    Task CloseAsync();
    
    // 서비스는 인코딩 방식(Protocol)을 모르고 채널에 추상적인 명령을 내림
    Task MoveAsync(double x, double y, double z, double a, double b);
    Task StopAsync();
    Task<MotionStatus> GetStatusAsync();
}

public class MotionChannel : IMotionChannel
{
    private readonly ICommPort _port;
    private readonly IMotionProtocol _protocol;

    public MotionChannel(ICommPort port, IMotionProtocol protocol)
    {
        _port = port;
        _protocol = protocol;
    }

    // 예시: Move 명령 시 자체 프로토콜로 인코딩하여 포트로 전송
    public async Task MoveAsync(double x, double y, double z, double a, double b)
    {
        if (!_port.IsOpen) await _port.OpenAsync();
        var data = _protocol.EncodeMove(x, y, z, a, b);
        await _port.SendAsync(data);
    }
    // (StopAsync, GetStatusAsync 등도 동일하게 구현)
}
```

### 3.3. 제어기 서비스 수정 (`PAMotionControlService` 등)
서비스는 더 이상 `IMotionProtocol`을 직접 주입받지 않으며, 각 역할(`Purpose`)에 매핑된 `IMotionChannel`들을 주입받습니다.

```csharp
public class PAMotionControlService : IMotionControl
{
    private readonly IDictionary<string, IMotionChannel> _channels;

    public PAMotionControlService(IDictionary<string, IMotionChannel> channels)
    {
        _channels = channels;
    }

    public async Task MoveAsync(double x, double y, double z, double a, double b)
    {
        // "CMD" 목적의 채널에 Move를 지시. (내부 프로토콜은 알 필요 없음)
        await _channels["CMD"].MoveAsync(x, y, z, a, b); 
    }
}
```

### 3.4. DI 등록 (Program.cs) 변경
팩토리 메서드에서 각 `CommItem`에 정의된 `ProtocolProvider`를 기반으로 프로토콜을 생성하고, 이를 `ICommPort`와 결합하여 `MotionChannel` 인스턴스를 생성한 뒤 서비스에 주입합니다.

## 4. 결론
이러한 설계 변경을 통해 통신 계층과 비즈니스 제어 계층을 명확히 분리하고, 각 통신 아이템이 독립적인 프로토콜을 가질 수 있는 매우 유연하고 확장 가능한 아키텍처를 구축할 수 있습니다.
