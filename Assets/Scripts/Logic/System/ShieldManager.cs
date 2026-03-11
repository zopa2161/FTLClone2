using System;
using Core.Data.SpaceShip;
using Core.Interface;

namespace Logic.System
{
    /// <summary>
    /// 실드 시스템 전담 매니저. IShieldLogic + ITickable 구현.
    /// ShieldRoomLogic의 전력/Manned 상태를 읽어 충전 로직을 직접 처리하며,
    /// 전투 시 피해 흡수 판정도 담당합니다.
    /// </summary>
    public class ShieldManager : IShieldLogic, ITickable
    {
        // 실드 하나 충전 시간: 75틱 = 7.5초 (틱 간격 0.1초 기준)
        private const float BASE_CHARGE_RATE = 1f / 75f;
        private const float MANNED_BONUS = 1.2f;

        private IRoomLogic _shieldRoom;
        private ShieldData _data;

        // ─── IShieldLogic ───────────────────────────────────────────────
        /// <summary>전력 2→1, 4→2, 6→3, 8→4 (최대 4개)</summary>
        public int MaxShields => _shieldRoom != null ? Math.Min(_shieldRoom.CurrentPower / 2, 4) : 0;

        public int CurrentShields => _data.CurrentShieldCount;
        public float ChargeGauge   => _data.ChargeGauge;

        public event Action<int, int, float> OnShieldChanged;
        // ────────────────────────────────────────────────────────────────

        public void Initialize(IRoomLogic shieldRoom, ShieldData shieldData)
        {
            _shieldRoom = shieldRoom;
            _data       = shieldData;
        }

        // ─── ITickable ──────────────────────────────────────────────────
        public void OnTickUpdate()
        {
            int max = MaxShields;

            // 전력 없음 → 실드·게이지 초기화
            if (max <= 0)
            {
                if (_data.CurrentShieldCount > 0 || _data.ChargeGauge > 0f)
                {
                    _data.CurrentShieldCount = 0;
                    _data.ChargeGauge        = 0f;
                    OnShieldChanged?.Invoke(CurrentShields, max, ChargeGauge);
                }
                return;
            }

            // 이미 최대 실드 → 충전 불필요
            if (_data.CurrentShieldCount >= max) return;

            // 충전 진행 (Manned 시 1.2배)
            float rate = BASE_CHARGE_RATE * (_shieldRoom.IsManned ? MANNED_BONUS : 1f);
            _data.ChargeGauge += rate;

            if (_data.ChargeGauge >= 1f)
            {
                _data.ChargeGauge        = 0f;
                _data.CurrentShieldCount = Math.Min(_data.CurrentShieldCount + 1, max);
            }

            OnShieldChanged?.Invoke(CurrentShields, max, ChargeGauge);
        }
        // ────────────────────────────────────────────────────────────────

        /// <summary>피해 한 방을 실드로 흡수합니다. 실드가 없으면 false 반환.</summary>
        public bool TryAbsorbDamage()
        {
            if (_data.CurrentShieldCount <= 0) return false;

            _data.CurrentShieldCount--;
            OnShieldChanged?.Invoke(CurrentShields, MaxShields, ChargeGauge);
            return true;
        }
    }
}
