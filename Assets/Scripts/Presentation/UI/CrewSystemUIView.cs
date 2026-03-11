using System.Collections.Generic;
using Core.Data.Crews;
using Core.Interface;
using Presentation.System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Presentation.Views.UI
{
    public class CrewSystemUIView : MonoBehaviour
    {
        public UIDocument Document;

        private VisualElement _crewPanel;

        // 살아있는 동안 이벤트 구독을 추적하기 위한 딕셔너리
        private readonly Dictionary<int, VisualElement> _slotDict = new();

        public void Initialize(IReadOnlyList<ICrewLogic> crewLogics)
        {
            var root = Document.rootVisualElement;

            var overlay = root.Q<VisualElement>("overlay");
            overlay.pickingMode = PickingMode.Ignore;

            _crewPanel = root.Q<VisualElement>("CrewPanel");
            _crewPanel.Clear();
            _slotDict.Clear();

            foreach (var crew in crewLogics)
            {
                var crewBase = AssetCatalogManager.Instance.GetCrewBaseData(crew.Data.BaseDataID);
                CreateCrewSlot(crew, crewBase);
            }
        }

        private void CreateCrewSlot(ICrewLogic crew, CrewBaseSO crewBase)
        {
            // 카드 루트
            var slot = new VisualElement();
            slot.AddToClassList("crew-slot");
            _slotDict[crew.CrewID] = slot;

            // 초상화
            var portrait = new VisualElement();
            portrait.AddToClassList("crew-portrait");
            if (crewBase != null && crewBase.DefaultSprite != null)
                portrait.style.backgroundImage = new StyleBackground(crewBase.DefaultSprite);

            // 이름
            var nameLabel = new Label(crew.Data.CrewName);
            nameLabel.AddToClassList("crew-name");

            // 체력바
            var healthBg = new VisualElement();
            healthBg.AddToClassList("crew-health-bg");

            var healthFill = new VisualElement();
            healthFill.AddToClassList("crew-health-fill");
            healthBg.Add(healthFill);

            // 조립
            slot.Add(portrait);
            slot.Add(nameLabel);
            slot.Add(healthBg);
            _crewPanel.Add(slot);

            // 초기 체력 렌더링
            UpdateHealthBar(healthFill, crew.CurrentHealth, crew.MaxHealth);

            // 이벤트 구독
            crew.OnHealthChanged += (current, max) => UpdateHealthBar(healthFill, current, max);
            crew.OnDied += _ => HandleCrewDied(crew.CrewID);
        }

        private void UpdateHealthBar(VisualElement fill, float current, float max)
        {
            var ratio = max > 0f ? current / max : 0f;
            fill.style.width = new StyleLength(Length.Percent(ratio * 100f));

            if (ratio <= 0.3f)
                fill.AddToClassList("danger");
            else
                fill.RemoveFromClassList("danger");
        }

        private void HandleCrewDied(int crewID)
        {
            if (_slotDict.TryGetValue(crewID, out var slot))
                slot.AddToClassList("dead");
        }
    }
}
