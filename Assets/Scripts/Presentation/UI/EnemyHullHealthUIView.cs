using Core.Interface;
using UnityEngine;
using UnityEngine.UIElements;

namespace Presentation.Views.UI
{
    public class EnemyHullHealthUIView : MonoBehaviour
    {
        public UIDocument Document;

        private const float GREEN_THRESHOLD  = 0.66f;
        private const float YELLOW_THRESHOLD = 0.33f;

        private VisualElement _panel;
        private VisualElement[] _bars;
        private IShipAPI _enemyAPI;

        public void ShowEnemy(IShipAPI enemyAPI)
        {
            _enemyAPI = enemyAPI;
            _panel = Document.rootVisualElement.Q("EnemyHullHealthPanel");
            if (_panel == null)
            {
                Debug.LogWarning("[EnemyHullHealthUIView] EnemyHullHealthPanel을 찾을 수 없습니다.");
                return;
            }

            _panel.Clear();
            int max = enemyAPI.MaxHullHealth;
            _bars = new VisualElement[max];
            for (int i = 0; i < max; i++)
            {
                var bar = new VisualElement();
                bar.AddToClassList("hull-bar");
                _panel.Add(bar);
                _bars[i] = bar;
            }

            _panel.style.display = DisplayStyle.Flex;
            Refresh(enemyAPI.CurrentHullHealth, max);

            enemyAPI.OnHullHealthChanged += OnHealthChanged;
        }

        public void HideEnemy()
        {
            if (_enemyAPI != null)
            {
                _enemyAPI.OnHullHealthChanged -= OnHealthChanged;
                _enemyAPI = null;
            }

            if (_panel != null)
            {
                _panel.Clear();
                _panel.style.display = DisplayStyle.None;
                _bars = null;
            }
        }

        private void OnHealthChanged(int current, int max) => Refresh(current, max);

        private void Refresh(int current, int max)
        {
            if (_bars == null || max <= 0) return;

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
