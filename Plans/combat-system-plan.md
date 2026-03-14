# 전투 시스템 구현 계획

## 목표
- 아군 무기: 타겟이 있으면 자동 발사 → 적 HP 감소
- 적군 무기: 게이지 만땅 → 아군 방 랜덤 선택 → 아군 HP 감소
- 무기 타입별 실드 상호작용 처리

---

## 데이터 현황 (변경 없음)

### WeaponBaseSO (기존 필드 활용)
| 필드 | 타입 | 용도 |
|------|------|------|
| `Damage` | int | 타격 1회당 피해량 |
| `ProjectileCount` | int | 발사 시 타격 횟수 (기본 1) |
| `Type` | WeaponType | Laser / Missile / Beam |

> **ProjectileCount는 신규 추가 불필요 — 이미 존재**

### WeaponType enum (기존)
| 값 | 설명 | 실드 상호작용 |
|----|------|--------------|
| `Laser` | 레이저 | 타격마다 실드 체크 |
| `Missile` | 미사일 | 실드 무시 (직접 Hull 피해) |
| `Beam` | 빔 | 미구현 (추후) |

---

## 피해 계산 규칙

### Laser (레이저) — 실드 per-hit 체크
```
발사체 수(ProjectileCount)만큼 반복:
  └─ shield.TryAbsorbDamage() == true  → 이 발사체 무효화 (실드 1 소모)
  └─ false (실드 없음 or 이미 소진)   → TakeDamage(Damage)
```

**예시**: ProjectileCount=2, Damage=1, 실드=1
- 1번째 타격: 실드 흡수 → 피해 0
- 2번째 타격: 실드 없음 → 피해 1
- **최종**: HP -1, 실드 0

**예시**: ProjectileCount=2, Damage=1, 실드=0
- 1번째 타격: 피해 1
- 2번째 타격: 피해 1
- **최종**: HP -2

### Missile (미사일) — 실드 무시
```
발사체 수(ProjectileCount)만큼 반복:
  └─ 실드 체크 없이 무조건 TakeDamage(Damage)
```

---

## 신규 클래스: CombatResolver (`Logic/System/`)

### 역할
- 아군 무기 `OnFired` 이벤트 구독 → 적에게 피해 적용
- 적군 무기 자동 발사 처리 (`TickEnemyWeapons()`)
- 무기 타입별 실드 상호작용 처리

### 핵심 메서드
```
BindWeaponEvents()
  - 아군 무기 OnFired 구독
  - 적군 무기 OnFired 구독

TickEnemyWeapons()
  - 적군 무기 IsPowered && IsReadyToFire → TryFire()
  - EnemyCombatManager.OnTickUpdate()에서 호출

ApplyDamage(targetAPI, weapon)
  - ProjectileCount 반복
  - Laser: 각 타격마다 TryAbsorbDamage() 체크
  - Missile: 실드 무시, 직접 TakeDamage
```

---

## 수정 파일 목록

| 파일 | 변경 내용 |
|------|----------|
| `Core/Interface/IShipAPI.cs` | `void TakeDamage(int damage)` 추가 |
| `Logic/SpaceShip/SpaceShipManager.cs` | `TakeDamage` 구현 (System.Math.Max 사용) |
| `Logic/SpaceShip/Weapons/WeaponLogic.cs` | 오토파이어: `IsAutoFire` 조건 제거 → 타겟만 있으면 발사 |
| `Logic/System/CombatResolver.cs` | **신규** |
| `Logic/Event/EnemyCombatManager.cs` | StartCombat에서 적군 무기 전력 자동 ON, SetCombatResolver(), TickEnemyWeapons() 호출 |
| `Presentation/System/ShipSetupManager.cs` | CombatResolver 생성/바인딩 |

---

## 전체 흐름도

```
[아군 발사]
WeaponLogic.OnTickUpdate
  → TargetRoomID != -1 && IsReadyToFire
    → TryFire() → OnFired
      → CombatResolver.OnPlayerWeaponFired(weapon)
        → ApplyDamage(enemyShipAPI, weapon)
            [Laser] 각 hit: TryAbsorbDamage() → 흡수 시 skip, 아니면 TakeDamage
            [Missile] 각 hit: 무조건 TakeDamage
              → SpaceShipManager.SetHullHealth → OnHullHealthChanged
                → EnemyHullHealthUIView 갱신

[적군 발사]
EnemyCombatManager.OnTickUpdate
  → CombatResolver.TickEnemyWeapons()
    → weapon.IsReadyToFire && IsPowered
      → weapon.TryFire() → OnFired
        → CombatResolver.OnEnemyWeaponFired(weapon)
          → ApplyDamage(playerShipAPI, weapon)
              [Laser] 각 hit: TryAbsorbDamage() → 흡수 시 skip, 아니면 TakeDamage
              [Missile] 각 hit: 무조건 TakeDamage
                → SpaceShipManager.SetHullHealth → OnHullHealthChanged
                  → (플레이어 HP UI 갱신 — 추후)

[승리/패배 체크]
EnemyCombatManager.OnTickUpdate
  → EnemyShipAPI.CurrentHullHealth <= 0 → EndCombat() → CompleteCombatSubEvent()
```

---

## 미포함 (추후 구현)
- `HullHealthUIView` 실시간 갱신 (플레이어 피격 시 UI)
- `Beam` 타입 피해 계산
- 미사일 탄약 소모
- 피격 이펙트 / 사운드
