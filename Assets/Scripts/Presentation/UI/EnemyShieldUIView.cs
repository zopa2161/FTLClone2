using System;
using Core.Interface;
using UnityEngine;
using UnityEngine.UIElements;

namespace Presentation.Views.UI
{
    public class EnemyShieldUIView : MonoBehaviour
    {
        public UIDocument Document;

        private IShieldLogic _shieldLogic;
        private VisualElement[] _circles;
        private VisualElement _chargeFill;
        private VisualElement _panel;

        public void ShowEnemy(IShieldLogic shieldLogic)
        {
            _panel = Document.rootVisualElement.Q("EnemyShieldPanel");
            if (_panel == null)
            {
                Debug.LogWarning("[EnemyShieldUIView] EnemyShieldPanel을 찾을 수 없습니다.");
                return;
            }

            if (shieldLogic == null)
            {
                _panel.style.display = DisplayStyle.None;
                return;
            }

            _shieldLogic = shieldLogic;

            var circlesRow = _panel.Q("EnemyShieldCirclesRow");
            circlesRow.Clear();
            int max = shieldLogic.MaxShields;
            _circles = new VisualElement[max];
            for (int i = 0; i < max; i++)
            {
                var circle = new VisualElement();
                circle.AddToClassList("shield-circle");
                circlesRow.Add(circle);
                _circles[i] = circle;
            }

            _chargeFill = _panel.Q("EnemyShieldChargeFill");

            _panel.style.display = DisplayStyle.Flex;
            UpdateUI(shieldLogic.CurrentShields, shieldLogic.MaxShields, shieldLogic.ChargeGauge);
            _shieldLogic.OnShieldChanged += UpdateUI;
        }

        public void HideEnemy()
        {
            if (_shieldLogic != null)
            {
                _shieldLogic.OnShieldChanged -= UpdateUI;
                _shieldLogic = null;
            }

            if (_panel != null)
            {
                _panel.Q("EnemyShieldCirclesRow")?.Clear();
                _panel.style.display = DisplayStyle.None;
                _circles = null;
            }
        }

        private void UpdateUI(int current, int max, float chargeGauge)
        {
            if (_circles == null) return;

            int activeCount = Math.Min(current, _circles.Length);
            for (int i = 0; i < _circles.Length; i++)
            {
                if (i < activeCount)
                    _circles[i].AddToClassList("active");
                else
                    _circles[i].RemoveFromClassList("active");
            }

            if (_chargeFill != null)
                _chargeFill.style.width = new StyleLength(Length.Percent(chargeGauge * 100f));
        }
    }
}
