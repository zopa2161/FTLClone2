using System;
using System.Collections.Generic;

namespace Core.Data.Map
{
    [Serializable]
    public class MapData
    {
        // 맵 생성 시 사용한 단위 크기 (UI 좌표 기준)
        public float MapWidth;
        public float MapHeight;

        // 생성 시 사용한 컬럼 수 (참고용)
        public int Columns;

        // 플레이어가 현재 위치한 노드 ID
        public string CurrentNodeID;

        // 전체 노드 목록 (ShipSaveData의 Tiles/Rooms 패턴과 동일한 평면 리스트)
        public List<NodeData> Nodes = new List<NodeData>();
    }
}
