using System;
using Core.Data.SpaceShip;
using Core.Interface;
using UnityEngine;

namespace Logic.System
{
    public class LogicCommandManager
    {
        private ICrewLogic _selectedCrew;
        public bool HasSelectedCrew => _selectedCrew != null;

        // 선택 상태가 바뀔 때마다 발송 (null = 선택 해제)
        public event Action<ICrewLogic> OnSelectionChanged;
        public event Action OnCrewUIClicked;

        // 유저가 빈 타일을 우클릭해서 이동 명령을 내렸을 때 (View에서 호출됨)
        public void OrderMoveCommand(TileCoord targetCoord)
        {
            if (_selectedCrew == null) return;
            _selectedCrew.CommandMoveTo(targetCoord);
        }

        public void SelectCrew(ICrewLogic crew, bool clickedByUI)
        {
            if (_selectedCrew == crew) return;

            if(clickedByUI) OnCrewUIClicked?.Invoke();

            if (_selectedCrew != null) _selectedCrew.OnDied -= HandleSelectedCrewDied;

            _selectedCrew = crew;
            Debug.Log($"[{crew.Data.CrewName}] 승무원 선택됨.");
            _selectedCrew.OnDied += HandleSelectedCrewDied;

            OnSelectionChanged?.Invoke(_selectedCrew);
        }

        public void DeselectCrew()
        {
            if (_selectedCrew == null) return;

            _selectedCrew.OnDied -= HandleSelectedCrewDied;
            _selectedCrew = null;
            Debug.Log("❌ 승무원 선택 해제");

            OnSelectionChanged?.Invoke(null);
        }

        private void HandleSelectedCrewDied(ICrewLogic deadCrew)
        {
            _selectedCrew = null;
            OnSelectionChanged?.Invoke(null);
        }
    }
}