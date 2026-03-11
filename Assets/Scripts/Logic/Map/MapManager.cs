using System;
using System.Collections.Generic;
using Core.Data.Map;
using Core.Interface;

namespace Logic.Map
{
    /// <summary>
    /// IMapLogic 구현체. MapData를 보유하고 노드 간 이동을 처리합니다.
    /// ShipSetupManager에서 Initialize()로 주입받는 패턴을 사용합니다.
    /// </summary>
    public class MapManager : IMapLogic
    {
        private MapData _mapData;

        // ─── IMapLogic ──────────────────────────────────────────────────
        public NodeData CurrentNode { get; private set; }

        public event Action<NodeData> OnNodeChanged;
        // ────────────────────────────────────────────────────────────────

        public void Initialize(MapData mapData)
        {
            _mapData = mapData;
            CurrentNode = FindNodeByID(_mapData.CurrentNodeID);
            if (CurrentNode != null)
                CurrentNode.IsVisited = true;
        }

        public IReadOnlyList<NodeData> GetReachableNodes()
        {
            if (CurrentNode == null) return new List<NodeData>();

            var reachable = new List<NodeData>();
            foreach (var id in CurrentNode.ConnectedNodeIDs)
            {
                var node = FindNodeByID(id);
                if (node != null && !node.IsVisited)
                    reachable.Add(node);
            }
            return reachable;
        }

        public bool MoveToNode(string nodeID)
        {
            var reachable = GetReachableNodes();
            NodeData target = null;
            foreach (var node in reachable)
            {
                if (node.NodeID == nodeID) { target = node; break; }
            }

            if (target == null) return false;

            target.IsVisited = true;
            CurrentNode = target;
            _mapData.CurrentNodeID = nodeID;

            OnNodeChanged?.Invoke(CurrentNode);
            return true;
        }

        // ─── 내부 헬퍼 ─────────────────────────────────────────────────
        private NodeData FindNodeByID(string nodeID)
        {
            if (string.IsNullOrEmpty(nodeID)) return null;
            foreach (var node in _mapData.Nodes)
            {
                if (node.NodeID == nodeID) return node;
            }
            return null;
        }
    }
}
