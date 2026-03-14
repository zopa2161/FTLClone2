using System.Collections.Generic;
using Core.Interface;
using Core.enums;
using UnityEngine;
using Random = System.Random;
namespace Logic.System
{
    /// <summary>
    /// 무기 발사(OnFired)와 실제 피해 적용 사이를 중계합니다.
    /// - 아군 무기 OnFired → 적군 HP 감소
    /// - 적군 무기 자동 발사(TickEnemyWeapons) → 아군 HP 감소
    /// 무기 타입별 실드 상호작용: Laser는 발사체마다 실드 체크, Missile은 실드 무시.
    /// </summary>
    public class CombatResolver
    {
        private readonly IShipAPI _playerShipAPI;
        private readonly IShipAPI _enemyShipAPI;
        private readonly WeaponManager _playerWeaponManager;
        private readonly Random _random = new Random();

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

        private void OnPlayerWeaponFired(IWeaponLogic weapon)
        {
            if (weapon.Data.TargetRoomID == -1) return;
            ApplyDamage(_enemyShipAPI, weapon);
        }

        private void OnEnemyWeaponFired(IWeaponLogic weapon)
        {
            var rooms = _playerShipAPI.GetAllRooms();
            if (rooms == null || rooms.Count == 0) return;
            int idx = _random.Next(rooms.Count);
            Debug.Log($"[CombatResolver] 적군이 아군 방 [{rooms[idx].Data.RoomID}] 공격.");
            ApplyDamage(_playerShipAPI, weapon);
        }

        private void ApplyDamage(IShipAPI targetAPI, IWeaponLogic weapon)
        {
            int hits = weapon.BaseData.ProjectileCount;
            int damage = weapon.BaseData.Damage;
            var shield = targetAPI.GetShieldLogic();

            for (int i = 0; i < hits; i++)
            {
                if (weapon.BaseData.Type == WeaponType.Laser
                    && shield != null && shield.TryAbsorbDamage())
                {
                    Debug.Log($"[CombatResolver] 실드가 {i + 1}번째 타격 흡수.");
                    continue;
                }
                targetAPI.TakeDamage(damage);
                Debug.Log($"[CombatResolver] {i + 1}/{hits}번째 타격 {damage} 피해 → HP: {targetAPI.CurrentHullHealth}");
            }
        }
    }
}
