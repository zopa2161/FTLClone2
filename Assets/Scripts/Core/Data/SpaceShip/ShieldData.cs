using System;

namespace Core.Data.SpaceShip
{
    [Serializable]
    public class ShieldData
    {
        public float ChargeGauge = 0f;     // 충전 중인 게이지 (0 ~ 1)
        public int CurrentShieldCount = 0; // 현재 활성 실드 수 (0 ~ 4)
    }
}
