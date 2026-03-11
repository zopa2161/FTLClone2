using Core.Interface;
using UnityEngine;

namespace Presentation.Views
{
    /// <summary>
    /// 실드 시각화 뷰 (프레임).
    /// ShieldRoomLogic의 OnShieldChanged 이벤트를 구독하여 실드 상태를 화면에 반영합니다.
    /// </summary>
    public class ShieldView : MonoBehaviour
    {
        private IShieldLogic _logic;

        public void Bind(IShieldLogic shieldLogic)
        {
            _logic = shieldLogic;
            _logic.OnShieldChanged += HandleShieldChanged;

            // 초기 상태 반영
            RefreshVisuals(_logic.CurrentShields, _logic.MaxShields, _logic.ChargeGauge);
        }

        private void OnDestroy()
        {
            if (_logic != null)
                _logic.OnShieldChanged -= HandleShieldChanged;
        }

        private void HandleShieldChanged(int current, int max, float chargeGauge)
        {
            RefreshVisuals(current, max, chargeGauge);
        }

        private void RefreshVisuals(int current, int max, float chargeGauge)
        {
            // TODO: 실드 시각화 구현 예정
            // - max 개의 실드 아이콘 중 current 개를 활성화
            // - chargeGauge (0~1) 로 충전 게이지 표시
        }
    }
}
