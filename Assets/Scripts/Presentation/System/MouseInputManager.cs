using Logic.System;
using Presentation.Views;
using UnityEngine;

// 중간 매니저(PlayerCommandManager) 참조
// CrewView 참조

namespace Presentation.System
{
    public class MouseInputManager : MonoBehaviour
    {
        [Header("레이어 마스크 설정")] public LayerMask CrewLayer;

        public LayerMask DoorLayer;

        public LayerMask RoomLayer;

        // 💡 로직 계층의 사령관(중간 매니저)
        private LogicCommandManager _commandManager;
        private RoomView _currentHoveredRoom;

        //===상태값===
        private CrewView _currentSelectedView;

        private void Update()
        {
            HandleHover();

            // 💡 좌클릭: 승무원 선택 (또는 UI 상호작용)
            if (Input.GetMouseButtonDown(0)) HandleLeftClick();
            if (Input.GetMouseButtonDown(1)) HandleRightClick();
        }

        public void Initialize()
        {
            _commandManager = new LogicCommandManager();
        }

        private void HandleLeftClick()
        {
            // 2D 환경이므로 화면 좌표를 월드 좌표로 변환 후 Physics2D 사용
            // (만약 3D 콜라이더를 쓰신다면 Physics.Raycast로 변경하시면 됩니다)
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 1순위: 무조건 '승무원 레이어'만 타겟팅해서 레이저를 쏩니다!
            var hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, CrewLayer);

            if (hit.collider != null)
            {
                // 부딪힌 껍데기(CrewView)를 가져옵니다.
                var clickedView = hit.collider.GetComponent<CrewView>();

                if (clickedView == _currentSelectedView) return;

                if (clickedView != null && clickedView.Logic != null)
                {
                    // 💡 핵심: 뷰가 뷰를 조종하지 않습니다. 
                    // "사령관님, 유저가 이 승무원(Logic)을 선택했습니다!" 라고 보고만 합니다.
                    if (_currentSelectedView != null) _currentSelectedView.Highlight(false);

                    _currentSelectedView = clickedView;
                    _currentSelectedView.Highlight(true);

                    _commandManager.SelectCrew(clickedView.Logic);
                }
            }
            else
            {
                // 허공이나 다른 곳을 클릭했다면 선택 해제

                if (_currentSelectedView != null)
                {
                    // 🌟 시각적 선택 해제
                    _currentSelectedView.Highlight(false);
                    _currentSelectedView = null;
                }

                // 🌟 논리적 선택 해제
                _commandManager.DeselectCrew();
            }

            var hitDoor = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, DoorLayer);
            if (hitDoor.collider != null)
            {
                var clickedDoor = hitDoor.collider.GetComponent<DoorView>();
                if (clickedDoor != null && clickedDoor.Logic != null)
                    // 💡 뷰가 문 로직의 토글 스위치를 누릅니다!
                    clickedDoor.Logic.ToggleDoorManual();
            }
        }

        private void HandleRightClick()
        {
            // 현재 선택된 승무원이 없다면 명령을 내릴 수 없으니 빠른 종료 (최적화)
            if (!_commandManager.HasSelectedCrew) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 💡 이번에는 '방(Room) 레이어'만 타겟으로 레이저를 쏩니다!
            var hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, RoomLayer);

            if (hit.collider != null)
            {
                var clickedRoom = hit.collider.GetComponent<RoomView>();

                if (clickedRoom != null && clickedRoom.Logic != null)
                {
                    // 1. 방 로직에서 타일 리스트를 가져와 첫 번째 타일(0번 인덱스)의 좌표를 뽑습니다.
                    var tileCoords = clickedRoom.Logic.Data.TileCoords;

                    if (tileCoords != null && tileCoords.Count > 0)
                    {
                        var targetCoord = tileCoords[0];

                        // 2. 사령관(PlayerCommandManager)에게 "선택된 애 저기로 보내!" 라고 명령합니다.
                        _commandManager.OrderMoveCommand(targetCoord);

                        Debug.Log(
                            $"[MouseInputManager] 우클릭 감지! 방({clickedRoom.Logic.Data.RoomType})의 첫 번째 타일 {targetCoord.X}, {targetCoord.Y}로 이동 명령 하달.");
                    }
                }
            }
        }

        private void HandleHover()
        {
            // 조건 1: 선택된 승무원이 없으면 하이라이트할 필요가 없습니다.
            if (!_commandManager.HasSelectedCrew)
            {
                ClearHoverState();
                return;
            }

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, RoomLayer);

            if (hit.collider != null)
            {
                var hoveredRoom = hit.collider.GetComponent<RoomView>();

                if (hoveredRoom != null)
                    // 💡 마우스가 '새로운 방'에 들어갔을 때만 처리 (최적화)
                    if (hoveredRoom != _currentHoveredRoom)
                    {
                        ClearHoverState(); // 이전 방의 불을 끄고

                        _currentHoveredRoom = hoveredRoom;
                        _currentHoveredRoom.SetHighlight(true); // 새 방의 불을 켭니다!
                    }
            }
            else
            {
                // 허공에 마우스를 올렸다면 불을 끕니다.
                ClearHoverState();
            }
        }

        // 🛠️ 헬퍼 함수: 현재 켜진 방의 하이라이트를 안전하게 끄는 역할
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