using System;
using System.Collections.Generic;
using Core.Data.Combat;
using Core.Interface;
using Core.enums;
using UnityEngine;
using Random = System.Random;

namespace Logic.System
{
    /// <summary>
    /// 무기 발사(OnFired)와 실제 피해 적용 사이를 중계합니다.
    ///
    /// 발사 즉시:
    ///   - 실드 예약(TryReserveShield) — CurrentShields 시각은 유지, PendingAbsorption만 증가
    ///   - 계산 결과를 PendingAttack 큐에 삽입
    ///   - OnAttackQueued 이벤트 발행 → View가 구독해 발사체 애니메이션 시작
    ///
    /// PROJECTILE_TRAVEL_TICKS 후:
    ///   - ApplyReservedAbsorption() — 실제 실드 차감 + OnShieldChanged 발행
    ///   - TakeDamage() — 실제 HP 감소
    /// </summary>
    public class CombatResolver : ITickable
    {
        /// <summary>발사 → 피해 적용까지의 틱 수 (기본 15틱 = 1.5초)</summary>
        public const int PROJECTILE_TRAVEL_TICKS = 15;

        private readonly IShipAPI _playerShipAPI;
        private readonly IShipAPI _enemyShipAPI;
        private readonly WeaponManager _playerWeaponManager;
        private readonly Random _random = new Random();

        private readonly List<PendingAttack> _pendingAttacks = new List<PendingAttack>();

        /// <summary>발사체가 큐에 삽입될 때 발행. View가 구독하여 애니메이션 시작.</summary>
        public event Action<PendingAttack> OnAttackQueued;

        public CombatResolver(IShipAPI playerAPI, IShipAPI enemyAPI, WeaponManager weaponManager)
        {
            _playerShipAPI = playerAPI;
            _enemyShipAPI = enemyAPI;
            _playerWeaponManager = weaponManager;
        }

        /// <summary>
        /// 아군/적군 무기 OnFired 이벤트 구독.
        /// EnemyCombatManager.StartCombat() 이후에 호출해야 합니다.
        /// </summary>
        public void BindWeaponEvents()
        {
            foreach (var weapon in _playerWeaponManager.Weapons)
            {
                var w = weapon;
                weapon.OnFired += () => OnPlayerWeaponFired(w);
            }

            var enemyWeapons = _enemyShipAPI.GetAllWeapons();
            if (enemyWeapons == null) return;
            foreach (var weapon in enemyWeapons)
            {
                var w = weapon;
                weapon.OnFired += () => OnEnemyWeaponFired(w);
            }
        }

        /// <summary>
        /// 적군 무기 자동 발사 처리. EnemyCombatManager.OnTickUpdate()에서 호출합니다.
        /// </summary>
        public void TickEnemyWeapons()
        {
            var enemyWeapons = _enemyShipAPI.GetAllWeapons();
            if (enemyWeapons == null) return;
            foreach (var weapon in enemyWeapons)
            {
                if (weapon.IsPowered && weapon.IsReadyToFire)
                    weapon.TryFire();
            }
        }

        // ─── ITickable ──────────────────────────────────────────────────
        /// <summary>매 틱 PendingAttack 카운트다운. 만료된 항목은 피해 적용.</summary>
        public void OnTickUpdate()
        {
            for (int i = _pendingAttacks.Count - 1; i >= 0; i--)
            {
                var attack = _pendingAttacks[i];
                attack.TicksRemaining--;
                if (attack.TicksRemaining > 0) continue;

                var shield = attack.TargetShipAPI.GetShieldLogic();
                foreach (var hit in attack.Hits)
                {
                    if (hit.ReservedShield)
                    {
                        shield?.ApplyReservedAbsorption();
                        Debug.Log($"[CombatResolver] 발사체 도달 — 실드 흡수 적용.");
                    }
                    else if (hit.Damage > 0)
                    {
                        attack.TargetShipAPI.TakeDamage(hit.Damage);
                        Debug.Log($"[CombatResolver] 발사체 도달 — {hit.Damage} 피해 → HP: {attack.TargetShipAPI.CurrentHullHealth}");
                    }
                }
                _pendingAttacks.RemoveAt(i);
            }
        }
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// 큐에 남은 공격을 모두 버리고 양쪽 실드의 PendingAbsorption을 초기화합니다.
        /// 전투 종료 시 EnemyCombatManager.EndCombat()에서 호출합니다.
        /// </summary>
        public void ClearPendingAttacks()
        {
            _playerShipAPI.GetShieldLogic()?.ResetPendingAbsorption();
            _enemyShipAPI.GetShieldLogic()?.ResetPendingAbsorption();
            _pendingAttacks.Clear();
        }

        // ─── 이벤트 핸들러 ──────────────────────────────────────────────
        private void OnPlayerWeaponFired(IWeaponLogic weapon)
        {
            if (weapon.Data.TargetRoomID == -1) return;
            PreCalculateAndEnqueue(_enemyShipAPI, weapon, weapon.Data.TargetRoomID);
        }

        private void OnEnemyWeaponFired(IWeaponLogic weapon)
        {
            var rooms = _playerShipAPI.GetAllRooms();
            if (rooms == null || rooms.Count == 0) return;
            int idx = _random.Next(rooms.Count);
            int targetRoomID = rooms[idx].Data.RoomID;
            Debug.Log($"[CombatResolver] 적군이 아군 방 [{targetRoomID}] 공격 예약.");
            PreCalculateAndEnqueue(_playerShipAPI, weapon, targetRoomID);
        }
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// 발사체 결과를 미리 계산하고 PendingAttack 큐에 삽입합니다.
        /// Laser: 실드를 예약(TryReserveShield) — CurrentShields 시각 유지, PendingAbsorption 증가.
        /// Missile: 실드 무시.
        /// </summary>
        private void PreCalculateAndEnqueue(IShipAPI targetAPI, IWeaponLogic weapon, int targetRoomID)
        {
            var shield = targetAPI.GetShieldLogic();
            var hits = new List<HitResult>();

            for (int i = 0; i < weapon.BaseData.ProjectileCount; i++)
            {
                bool reserved = weapon.BaseData.Type == WeaponType.Laser
                                && shield != null
                                && shield.TryReserveShield();

                hits.Add(new HitResult
                {
                    ReservedShield = reserved,
                    Damage = reserved ? 0 : weapon.BaseData.Damage
                });
            }

            var pending = new PendingAttack
            {
                TargetShipAPI  = targetAPI,
                Hits           = hits,
                TicksRemaining = PROJECTILE_TRAVEL_TICKS,
                SourceWeapon   = weapon,
                TargetRoomID   = targetRoomID,
                WeaponType     = weapon.BaseData.Type
            };
            _pendingAttacks.Add(pending);
            OnAttackQueued?.Invoke(pending);

            Debug.Log($"[CombatResolver] 발사체 {hits.Count}발 큐 삽입 — {PROJECTILE_TRAVEL_TICKS}틱 후 적용.");
        }
    }
}
