using System;
using System.Collections.Generic;
using Core.Data.Map;
using Core.Interface;
using UnityEngine;
using UnityEngine.UIElements;

namespace Presentation.UI
{
    /// <summary>
    /// UI Toolkit 기반 맵 화면.
    /// 노드를 버튼으로 렌더링하고, 도달 가능한 노드만 활성화하며
    /// 클릭 시 IMapLogic.MoveToNode()를 호출합니다.
    /// Show() / Hide()로 전체 화면 오버레이를 제어합니다.
    /// </summary>
    public class MapView : MonoBehaviour
    {
        public UIDocument Document;

        private IMapLogic _mapLogic;
        private MapData _mapData;

        private VisualElement _mapOverlay;
        private VisualElement _mapContainer;
        private Button _cancelButton;

        // NodeID → 버튼 요소 (하이라이트 동기화용)
        private readonly Dictionary<string, Button> _nodeButtons = new Dictionary<string, Button>();

        /// <summary>노드를 선택해 점프가 확정됐을 때 발행됩니다. (nodeID 전달)</summary>
        public event Action<string> OnNodeJumped;

        // ─── 초기화 ─────────────────────────────────────────────────────
        public void Initialize(IMapLogic mapLogic, MapData mapData)
        {
            _mapLogic = mapLogic;
            _mapData  = mapData;

            var root = Document.rootVisualElement;
            _mapOverlay   = root.Q<VisualElement>("MapOverlay");
            _mapContainer = root.Q<VisualElement>("MapContainer");
            _cancelButton = root.Q<Button>("MapCancelButton");

            _cancelButton.clicked += Hide;
            _mapLogic.OnNodeChanged += HandleNodeChanged;
        }

        private void OnDestroy()
        {
            if (_mapLogic != null)
                _mapLogic.OnNodeChanged -= HandleNodeChanged;
            if (_cancelButton != null)
                _cancelButton.clicked -= Hide;
        }

        // ─── 표시 제어 ──────────────────────────────────────────────────
        public void Show()
        {
            _mapOverlay.style.display = DisplayStyle.Flex;
            BuildNodeButtons();
            MarkCurrentNode(_mapLogic.CurrentNode);
            HighlightReachable(_mapLogic.GetReachableNodes());
        }

        public void Hide()
        {
            _mapOverlay.style.display = DisplayStyle.None;
        }

        // ─── 렌더링 ─────────────────────────────────────────────────────

        /// <summary>MapData 내 모든 노드를 절대 위치 버튼으로 생성합니다.</summary>
        private void BuildNodeButtons()
        {
            _nodeButtons.Clear();
            _mapContainer.Clear();

            foreach (var node in GetAllNodes())
            {
                var btn = new Button();
                btn.AddToClassList("node-button");
                btn.AddToClassList(node.Type.ToString().ToLower()); // "normal", "store", etc.

                // 정규화 좌표 → 컨테이너 내 절대 위치
                btn.style.position = Position.Absolute;
                btn.style.left     = Length.Percent(node.X * 100f);
                btn.style.top      = Length.Percent(node.Y * 100f);

                var capturedID = node.NodeID;
                btn.clicked += () => OnNodeButtonClicked(capturedID);

                _mapContainer.Add(btn);
                _nodeButtons[node.NodeID] = btn;
            }
        }

        /// <summary>reachable 노드만 활성화합니다. 나머지는 비활성화.</summary>
        private void HighlightReachable(IReadOnlyList<NodeData> reachableNodes)
        {
            foreach (var btn in _nodeButtons.Values)
            {
                btn.RemoveFromClassList("reachable");
                btn.SetEnabled(false);
            }

            foreach (var node in reachableNodes)
            {
                if (_nodeButtons.TryGetValue(node.NodeID, out var btn))
                {
                    btn.AddToClassList("reachable");
                    btn.SetEnabled(true);
                }
            }
        }

        /// <summary>현재 노드 버튼에 "current" 클래스를 표시합니다.</summary>
        private void MarkCurrentNode(NodeData node)
        {
            foreach (var btn in _nodeButtons.Values)
                btn.RemoveFromClassList("current");

            if (node != null && _nodeButtons.TryGetValue(node.NodeID, out var currentBtn))
                currentBtn.AddToClassList("current");
        }

        // ─── 이벤트 핸들러 ──────────────────────────────────────────────
        private void OnNodeButtonClicked(string nodeID)
        {
            if (_mapLogic.MoveToNode(nodeID))
            {
                OnNodeJumped?.Invoke(nodeID);
                Hide();
            }
        }

        private void HandleNodeChanged(NodeData newNode)
        {
            MarkCurrentNode(newNode);
            HighlightReachable(_mapLogic.GetReachableNodes());
        }

        // ─── 헬퍼 ───────────────────────────────────────────────────────
        private IEnumerable<NodeData> GetAllNodes() => _mapData.Nodes;
    }
}
