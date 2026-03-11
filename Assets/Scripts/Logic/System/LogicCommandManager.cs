using System;
using System.Collections.Generic;
using Core.Data.SpaceShip;
using Core.Interface;
using UnityEngine;

namespace Logic.System
{
    public class LogicCommandManager
    {
        private ICrewLogic _selectedCrew;
        private IReadOnlyList<ICrewLogic> _allCrews = new List<ICrewLogic>();

        public bool HasSelectedCrew => _selectedCrew != null;

        // 선택 상태가 바뀔 때마다 발송 (null = 선택 해제)
        public event Action<ICrewLogic> OnSelectionChanged;
        public event Action OnCrewUIClicked;

        public void SetAllCrews(IReadOnlyList<ICrewLogic> allCrews)
        {
            _allCrews = allCrews;
        }

        // 유저가 빈 타일을 우클릭해서 이동 명령을 내렸을 때 (View에서 호출됨)
        public void OrderMoveCommand(TileCoord targetCoord)
        {
            if (_selectedCrew == null) return;
            _selectedCrew.CommandMoveTo(targetCoord);
        }

        // 방을 우클릭했을 때: 비어 있는 타일 순서대로 배정
        public void OrderMoveToRoom(IRoomLogic room)
        {
            if (_selectedCrew == null) return;

            var tiles = room.GetRoomTiles();
            if (tiles == null || tiles.Count == 0) return;

            foreach (var tile in tiles)
            {
                bool isOccupied = false;
                foreach (var crew in _allCrews)
                {
                    if (crew == _selectedCrew) continue;
                    if (crew.Data.CurrentX == tile.X && crew.Data.CurrentY == tile.Y)
                    {
                        isOccupied = true;
                        break;
                    }
                }

                if (!isOccupied)
                {
                    _selectedCrew.CommandMoveTo(tile);
                    Debug.Log($"[LogicCommandManager] {room.Data.RoomType} 방 타일({tile.X},{tile.Y})로 이동 명령.");
                    return;
                }
            }

            Debug.Log($"[LogicCommandManager] {room.Data.RoomType} 방이 가득 찼습니다. 이동 불가.");
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