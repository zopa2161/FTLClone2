using Core.Interface;
using UnityEngine;
using UnityEngine.UIElements;

namespace Presentation.Views.UI
{
    public class HullHealthUIView : MonoBehaviour
    {
        public UIDocument Document;

        private const float GREEN_THRESHOLD  = 0.66f;
        private const float YELLOW_THRESHOLD = 0.33f;

        private VisualElement[] _bars;

        public void Initialize(IShipAPI shipAPI)
        {
            var root = Document.rootVisualElement;
            var panel = root.Q<VisualElement>("HullHealthPanel");
            if (panel == null)
            {
                Debug.LogWarning("[HullHealthUIView] HullHealthPanel을 찾을 수 없습니다.");
                return;
            }

            int maxHealth = shipAPI.MaxHullHealth;
            _bars = new VisualElement[maxHealth];
            for (int i = 0; i < maxHealth; i++)
            {
                var bar = new VisualElement();
                bar.AddToClassList("hull-bar");
                panel.Add(bar);
                _bars[i] = bar;
            }

            Refresh(shipAPI.CurrentHullHealth, maxHealth);
        }

        private void Refresh(int current, int max)
        {
            if (max <= 0) return;

            float ratio = (float)current / max;
            string colorClass = ratio > GREEN_THRESHOLD  ? "hull-bar-green"
                              : ratio > YELLOW_THRESHOLD ? "hull-bar-yellow"
                              :                            "hull-bar-red";

            for (int i = 0; i < _bars.Length; i++)
            {
                _bars[i].RemoveFromClassList("hull-bar-green");
                _bars[i].RemoveFromClassList("hull-bar-yellow");
                _bars[i].RemoveFromClassList("hull-bar-red");

                if (i < current)
                    _bars[i].AddToClassList(colorClass);
            }
        }
    }
}
