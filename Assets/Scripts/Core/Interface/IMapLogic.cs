using System;
using System.Collections.Generic;
using Core.Data.Map;

namespace Core.Interface
{
    public interface IMapLogic
    {
        /// <summary>플레이어가 현재 위치한 노드</summary>
        NodeData CurrentNode { get; }

        /// <summary>현재 노드에서 이동 가능한 노드 목록 (연결되어 있고 아직 미방문)</summary>
        IReadOnlyList<NodeData> GetReachableNodes();

        /// <summary>지정한 nodeID로 이동. 도달 불가능한 노드이면 false 반환</summary>
        bool MoveToNode(string nodeID);

        /// <summary>CurrentNode가 변경된 직후 발생. 새 노드를 인자로 전달</summary>
        event Action<NodeData> OnNodeChanged;
    }
}
