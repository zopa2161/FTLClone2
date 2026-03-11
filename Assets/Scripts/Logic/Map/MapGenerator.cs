using System;
using System.Collections.Generic;
using Core.Data.Map;

namespace Logic.Map
{
    /// <summary>
    /// 컬럼 기반 절차적 맵을 생성합니다.
    /// GridBuilder처럼 데이터 조립만 담당하고 Unity에 의존하지 않습니다.
    /// </summary>
    public class MapGenerator
    {
        private Random _random = new Random();

        // ─── 진입점 ─────────────────────────────────────────────────────
        /// <summary>
        /// 컬럼×행 구조의 MapData를 생성합니다.
        /// </summary>
        /// <param name="columns">총 컬럼 수 (col 0 = Start, col-1 = Exit)</param>
        /// <param name="maxRowsPerColumn">컬럼당 최대 노드 수 (1~3 권장)</param>
        /// <param name="mapWidth">맵 가로 크기 (UI 단위)</param>
        /// <param name="mapHeight">맵 세로 크기 (UI 단위)</param>
        public MapData GenerateMap(int columns, int maxRowsPerColumn, float mapWidth, float mapHeight)
        {
            var mapData = new MapData
            {
                MapWidth  = mapWidth,
                MapHeight = mapHeight,
                Columns   = columns
            };

            var columnGroups = PlaceNodes(columns, maxRowsPerColumn, mapWidth, mapHeight);
            ConnectColumns(columnGroups);

            // 평면 리스트로 합산
            foreach (var col in columnGroups)
                mapData.Nodes.AddRange(col);

            // 시작 노드 설정
            if (columnGroups.Count > 0 && columnGroups[0].Count > 0)
            {
                var startNode = columnGroups[0][0];
                startNode.Type = NodeType.Start;
                mapData.CurrentNodeID = startNode.NodeID;
            }

            // 출구 노드 설정
            if (columnGroups.Count > 0)
            {
                var lastCol = columnGroups[columnGroups.Count - 1];
                foreach (var node in lastCol)
                    node.Type = NodeType.Exit;
            }

            return mapData;
        }

        // ─── 내부 단계 ──────────────────────────────────────────────────

        /// <summary>각 컬럼에 1 ~ maxRowsPerColumn개의 노드를 세로 방향으로 배치합니다.</summary>
        private List<List<NodeData>> PlaceNodes(int columns, int maxRowsPerColumn, float mapWidth, float mapHeight)
        {
            var columnGroups = new List<List<NodeData>>();

            for (int col = 0; col < columns; col++)
            {
                int rowCount = _random.Next(1, maxRowsPerColumn + 1);
                var colNodes = new List<NodeData>();

                // 컬럼 X 위치: 0 ~ 1 정규화
                float xNorm = columns <= 1 ? 0.5f : (float)col / (columns - 1);

                // 행 Y 위치: 세로를 rowCount+1 구간으로 나눠 균등 배치 후 약간 흔들기
                for (int row = 0; row < rowCount; row++)
                {
                    float yBase  = (float)(row + 1) / (rowCount + 1);
                    float jitter = (float)(_random.NextDouble() - 0.5) * (0.5f / (rowCount + 1));
                    float yNorm  = Math.Clamp(yBase + jitter, 0.05f, 0.95f);

                    colNodes.Add(new NodeData
                    {
                        NodeID = MakeNodeID(col, row),
                        Type   = NodeType.Normal,
                        X      = xNorm,
                        Y      = yNorm
                    });
                }

                columnGroups.Add(colNodes);
            }

            return columnGroups;
        }

        /// <summary>
        /// 인접한 두 컬럼의 노드를 연결합니다.
        /// 각 노드가 최소 하나의 연결을 갖도록 보장합니다.
        /// </summary>
        private void ConnectColumns(List<List<NodeData>> columnGroups)
        {
            for (int c = 0; c < columnGroups.Count - 1; c++)
            {
                var left  = columnGroups[c];
                var right = columnGroups[c + 1];

                // 오른쪽 노드 각각에 왼쪽 노드 중 하나를 무조건 연결 (고립 방지)
                foreach (var rNode in right)
                {
                    var lNode = left[_random.Next(left.Count)];
                    if (!lNode.ConnectedNodeIDs.Contains(rNode.NodeID))
                        lNode.ConnectedNodeIDs.Add(rNode.NodeID);
                }

                // 추가 랜덤 연결 (맵 밀도 증가)
                foreach (var lNode in left)
                {
                    if (_random.NextDouble() < 0.4)
                    {
                        var rNode = right[_random.Next(right.Count)];
                        if (!lNode.ConnectedNodeIDs.Contains(rNode.NodeID))
                            lNode.ConnectedNodeIDs.Add(rNode.NodeID);
                    }
                }
            }
        }

        /// <summary>고유 NodeID 문자열을 생성합니다. 예: "node_col2_row0"</summary>
        private string MakeNodeID(int col, int row)
        {
            return $"node_col{col}_row{row}";
        }
    }
}
