using System.Collections.Generic;
using System.Linq;
using Core.Interface;
using UnityEngine;

namespace Presentation.System
{
    public class WeaponManager
    {
        private List<IWeaponLogic> _weapons = new List<IWeaponLogic>();

        // 외부에서 장착된 무기 목록을 볼 수 있게 열어둠
        public IReadOnlyList<IWeaponLogic> Weapons => _weapons;

        private IRoomLogic _weaponRoom;

        public void Initialize(List<IWeaponLogic> weaponLogics, IRoomLogic weaponRoom)
        {
            _weapons= weaponLogics;
            _weaponRoom = weaponRoom;
            if (_weaponRoom != null)
            {
                _weaponRoom.OnPowerChanged += HandleRoomPowerChanged;
            }

        }
        public int GetUsedWeaponPower()
        {
            return _weapons.Where(w => w.IsPowered).Sum(w => w.BaseData.RequiredPower);
        }
        
        public int GetAvailableWeaponPower()
        {
            if (_weaponRoom == null) return 0;
            return _weaponRoom.CurrentPower - GetUsedWeaponPower();
        }
        
        
        // ==========================================
        // 🌟 3. 유저의 무기 ON/OFF 요청 처리
        // ==========================================
        public bool TryToggleWeaponPower(int index, bool turnOn)
        {
            var weapon = GetWeapon(index);
            if (weapon == null) return false;

            if (turnOn)
            {
                // 켜려고 할 때는 '남은 잉여 전력'이 무기의 요구량보다 큰지 검사!
                if (GetAvailableWeaponPower() >= weapon.BaseData.RequiredPower)
                {
                    weapon.SetPower(true);
                    return true;
                }
                return false; // 전력 부족으로 실패
            }
            else
            {
                // 끄는 건 언제나 자유롭게 가능
                weapon.SetPower(false);
                return true;
            }
        }

        // ==========================================
        // 🚨 4. 무기 방의 전력이 변했을 때 (오토 셧다운 로직)
        // ==========================================
        private void HandleRoomPowerChanged(int currentRoomPower, int maxRoomPower)
        {
            int usedPower = GetUsedWeaponPower();

            // 만약 유저가 전력을 빼거나 방이 부서져서, 방의 총 전력이 현재 쓰고 있는 전력보다 적어졌다면?
            if (currentRoomPower < usedPower)
            {
                ForceShutdownWeaponsToMatchPower(currentRoomPower);
            }
        }

        // 전력 한도에 맞을 때까지 켜진 무기를 강제로 하나씩 끕니다 (보통 오른쪽 끝 슬롯부터 끔)
        private void ForceShutdownWeaponsToMatchPower(int limitPower)
        {
            int currentUsed = GetUsedWeaponPower();

            for (int i = _weapons.Count - 1; i >= 0; i--)
            {
                var weapon = _weapons[i];
                if (weapon.IsPowered)
                {
                    weapon.SetPower(false);
                    currentUsed -= weapon.BaseData.RequiredPower;

                    Debug.Log($"[WeaponManager] 전력 부족! {weapon.BaseData.WeaponName} 강제 종료됨.");

                    // 한도 내로 들어왔으면 종료
                    if (currentUsed <= limitPower) break;
                }
            }
        }
        
        public IWeaponLogic GetWeapon(int index)
        {
            if (index >= 0 && index < _weapons.Count) return _weapons[index];
            return null;
        }
    }
}