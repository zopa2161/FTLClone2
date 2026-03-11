using System;
using System.Collections.Generic;

namespace Core.Data.Map
{
    public enum NodeType
    {
        Start,
        Normal,
        Store,
        Elite,
        Exit
    }

    [Serializable]
    public class NodeData
    {
        public string NodeID;

        public NodeType Type;

        // 정규화된 위치 (0 ~ 1)
        public float X;
        public float Y;

        // 이 노드에서 이동 가능한 다음 노드 ID 목록
        public List<string> ConnectedNodeIDs = new List<string>();

        public bool IsVisited;

        // 방문 시 발생할 MapEventBaseSO의 EventID
        public string EventID;
    }
}
