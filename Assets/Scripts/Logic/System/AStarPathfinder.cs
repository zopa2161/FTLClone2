using System;
using System.Collections.Generic;
using Core.Data.SpaceShip;
using Core.Interface;

namespace Logic.System
{
    public static class AStarPathfinder
    {
        // 💡 오직 계산만 수행하고, 승무원이 한 칸씩 빼서 쓸 수 있도록 Queue를 반환합니다.
        public static Queue<TileCoord> FindPath(IGridMap gridMap, TileCoord start, TileCoord end)
        {
            var pathQueue = new Queue<TileCoord>();

            // 출발점과 도착점이 같으면 제자리이므로 빈 큐 반환
            if (start == end) return pathQueue;

            // 탐색할 타일 목록 (Open List)
            var openSet = new List<TileCoord> { start };

            // 타일이 어디서 왔는지 추적하는 발자국 (경로 역추적용)
            var cameFrom = new Dictionary<TileCoord, TileCoord>();

            // G Cost: 시작점부터 특정 타일까지 이동하는 데 걸린 실제 비용 (한 칸당 무조건 1)
            var gScore = new Dictionary<TileCoord, int>();
            gScore[start] = 0;

            // F Cost: G Cost + H Cost (도착점까지의 예상 비용)
            var fScore = new Dictionary<TileCoord, int>();
            fScore[start] = GetManhattanDistance(start, end);

            while (openSet.Count > 0)
            {
                // 1. OpenSet에서 F 비용이 가장 낮은 타일을 찾아서 현재 위치로 잡습니다.
                var current = openSet[0];
                for (var i = 1; i < openSet.Count; i++)
                {
                    var node = openSet[i];
                    var currentFScore = fScore.ContainsKey(current) ? fScore[current] : int.MaxValue;
                    var nodeFScore = fScore.ContainsKey(node) ? fScore[node] : int.MaxValue;

                    if (nodeFScore < currentFScore) current = node;
                }

                // 2. 목적지에 도착했다면? 발자국을 거슬러 올라가 경로를 완성합니다!
                if (current == end) return ReconstructPath(cameFrom, current);

                // 현재 타일은 탐색을 마쳤으므로 OpenSet에서 제거
                openSet.Remove(current);

                // 3. 현재 타일의 이웃들을 순회합니다. (IGridMap이 주는 연결망만 믿고 갑니다)
                foreach (var neighbor in gridMap.GetConnectedNeighbors(current))
                {
                    // 💡 작성자님의 핵심 기획 반영: 
                    // 문이든 같은 방 타일이든 차별 없이 '한 칸 이동 = 비용 1 증가' 로 고정합니다.
                    var tentativeGScore = gScore[current] + 1;

                    var neighborGScore = gScore.ContainsKey(neighbor) ? gScore[neighbor] : int.MaxValue;

                    // 더 빠르고 좋은 길을 발견했다면 기록을 갱신합니다.
                    if (tentativeGScore < neighborGScore)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + GetManhattanDistance(neighbor, end);

                        // 아직 탐색 목록에 없다면 추가
                        if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                    }
                }
            }

            // while문을 다 돌았는데도 return을 못 했다면 길이 막힌 것입니다. (빈 큐 반환)
            return pathQueue;
        }

        // ==========================================
        // 🛠️ 헬퍼 함수 1: 휴리스틱 (H Cost) 계산
        // 우주선은 대각선 이동이 없으므로 '맨해튼 거리(가로+세로)'를 사용합니다.
        // ==========================================
        private static int GetManhattanDistance(TileCoord a, TileCoord b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        // ==========================================
        // 🛠️ 헬퍼 함수 2: 발자국을 거꾸로 추적하여 Queue 만들기
        // ==========================================
        private static Queue<TileCoord> ReconstructPath(Dictionary<TileCoord, TileCoord> cameFrom, TileCoord current)
        {
            var pathList = new List<TileCoord>();

            // 도착점부터 출발점 직전까지 거꾸로 리스트에 담습니다.
            while (cameFrom.ContainsKey(current))
            {
                pathList.Add(current);
                current = cameFrom[current];
            }

            // 역순으로 담겼으니 뒤집어줍니다.
            pathList.Reverse();

            // 승무원이 한 칸씩 빼서(Dequeue) 먹기 좋게 Queue로 변환합니다.
            var finalQueue = new Queue<TileCoord>();
            foreach (var coord in pathList) finalQueue.Enqueue(coord);

            return finalQueue;
        }
    }
}