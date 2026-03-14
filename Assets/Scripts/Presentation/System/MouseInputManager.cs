using System.Collections.Generic;
using Core.enums;
using Logic.System;
using Presentation.Views;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Presentation.System
{
    public class MouseInputManager : MonoBehaviour
    {
        [Header("레이어 마스크 설정")] public LayerMask CrewLayer;
        public LayerMask DoorLayer;
        public LayerMask RoomLayer;

        private LogicCommandManager _commandManager;
        private WeaponManager _weaponManager;
        private RoomView _currentHoveredRoom;
        private CrewView _currentSelectedView;
        private bool _clickCrewUI;

        // 등록된 모든 CrewView (CrewID → CrewView)
        private readonly Dictionary<int, CrewView> _crewViewMap = new();

        // 무기별 현재 타겟 RoomView (weaponIndex → RoomView)
        private readonly Dictionary<int, RoomView> _targetedRoomViewByWeapon = new();

        // ShipSetupManager가 CommandManager를 공유받기 위한 프로퍼티
        public LogicCommandManager CommandManager => _commandManager;

        public void InitializeWeaponManager(WeaponManager weaponManager)
        {
            _weaponManager = weaponManager;
        }

        private void Update()
        {
            HandleHover();
            if (Input.GetMouseButtonDown(0)) HandleLeftClick();
            if (Input.GetMouseButtonDown(1)) HandleRightClick();
        }

        public void Initialize()
        {
            _commandManager = new LogicCommandManager();
            _commandManager.OnSelectionChanged += HandleSelectionChanged;
            _commandManager.OnCrewUIClicked += HnadleCrewUIClicked;
        }

        // ShipSetupManager에서 SetupCrews 완료 후 호출
        public void RegisterCrewViews(List<CrewView> crewViews)
        {
            _crewViewMap.Clear();
            foreach (var view in crewViews)
                _crewViewMap[view.Logic.CrewID] = view;
        }

        private void HandleLeftClick()
        {
        
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject() && _clickCrewUI) 
                {
             
                    _clickCrewUI = false;
                    return; // UI 위라면 인게임 클릭 판정을 즉시 취소!
                }
            }
            
            _clickCrewUI = false;


            
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, CrewLayer);
            if (hit.collider != null)
            {
    
                var clickedView = hit.collider.GetComponent<CrewView>();
                if (clickedView != null && clickedView.Logic != null) _commandManager.SelectCrew(clickedView.Logic, false); // OnSelectionChanged가 하이라이트 처리

                return;
            }
            else
            {  
                _commandManager.DeselectCrew();
            }
        

            var hitDoor = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, DoorLayer);
            if (hitDoor.collider != null)
            {  
                var clickedDoor = hitDoor.collider.GetComponent<DoorView>();
                if (clickedDoor != null && clickedDoor.Logic != null)
                    clickedDoor.Logic.ToggleDoorManual();

                return;
            }

  

            // 적군 방 타겟 설정: 무기가 선택된 상태에서 적군 방 클릭
   
            if (_weaponManager != null && _weaponManager.HasSelectedWeapon)
            {
                var hitRoom = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, RoomLayer);
                if (hitRoom.collider != null)
                {   
                    
                    var clickedRoom = hitRoom.collider.GetComponent<RoomView>();
                    if (clickedRoom != null && clickedRoom.Logic != null && clickedRoom.Faction == Faction.Enemy)
                    {
                        SetWeaponTarget(_weaponManager.SelectedWeaponIndex, clickedRoom);
                    }
                    
                }
                return;
            }
   
        }

        private void HandleRightClick()
        {

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject() && _clickCrewUI) 
                {
                    _clickCrewUI = false;
                    return; // UI 위라면 인게임 클릭 판정을 즉시 취소!
                }
            }
            
            _clickCrewUI = false;



            if (!_commandManager.HasSelectedCrew) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, RoomLayer);

            if (hit.collider != null)
            {
                var clickedRoom = hit.collider.GetComponent<RoomView>();
                if (clickedRoom != null && clickedRoom.Logic != null)
                {
                    _commandManager.OrderMoveToRoom(clickedRoom.Logic);
                    Debug.Log($"[MouseInputManager] 우클릭 감지! 방({clickedRoom.Logic.Data.RoomType})으로 이동 명령 하달.");
                }
            }
            else
            {
                _commandManager.DeselectCrew();
            }


        }

        // OnSelectionChanged 이벤트 수신 → CrewView 하이라이트 동기화
        private void HandleSelectionChanged(Core.Interface.ICrewLogic selectedCrew)
        {
            // 이전 선택 해제
            if (_currentSelectedView != null)
            {
                _currentSelectedView.Highlight(false);
                _currentSelectedView = null;
            }

            if (selectedCrew == null) return;

            // 새 선택 하이라이트
            if (_crewViewMap.TryGetValue(selectedCrew.CrewID, out var view))
            {
                _currentSelectedView = view;
                _currentSelectedView.Highlight(true);
            }
        }

        private void HandleHover()
        {
            bool crewSelected = _commandManager.HasSelectedCrew;
            bool weaponSelected = _weaponManager != null && _weaponManager.HasSelectedWeapon;

            if (!crewSelected && !weaponSelected)
            {
                ClearHoverState();
                return;
            }

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, RoomLayer);

            if (hit.collider != null)
            {
                var hoveredRoom = hit.collider.GetComponent<RoomView>();
                // 승무원 선택 시: 아군 방 하이라이트 / 무기 선택 시: 적군 방 하이라이트
                bool shouldHighlight = (crewSelected && hoveredRoom != null && hoveredRoom.Faction == Faction.Player)
                                    || (weaponSelected && hoveredRoom != null && hoveredRoom.Faction == Faction.Enemy);

                if (shouldHighlight && hoveredRoom != _currentHoveredRoom)
                {
                    ClearHoverState();
                    _currentHoveredRoom = hoveredRoom;
                    _currentHoveredRoom.SetHighlight(true);
                }
                else if (!shouldHighlight)
                {
                    ClearHoverState();
                }
            }
            else
            {
                ClearHoverState();
            }
        }

        private void HnadleCrewUIClicked()
        {
            Debug.Log("작동");
            _clickCrewUI = true;
        }

        private void SetWeaponTarget(int weaponIndex, RoomView newTarget)
        {
            // 이전 타겟 해제
            if (_targetedRoomViewByWeapon.TryGetValue(weaponIndex, out var prevRoom))
                prevRoom.SetTargeted(false);

            _targetedRoomViewByWeapon[weaponIndex] = newTarget;
            newTarget.SetTargeted(true);

            _weaponManager.SetTargetForSelectedWeapon(newTarget.Logic.Data.RoomID);
            Debug.Log($"[MouseInputManager] 무기[{weaponIndex}] 타겟 → Enemy Room {newTarget.Logic.Data.RoomID}");
        }

        private void ClearHoverState()
        {
            if (_currentHoveredRoom != null)
            {
                _currentHoveredRoom.SetHighlight(false);
                _currentHoveredRoom = null;
            }
        }
    }
}
