# 일반화된 통신 구조 분석

**작성일**: 2026-05-07  
**주제**: PncNext 프로젝트 - 유연한 통신 아키텍처

## 1. 개요
제안된 아키텍처는 논리적 모션 제어기(Motion Controller)와 물리적 통신 채널을 분리하는 것을 목표로 합니다. 이를 통해 단일 제어기가 여러 통신 아이템(IP, Port, Serial)을 동적으로 관리할 수 있습니다.

## 2. 아키텍처 분석

### 장점:
- **최대 유연성**: 단일 장비에 대해 혼합 통신(Ethernet + Serial)을 지원합니다.
- **분산 하드웨어 지원**: 서로 다른 IP 주소를 가진 구성 요소들을 하나의 제어기 아래에서 관리할 수 있습니다.
- **확장성**: 코드 수정 없이 DB 설정을 통해 포트를 추가하거나 제거할 수 있습니다.

### 잠재적 과제 및 해결 방안:
- **매핑 모호성**: 각 포트의 역할(예: CMD, STS, LOG, EVT)을 식별하기 위한 'Purpose(용도)' 필드를 추가하여 해결합니다.
- **설정 복잡성**: 표준 설정을 위한 기본 템플릿(Seeding 데이터)을 제공하여 해결합니다.

## 3. 제안된 데이터베이스 변경 사항

### MotionControllerConfigs 테이블:
- **삭제**: IPAddress, Port, ComPort, BaudRate.
- **추가**: CommItemConfigs와의 일대다(1:N) 관계.

### CommItemConfigs 테이블 (신규):
- `Id` (PK)
- `MotionControllerConfigId` (FK)
- `CommType` (Ethernet / Serial)
- `IPAddress`, `Port` (Ethernet용)
- `ComPort`, `BaudRate` (Serial용)
- **`Purpose`** (CMD, STS, LOG, EVT 등 식별자)

## 4. 프로그램 구조 업데이트

### 서비스 레이어 (PAMotionControlService):
- 단일 `ICommPort` 대신 `IDictionary<string, ICommPort>` 또는 전용 컨테이너 객체를 주입받습니다.
- 각 기능(이동, 상태 조회 등)은 'Purpose' 키를 사용하여 적절한 포트에 접근합니다.

### DI 등록 (Program.cs):
- 팩토리 메서드는 활성 설정에 포함된 모든 `CommItems`를 루프 돌며 적절한 구현체(`TcpCommPort` 또는 `SerialCommPort`)를 인스턴스화합니다.

## 5. 결론
이 아키텍처는 다양한 덴탈 밀링머신 구성을 위한 견고한 기반을 제공하며, 장기적인 유지보수 비용을 절감하고 하드웨어 변경에 대한 적응성을 크게 향상시킵니다.
