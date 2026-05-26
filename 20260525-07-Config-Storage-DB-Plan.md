# 장비 설정 데이터(옵셋 및 티칭) DB 저장 및 이력 관리 설계안

**작성일**: 2026-05-15  
**주제**: PncNext 프로젝트 - 데이터베이스 기반 장비 설정 관리 및 변경 이력 추적

## 1. 개요
장비의 정밀도와 직결되는 좌표계 옵셋(Coordinate Offset) 및 티칭 데이터(Teaching Data)를 안전하게 보관하고, 변경 사항을 체계적으로 추적하기 위해 데이터베이스(SQLite) 기반의 저장 구조를 설계합니다.

## 2. 저장 방식 채택: 데이터베이스 (SQLite / EF Core)

### 2.1. 채택 이유
- **데이터 정합성**: 트랜잭션을 통해 저장 중 데이터가 깨지는 현상을 방지합니다.
- **관계 관리**: 특정 제어기 설정(`MotionControllerConfig`)과 옵셋/티칭 데이터를 ID 기반으로 완벽히 매핑할 수 있습니다.
- **이력 추적 (Audit Log)**: "누가, 언제, 어떤 좌표를 수정했는가"에 대한 로그를 남기기에 최적화된 구조입니다.
- **일관성**: 현재 프로젝트의 설정 관리 방식(EF Core)과 통일된 아키텍처를 유지합니다.

## 3. 데이터베이스 스키마 설계

### 3.1. 좌표계 옵셋 테이블 (`CoordinateOffsets`)
- `Id` (PK)
- `MotionControllerConfigId` (FK)
- `SystemName`: 좌표계 명칭 (예: "G54", "G55", "G53")
- `PosX`, `PosY`, `PosZ`, `PosA`, `PosB`, `PosC`: 각 축별 옵셋 값
- `LastUpdated`: 최종 수정 일시

### 3.2. 티칭 데이터 테이블 (`TeachingPoints`)
- `Id` (PK)
- `MotionControllerConfigId` (FK)
- `PointKey`: 티칭 포인트 식별자 (예: "0"~"99", "Tool_Change_Pos")
- `PosX`, `PosY`, `PosZ`, `PosA`, `PosB`, `PosC`: 티칭 좌표 값
- `Description`: 해당 위치에 대한 메모
- `LastUpdated`: 최종 수정 일시

### 3.3. 변경 이력 테이블 (`ConfigChangesHistory`)
- `Id` (PK)
- `Category`: 데이터 유형 ("OFFSET", "TEACHING")
- `TargetId`: 변경된 데이터의 원본 ID
- `OldValueJson`: 변경 전 데이터 스냅샷 (JSON)
- `NewValueJson`: 변경 후 데이터 스냅샷 (JSON)
- `ChangeDate`: 변경 일시
- `Reason`: 변경 사유 (예: "장비 점검 후 보정", "툴 교체")

## 4. 프로그램 구현 전략

### 4.1. MotionConfigStore (관리 서비스)
- **로딩**: 프로그램 시작 시 DB에서 현재 활성화된 제어기의 옵셋/티칭 데이터를 메모리(캐시)로 로드합니다.
- **수정 및 저장**: 사용자가 UI에서 좌표를 수정하면 `MotionConfigStore`가 DB를 업데이트하고, 동시에 `ConfigChangesHistory`에 자동으로 기록을 생성합니다.
- **동기화**: DB 저장 성공 후 실제 제어기에 해당 수치를 전송하는 명령을 트리거합니다.

### 4.2. 이력 조회 기능
- 특정 기간 동안의 좌표 변화를 조회하는 API 엔드포인트를 제공하여 장비 상태 진단 및 문제 해결에 활용합니다.

## 5. 결론
이 설계안은 장비의 중요 설정 데이터를 단순 보관하는 것을 넘어, 정밀 제어 시스템의 필수 요건인 **데이터 안정성**과 **추적 가능성**을 완벽하게 충족합니다. 이를 통해 장비의 정밀도 이슈 발생 시 신속한 원인 파악 및 이전 상태로의 복구가 가능해집니다.
