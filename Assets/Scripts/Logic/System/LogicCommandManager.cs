using Core.Data.SpaceShip;
using Core.Interface;
using UnityEngine;

namespace Logic.System
{
    public class LogicCommandManager
    {
        private ICrewLogic _selectedCrew;
        public bool HasSelectedCrew => _selectedCrew != null;


        // 유저가 빈 타일을 우클릭해서 이동 명령을 내렸을 때 (View에서 호출됨)
        public void OrderMoveCommand(TileCoord targetCoord)
        {
            if (_selectedCrew == null) return;

            // 💡 뷰가 직접 승무원의 좌표를 바꾸지 않습니다! "저기로 가라"는 명령만 내립니다.
            _selectedCrew.CommandMoveTo(targetCoord);
        }

        public void SelectCrew(ICrewLogic crew)
        {
            if (_selectedCrew != null) _selectedCrew.OnDied -= HandleSelectedCrewDied;

            _selectedCrew = crew;
            Debug.Log($"[{crew.Data.CrewName}] 승무원 선택됨.");
            if (_selectedCrew != null) _selectedCrew.OnDied += HandleSelectedCrewDied;
        }

        public void DeselectCrew()
        {
            if (_selectedCrew != null)
            {
                Debug.Log("❌ 승무원 선택 해제");
                _selectedCrew = null;
            }
        }

        private void HandleSelectedCrewDied(ICrewLogic deadCrew)
        {
            // 선택 중이던 애가 죽었다면? 즉시 선택 해제!
            _selectedCrew = null;

            // UI에 선택이 풀렸다는 신호 발송
            // OnSelectionCleared?.Invoke(); 
        }
    }
}