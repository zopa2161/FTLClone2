using System.Collections.Generic;
using Core.Data.Map;
using Core.Interface;
using UnityEngine;
using UnityEngine.UIElements;

namespace Presentation.UI
{
    /// <summary>
    /// UI Toolkit 기반 맵 화면.
    /// 노드를 버튼으로 렌더링하고, 도달 가능한 노드를 강조하며
    /// 클릭 시 IMapLogic.MoveToNode()를 호출합니다.
    /// CrewSystemUIView / ShieldSystemUIView와 동일한 Initialize() 패턴을 사용합니다.
    /// </summary>
    public class MapView : MonoBehaviour
    {
        public UIDocument Document;

        private IMapLogic _mapLogic;
        private VisualElement _mapContainer;

        // NodeID → 버튼 요소 (하이라이트 동기화용)
        private readonly Dictionary<string, Button> _nodeButtons = new Dictionary<string, Button>();

        // ─── 초기화 ─────────────────────────────────────────────────────
        public void Initialize(IMapLogic mapLogic)
        {
            _mapLogic = mapLogic;

            var root = Document.rootVisualElement;
            _mapContainer = root.Q<VisualElement>("MapContainer");

            BuildNodeButtons();
            MarkCurrentNode(_mapLogic.CurrentNode);
            HighlightReachable(_mapLogic.GetReachableNodes());

            _mapLogic.OnNodeChanged += HandleNodeChanged;
        }

        private void OnDestroy()
        {
            if (_mapLogic != null)
                _mapLogic.OnNodeChanged -= HandleNodeChanged;
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

        /// <summary>도달 가능한 노드에 "reachable" USS 클래스를 추가합니다.</summary>
        private void HighlightReachable(IReadOnlyList<NodeData> reachableNodes)
        {
            // 모든 버튼에서 reachable 클래스 제거 후 재설정
            foreach (var btn in _nodeButtons.Values)
                btn.RemoveFromClassList("reachable");

            foreach (var node in reachableNodes)
            {
                if (_nodeButtons.TryGetValue(node.NodeID, out var btn))
                    btn.AddToClassList("reachable");
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
            _mapLogic.MoveToNode(nodeID);
        }

        private void HandleNodeChanged(NodeData newNode)
        {
            MarkCurrentNode(newNode);
            HighlightReachable(_mapLogic.GetReachableNodes());
        }

        // ─── 헬퍼 ───────────────────────────────────────────────────────
        private IEnumerable<NodeData> GetAllNodes()
        {
            // MapManager가 보유한 MapData.Nodes 목록을 IMapLogic으로는 직접 접근할 수 없으므로
            // Initialize 시 MapData를 함께 받거나, MapManager 캐스팅을 사용할 것.
            // 현재는 스켈레톤이므로 빈 구현으로 둡니다.
            // TODO: Initialize(IMapLogic, MapData) 시그니처로 변경하거나 IMapData 인터페이스 추가 고려
            yield break;
        }
    }
}
