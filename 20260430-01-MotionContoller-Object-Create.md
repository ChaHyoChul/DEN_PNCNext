# MotionController 객체 동적 생성 방안 검토서

**작성일**: 2026-04-30  
**대상**: PncNext 프로젝트 Motion 제어 모듈

## 1. 개요
현재 `Program.cs`에서 `IMotionControl` 관련 객체들은 특정 구현체(`TcpCommPort`, `DummyProtocol`)로 하드코딩되어 의존성 주입(DI)이 설정되어 있습니다. 장비의 사양(통신 방식, 프로토콜 종류)이 데이터베이스(`MotionControllerConfig`)에 저장됨에 따라, 이를 읽어와 런타임에 적절한 객체를 동적으로 생성하는 구조로 개선이 필요합니다.

## 2. 현재 구조의 문제점
- 새로운 장비나 통신 방식이 추가될 때마다 `Program.cs` 코드를 수정해야 함.
- 데이터베이스의 설정값이 실제 객체 생성에 반영되지 않음.
- 여러 대의 컨트롤러를 동시에 지원하거나, 설정 변경 시 유연한 대응이 어려움.

## 3. 제안하는 해결 방안: **팩토리 패턴(Factory Pattern) 도입**

`IMotionControlFactory`를 도입하여 DB 설정을 읽고 적절한 `ICommPort`와 `IMotionProtocol`을 조합하여 `IMotionControl` 객체를 생성하는 방식을 제안합니다.

### 3.1. 객체 생성 흐름
1. **설정 로드**: `AppDbContext`를 통해 활성화된(`IsActive == true`) 장비 설정을 조회합니다.
2. **통신 포트 결정**: `CommType` 값에 따라 객체를 생성합니다.
   - `Ethernet` -> `TcpCommPort(IP, Port)`
   - `Serial` -> `SerialCommPort(ComPort, BaudRate)`
3. **프로토콜 결정**: `ProtocolProvider` 값에 따라 객체를 생성합니다.
   - `PAMotionProtocol` -> `PAMotionProtocol()`
   - `Dummy` -> `DummyProtocol()`
4. **최종 조립**: 생성된 포트와 프로토콜 객체를 `MotionControlService`에 주입하여 반환합니다.

### 3.2. 구현 기술 스택 제안
- **Service Locator / Provider**: DI 컨테이너 내에서 `IServiceProvider`를 활용하여 필요한 시점에 DB를 조회하고 객체를 생성합니다.
- **Strategy Registry**: 각 문자열 키(예: "Ethernet", "PAMotionProtocol")와 실제 클래스 타입을 매핑하여 관리합니다.

## 4. 상세 아키텍처 (Conceptual)

### 4.1. IMotionControlProvider (또는 Factory)
```csharp
public interface IMotionControlProvider {
    Task<IMotionControl> GetActiveControlAsync();
}
```

### 4.2. DI 등록 방식 개선
```csharp
// Program.cs 예시
builder.Services.AddScoped<IMotionControl>(sp => {
    var dbContext = sp.GetRequiredService<AppDbContext>();
    var config = dbContext.MotionControllerConfigs.FirstOrDefault(c => c.IsActive);
    
    if (config == null) throw new Exception("활성화된 장비 설정이 없습니다.");

    // 통신 포트 생성
    ICommPort commPort = config.CommType switch {
        "Ethernet" => new TcpCommPort(config.IPAddress, config.Port ?? 5000),
        "Serial" => new SerialCommPort(config.ComPort, config.BaudRate ?? 9600),
        _ => throw new NotSupportedException()
    };

    // 프로토콜 생성
    IMotionProtocol protocol = config.ProtocolProvider switch {
        "PAMotionProtocol" => new PAMotionProtocol(),
        "Dummy" => new DummyProtocol(),
        _ => throw new NotSupportedException()
    };

    return new MotionControlService(commPort, protocol);
});
```

## 5. 기대 효과
- **유연성**: 코드 수정 없이 DB 설정만으로 장비의 통신 방식 및 프로토콜을 변경할 수 있습니다.
- **확장성**: 새로운 프로토콜이나 통신 방식이 추가되어도 팩토리 로직만 보완하면 기존 API 코드는 수정할 필요가 없습니다.
- **테스트 용이성**: DB 설정을 "Dummy"로 변경하는 것만으로 손쉽게 에뮬레이션 모드로 전환이 가능합니다.

## 6. 향후 고려 사항
- 활성화된 설정이 여러 개일 경우(다중 축/다중 컨트롤러)에 대한 처리 방안.
- 객체 생성 비용을 줄이기 위한 싱글톤/캐싱 전략 적용 여부.
- 설정 변경 시 실시간으로 객체를 갱신(Refresh)하는 메커니즘.
