using Core.Interface;
using UnityEngine;
using UnityEngine.UIElements;

namespace Presentation.Views.UI
{
    public class ShieldSystemUIView : MonoBehaviour
    {
        public UIDocument Document;

        private IShieldLogic _shieldLogic;
        private readonly VisualElement[] _circles = new VisualElement[4];
        private VisualElement _chargeFill;

        public void Initialize(IShieldLogic shieldLogic)
        {
            _shieldLogic = shieldLogic;

            var root = Document.rootVisualElement;
            var overlay = root.Q<VisualElement>("overlay");
            overlay.pickingMode = PickingMode.Ignore;

            var circlesRow = root.Q<VisualElement>("ShieldCirclesRow");
            circlesRow.Clear();
            for (int i = 0; i < 4; i++)
            {
                var circle = new VisualElement();
                circle.AddToClassList("shield-circle");
                circlesRow.Add(circle);
                _circles[i] = circle;
            }

            _chargeFill = root.Q<VisualElement>("ShieldChargeFill");

            if (_shieldLogic != null)
            {
                UpdateUI(_shieldLogic.CurrentShields, _shieldLogic.MaxShields, _shieldLogic.ChargeGauge);
                _shieldLogic.OnShieldChanged += UpdateUI;
            }
        }

        private void OnDestroy()
        {
            if (_shieldLogic != null)
                _shieldLogic.OnShieldChanged -= UpdateUI;
        }

        private void UpdateUI(int current, int max, float chargeGauge)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i < current)
                    _circles[i].AddToClassList("active");
                else
                    _circles[i].RemoveFromClassList("active");
            }

            _chargeFill.style.width = new StyleLength(Length.Percent(chargeGauge * 100f));
        }
    }
}
