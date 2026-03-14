using Core.Interface;
using UnityEngine;

namespace Presentation.Views
{
    /// <summary>
    /// 실드 시각화 뷰.
    /// 실드 수가 0 → 1 이상이 되면 GameObject를 활성화하고,
    /// 1 이상 → 0이 되면 비활성화합니다.
    /// </summary>
    public class ShieldView : MonoBehaviour
    {
        private IShieldLogic _logic;
        private bool _isActive; // 현재 오브젝트 활성 상태 추적

        public void Bind(IShieldLogic shieldLogic)
        {
            Debug.Log("쉴드 바인드");
            _logic = shieldLogic;
            _logic.OnShieldChanged += HandleShieldChanged;

            // 초기 상태 반영
            ApplyActiveState(_logic.CurrentShields);
        }

        private void OnDestroy()
        {
            if (_logic != null)
                _logic.OnShieldChanged -= HandleShieldChanged;
        }

        private void HandleShieldChanged(int current, int max, float chargeGauge)
        {
            // 0 ↔ 1 경계에서만 오브젝트 on/off
            bool shouldBeActive = current > 0;
            if (shouldBeActive != _isActive)
                ApplyActiveState(current);
        }

        private void ApplyActiveState(int currentShields)
        {
            _isActive = currentShields > 0;
            gameObject.SetActive(_isActive);
        }
    }
}
