# 툴(Tool) 및 툴 포켓(Tool Pocket) 관리 시스템 분석서 v2

**작성일**: 2026-04-30  
**대상**: PncNext 프로젝트 다기능 툴 수명 관리 및 기구별 특화 제어 모듈

## 1. 개요 (v2 업데이트)
본 문서는 이전 분석(v1)을 확장하여, 장비별로 상이한 툴 포켓의 개수(8, 12, 16개)와 기구적 형상(원형, 선형 등)에 따른 물리적 제어 차이를 수용할 수 있는 유연한 아키텍처를 정의합니다. 특히 포켓 종류별로 달라지는 자동 티칭(Auto Teaching) 및 측정 로직을 데이터 기반으로 관리하는 방안을 중점적으로 다룹니다.

## 2. 확장된 데이터 구조 설계 (DB)

### 2.1. ToolPocketDefinitions (툴 포켓 마스터 정의)
장비에 장착 가능한 툴 포켓의 종류와 기구적 특성을 정의합니다.
- `Id` (PK)
- `TypeName`: 포켓 종류 명칭 (예: "Linear_8", "Circular_12", "Linear_16")
- `SlotCount`: 가용 슬롯 개수 (8, 12, 16 등)
- `TeachingLogicType`: 티칭 시 적용할 알고리즘 식별자 (예: "CircularStrategy", "LinearStrategy")
- `BaseParametersJson`: 기구적 기본 파라미터 (Pitch, Radius, Offset 등)를 JSON 형태로 저장하여 유연성 확보.

### 2.2. ToolPockets (개별 포켓 관리)
현재 장비에 설정된 구체적인 포켓 리스트입니다.
- `Id` (PK)
- `DefinitionId`: 어떤 종류의 포켓인지 정의 (FK to ToolPocketDefinitions)
- `PocketIndex`: 포켓의 순번 (1..SlotCount)
- `TargetDiameter`: 해당 슬롯에 할당된 툴 직경
- `CurrentToolId`: 현재 장착된 툴 ID (FK to Tools)

### 2.3. Tools (개별 툴 정보) - *v1과 동일*
- `ToolId` (PK): "직경-교체날짜-시간"
- `Diameter`, `MaxUsageTime`, `UsedTime`, `Status`, `BackupToolId` 등

## 3. 프로그램 아키텍처: 전략 패턴(Strategy Pattern) 기반 제어

### 3.1. ITeachingStrategy (티칭 전략 인터페이스)
포켓의 기구적 형상에 따라 달라지는 계산 및 동작 로직을 추상화합니다.
- `CalculatePocketPosition(int index, PocketParameters params)`: 인덱스별 물리 좌표 계산.
- `ExecuteTeachingSequence(ITeachingContext context)`: 포켓 특성에 맞는 티칭 시퀀스 수행.

### 3.2. 구현 클래스 예시
- **CircularTeachingStrategy**: 원형 포켓의 중심점과 반경을 이용한 각도 기반 좌표 계산.
- **LinearTeachingStrategy**: 선형 포켓의 시작점과 피치(Pitch)를 이용한 증분 기반 좌표 계산.

### 3.3. ToolManager (중앙 관리 모듈)
- **포켓 종류 인식**: DB에서 `ToolPocketDefinitions`를 로드하여 현재 장비에 적합한 `ITeachingStrategy`를 런타임에 결정(Factory 활용).
- **통합 관리 UI 지원**: 8개, 12개, 16개 등 포켓 개수에 따라 UI 레이아웃을 동적으로 구성하기 위한 메타데이터 제공.

## 4. 핵심 워크플로우 (확장)

### 4.1. Auto Teaching 시나리오
1. 사용자가 "포켓 티칭 시작" 명령 전달.
2. 시스템은 현재 설정된 `ToolPocketDefinition`을 확인.
3. 해당 포켓 타입에 맞는 `TeachingStrategy`를 로드.
4. `BaseParametersJson`에서 피치나 옵셋 값을 읽어와 각 포켓 위치를 자동 계산하며 측정 수행.

### 4.2. 툴 등록 및 백업 로직
- 각 포켓 인덱스별로 툴을 개별 등록하거나 관리할 수 있는 인터페이스 제공.
- 백업 툴 검색 시, 동일한 `Definition` 내에서 뿐만 아니라 장비 내 모든 사용 가능한 포켓을 대상으로 최적의 툴 검색.

## 5. 실시간 데이터 공유 및 가공 리포트
- **ThreadState 확장**: 현재 포켓의 레이아웃 정보(개수, 형상)를 포함하여 클라이언트가 적절한 가상 툴 뷰를 그릴 수 있도록 지원.
- **통합 리포트**: 가공 완료 후 "사용된 툴 ID - 포켓 번호 - 포켓 종류 - 가공 시간"을 결합한 상세 이력 생성.

## 6. 결론 및 제안
- **유연한 설정**: 기구적 변경 사항을 코드 수정 없이 DB의 JSON 파라미터 수정만으로 대응 가능하도록 설계했습니다.
- **모듈화**: 티칭 로직을 전략 패턴으로 분리함으로써 신규 기구 형상이 도입되어도 기존 시스템의 안정성을 유지하며 확장할 수 있습니다.

이 고도화된 설계를 통해 8개부터 16개까지, 그리고 다양한 기구적 특성을 가진 툴 포켓 시스템을 하나의 소프트웨어 구조로 통합 관리할 수 있습니다.
