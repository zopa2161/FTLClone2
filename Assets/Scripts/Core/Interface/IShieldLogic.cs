using System;

namespace Core.Interface
{
    public interface IShieldLogic
    {
        /// <summary>현재 전력량에 따른 최대 실드 수 (전력 2→1, 4→2, 6→3, 8→4)</summary>
        int MaxShields { get; }

        /// <summary>현재 활성 실드 수 (0 ~ MaxShields)</summary>
        int CurrentShields { get; }

        /// <summary>다음 실드 충전 게이지 (0 ~ 1)</summary>
        float ChargeGauge { get; }

        /// <summary>실드 하나를 소모해 피해를 막습니다. 성공 시 true 반환.</summary>
        bool TryAbsorbDamage();

        /// <summary>실드를 MaxShields까지 즉시 충전합니다. 전투 시작 시 초기화용.</summary>
        void RechargeToMax();

        /// <summary>실드 상태가 바뀔 때 발송 (currentShields, maxShields, chargeGauge)</summary>
        event Action<int, int, float> OnShieldChanged;
    }
}
