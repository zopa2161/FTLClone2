using System.Collections.Generic;
using Core.Data.Crews;
using Core.Interface;
using Logic.System;
using Presentation.System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Presentation.Views.UI
{
    public class CrewSystemUIView : MonoBehaviour
    {
        public UIDocument Document;

        private LogicCommandManager _commandManager;
        private VisualElement _crewPanel;

        // CrewID → 슬롯 루트 VisualElement
        private readonly Dictionary<int, VisualElement> _slotDict = new();

        // 현재 선택된 CrewID (-1 = 없음)
        private int _selectedCrewID = -1;

        public void Initialize(IReadOnlyList<ICrewLogic> crewLogics, LogicCommandManager commandManager)
        {
            _commandManager = commandManager;

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

            // 외부(게임 월드 클릭 등)에서 선택이 바뀌었을 때 UI 동기화
            _commandManager.OnSelectionChanged += HandleSelectionChanged;
        }

        private void OnDestroy()
        {
            if (_commandManager != null)
                _commandManager.OnSelectionChanged -= HandleSelectionChanged;
        }

        private void CreateCrewSlot(ICrewLogic crew, CrewBaseSO crewBase)
        {
            var slot = new VisualElement();
            slot.AddToClassList("crew-slot");
            _slotDict[crew.CrewID] = slot;

            // 좌측: 초상화
            var portrait = new VisualElement();
            portrait.AddToClassList("crew-portrait");
            if (crewBase != null && crewBase.DefaultSprite != null)
                portrait.style.backgroundImage = new StyleBackground(crewBase.DefaultSprite);

            // 우측: 이름 + 체력바를 세로로 담는 컨테이너
            var infoColumn = new VisualElement();
            infoColumn.AddToClassList("crew-info-column");

            var nameLabel = new Label(crew.Data.CrewName);
            nameLabel.AddToClassList("crew-name");

            var healthBg = new VisualElement();
            healthBg.AddToClassList("crew-health-bg");
            var healthFill = new VisualElement();
            healthFill.AddToClassList("crew-health-fill");
            healthBg.Add(healthFill);

            infoColumn.Add(nameLabel);
            infoColumn.Add(healthBg);

            slot.Add(portrait);
            slot.Add(infoColumn);
            _crewPanel.Add(slot);

            // 초기 체력 렌더링
            UpdateHealthBar(healthFill, crew.CurrentHealth, crew.MaxHealth);

            // 이벤트 구독
            crew.OnHealthChanged += (current, max) => UpdateHealthBar(healthFill, current, max);
            crew.OnDied += _ => HandleCrewDied(crew.CrewID);

            // 슬롯 클릭 → 선택 / 재클릭 → 선택 해제
            var capturedCrewID = crew.CrewID;
            slot.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;
              

                if (_selectedCrewID == capturedCrewID)
                    _commandManager.DeselectCrew();
                else
                    _commandManager.SelectCrew(crew,true);
            });
        }

        // OnSelectionChanged 수신 → 슬롯 시각 동기화
        private void HandleSelectionChanged(ICrewLogic selectedCrew)
        {
            // 이전 선택 해제
            if (_selectedCrewID != -1 && _slotDict.TryGetValue(_selectedCrewID, out var prevSlot))
                prevSlot.RemoveFromClassList("selected");

            if (selectedCrew == null)
            {
                _selectedCrewID = -1;
                return;
            }

            _selectedCrewID = selectedCrew.CrewID;
            if (_slotDict.TryGetValue(_selectedCrewID, out var newSlot))
                newSlot.AddToClassList("selected");
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
