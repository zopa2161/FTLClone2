using System.Collections.Generic;
using Core.Interface;
using Logic.System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Presentation.Views.UI
{
    public class PowerSystemUIView : MonoBehaviour
    {
        public UIDocument Document;

        // 생성된 바들을 추적하기 위한 리스트
        private readonly List<VisualElement> _reactorBars = new();
        private readonly Dictionary<int, List<VisualElement>> _roomBarsDict = new();
        private LogicCommandManager _commandManager; // (선택) 로직 사령관

        private IPowerSystem _powerSystem;

        // UI 요소 캐싱
        private VisualElement _reactorBarContainer;
        private VisualElement _roomControlsGroup;

        public void Initialize(IPowerSystem powerSystem, IEnumerable<IRoomLogic> rooms,
            LogicCommandManager commandManager)
        {

            _powerSystem = powerSystem;
            _commandManager = commandManager;

            var root = Document.rootVisualElement;
             var overlay = root.Q<VisualElement>("overlay");
            overlay.pickingMode = PickingMode.Ignore;
            
            _reactorBarContainer = root.Q<VisualElement>("ReactorBarContainer");
            _roomControlsGroup = root.Q<VisualElement>("RoomControlsGroup");

            // 1. 좌측 메인 전력 바 생성 (Max - 3)
            var displayReactorBars = _powerSystem.MaxReactorPower - 3;
            for (var i = 0; i < displayReactorBars; i++)
            {
                var bar = new VisualElement();
                bar.AddToClassList("reactor-bar");
                _reactorBarContainer.Add(bar);
                _reactorBars.Add(bar);
            }

            // 2. 우측 방 컨트롤 동적 생성 (S, E, M, O, W 순서대로 rooms가 들어왔다고 가정)
            foreach (var room in rooms)
            {
                // 전력이 필요 없는 방(MaxCapacity == 0)은 UI를 만들지 않습니다.
                if (room.MaxPowerCapacity <= 0) continue;
                CreateRoomControlUI(room);
            }

            // 3. 무전기 구독 및 초기 렌더링
            _powerSystem.OnReactorPowerChanged += UpdateReactorUI;
            UpdateReactorUI(_powerSystem.AvailableReactorPower, _powerSystem.MaxReactorPower);
        }

        private void CreateRoomControlUI(IRoomLogic room)
        {
            // 방 기둥 컨테이너
            var column = new VisualElement();
            column.AddToClassList("room-control-column");

            // 1. 동그란 버튼 생성
            var button = new Label(); // Button 대신 Label을 쓰고 이벤트를 직접 달아줍니다
            button.AddToClassList("room-button");
            // 방 이름의 첫 글자 추출 (Shield -> S, Engine -> E)
            button.text = room.Data.RoomType.Substring(0, 1).ToUpper();

            // 💡 좌클릭(할당) 우클릭(회수) 이벤트 등록
            button.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 0) // 좌클릭 (0)
                    _powerSystem.TryAddPowerToRoom(room.Data.RoomID);
                else if (evt.button == 1) // 우클릭 (1)
                    _powerSystem.TryRemovePowerFromRoom(room.Data.RoomID);
            });

            // 2. 방 바(Bar) 컨테이너 생성
            var barContainer = new VisualElement();
            barContainer.AddToClassList("room-bar-container");

            var bars = new List<VisualElement>();
            for (var i = 0; i < room.MaxPowerCapacity; i++)
            {
                var bar = new VisualElement();
                bar.AddToClassList("room-bar");
                barContainer.Add(bar);
                bars.Add(bar);
            }

            _roomBarsDict[room.Data.RoomID] = bars;

            // 조립: 기둥에 바 컨테이너와 버튼을 넣고, 그룹에 추가
            column.Add(barContainer);
            column.Add(button);
            _roomControlsGroup.Add(column);

            // 3. 방 전력 무전기 구독 및 초기 렌더링
            room.OnPowerChanged += (current, max) => UpdateRoomUI(room.Data.RoomID, current);
            UpdateRoomUI(room.Data.RoomID, room.CurrentPower);
        }

        // ==========================================
        // 💡 렌더링(색상 채우기) 로직
        // ==========================================
        private void UpdateReactorUI(int availablePower, int maxPower)
        {
            // 보여줄 바의 개수 (예: Max가 10이면 7개만 그림)
            var totalBars = _reactorBars.Count;

            // 주의: 어딘가 다른 3곳에서 고정으로 전력을 쓰고 있다고 가정할 때의 계산입니다.
            // 잉여 전력만큼 아래에서부터(column-reverse 덕분에 index 0부터 채우면 됨) 불을 켭니다.
            for (var i = 0; i < totalBars; i++)
                if (i < availablePower) _reactorBars[i].AddToClassList("filled");
                else _reactorBars[i].RemoveFromClassList("filled");
        }

        private void UpdateRoomUI(int roomID, int currentPower)
        {
            if (_roomBarsDict.TryGetValue(roomID, out var bars))
                for (var i = 0; i < bars.Count; i++)
                    if (i < currentPower) bars[i].AddToClassList("filled");
                    else bars[i].RemoveFromClassList("filled");
        }
    }
}