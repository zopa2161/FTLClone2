using System.Collections.Generic;
using Core.Data.Combat;
using Core.enums;
using Core.Interface;
using Logic.System;
using UnityEngine;

namespace Presentation.Views.Combat
{
    /// <summary>
    /// 발사체 애니메이션을 관리하는 Presentation 계층 매니저.
    /// CombatResolver.OnAttackQueued를 구독해 PendingAttack 데이터를 받고,
    /// PROJECTILE_TRAVEL_TICKS 동안 발사체를 출발점 → 도착점으로 이동시킨다.
    /// 발사체 도착 타이밍 = CombatResolver의 피해 적용 타이밍.
    /// </summary>
    public class CombatViewManager : MonoBehaviour, ITickable
    {
        [Header("투사체 프리팹")]
        public GameObject LaserPrefab;
        public GameObject MissilePrefab;
        public GameObject BeamPrefab;

        [Header("발사 위치")]
        public Transform LaunchPoint;

        // ─── 런타임 참조 ────────────────────────────────────────────────
        private SpaceShipView _playerShipView;
        private SpaceShipView _enemyShipView;
        private IShipAPI _playerShipAPI;  // TargetShipAPI 비교용
        private SimulationCore _simCore;
        private CombatResolver _combatResolver;
        // ────────────────────────────────────────────────────────────────

        // 발사체 속도 (Unity units / tick). 값을 낮출수록 짧은 거리에서도 대기 없이 날아감
        private const float PROJECTILE_SPEED = 0.5f;

        private readonly List<ActiveProjectile> _activeProjectiles = new();

        private class ActiveProjectile
        {
            public GameObject Obj;
            public Vector3 StartPos;
            public Vector3 EndPos;
            public int WaitTicks;        // 이동 전 대기 틱 수
            public int TravelTicks;      // 실제 이동 총 틱 수
            public int TravelTicksLeft;  // 이동 중 남은 틱
        }

        // ─── 초기화 ─────────────────────────────────────────────────────

        /// <summary>BeginSetup 시 플레이어 정보로 초기화합니다.</summary>
        public void Initialize(SpaceShipView playerShipView, IShipAPI playerShipAPI, SimulationCore simCore)
        {
            _playerShipView = playerShipView;
            _playerShipAPI  = playerShipAPI;
            _simCore        = simCore;
        }

        /// <summary>전투 시작 시 적군 View와 resolver를 연결합니다.</summary>
        public void OnCombatStarted(SpaceShipView enemyShipView, CombatResolver resolver)
        {
            _enemyShipView  = enemyShipView;
            _combatResolver = resolver;
            resolver.OnAttackQueued += HandleAttackQueued;
            _simCore.RegisterTickables(this);
        }

        /// <summary>전투 종료 시 발사체를 모두 제거하고 등록을 해제합니다.</summary>
        public void OnCombatEnded()
        {
            if (_combatResolver != null)
                _combatResolver.OnAttackQueued -= HandleAttackQueued;

            foreach (var p in _activeProjectiles)
                if (p.Obj != null) Destroy(p.Obj);
            _activeProjectiles.Clear();

            _simCore.UnregisterTickable(this);
            _enemyShipView  = null;
            _combatResolver = null;
        }

        // ─── ITickable ──────────────────────────────────────────────────

        public void OnTickUpdate()
        {
            for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                var p = _activeProjectiles[i];

                if (p.WaitTicks > 0)
                {
                    p.WaitTicks--;
                    if (p.WaitTicks == 0)
                        p.Obj.SetActive(true); // 대기 끝 → 표시
                    continue;
                }

                p.TravelTicksLeft--;
                float t = 1f - (float)p.TravelTicksLeft / p.TravelTicks;
                p.Obj.transform.position = Vector3.Lerp(p.StartPos, p.EndPos, t);

                if (p.TravelTicksLeft <= 0)
                {
                    Destroy(p.Obj);
                    _activeProjectiles.RemoveAt(i);
                }
            }
        }

        // ────────────────────────────────────────────────────────────────

        private void HandleAttackQueued(PendingAttack pending)
        {
            var targetView = pending.TargetShipAPI == _playerShipAPI ? _playerShipView : _enemyShipView;
            if (targetView == null) return;

            var prefab = GetPrefab(pending.WeaponType);
            if (prefab == null) return;

            foreach (var hit in pending.Hits)
            {
                Vector3 startPos = LaunchPoint.position;
                Vector3 endPos = hit.ReservedShield
                    ? (targetView.ShieldAbsorbingPoint != null
                        ? targetView.ShieldAbsorbingPoint.position
                        : targetView.transform.position)
                    : GetRoomPosition(targetView, pending.TargetRoomID);

                // 거리 기반 이동 틱 계산 — 초과 시 클램프 (최대 속도로 날아감)
                float distance = Vector3.Distance(startPos, endPos);
                int travelTicks = Mathf.Max(1, Mathf.RoundToInt(distance / PROJECTILE_SPEED));
                travelTicks = Mathf.Min(travelTicks, CombatResolver.PROJECTILE_TRAVEL_TICKS);
                int waitTicks = CombatResolver.PROJECTILE_TRAVEL_TICKS - travelTicks;

                // 발사 방향으로 회전 (+ 180° = 스프라이트 기본 방향 보정)
                Vector2 dir = endPos - startPos;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

                var obj = Instantiate(prefab, startPos, rotation);
                obj.SetActive(waitTicks == 0); // 대기 중에는 숨김
                _activeProjectiles.Add(new ActiveProjectile
                {
                    Obj             = obj,
                    StartPos        = startPos,
                    EndPos          = endPos,
                    WaitTicks       = waitTicks,
                    TravelTicks     = travelTicks,
                    TravelTicksLeft = travelTicks
                });
            }
        }

        private GameObject GetPrefab(WeaponType type) => type switch
        {
            WeaponType.Laser   => LaserPrefab,
            WeaponType.Missile => MissilePrefab,
            WeaponType.Beam    => BeamPrefab,
            _                  => null
        };

        private Vector3 GetRoomPosition(SpaceShipView shipView, int roomID)
        {
            var roomView = shipView.RoomViews.Find(r => r.RoomID == roomID);
            return roomView != null ? roomView.transform.position : shipView.transform.position;
        }
    }
}
