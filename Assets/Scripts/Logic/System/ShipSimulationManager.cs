using System.Collections.Generic;
using Core.Data.SpaceShip;
using Core.Interface;
using Logic.SpaceShip;
using Logic.SpaceShip.Rooms;
using UnityEngine;
using Random = System.Random;

namespace Logic.System
{
    public class ShipSimulationManager : ITickable
    {
        private const float WIND_SPEED_MULTIPLIER = 0.15f;
        private const float DROP_MULTIPLIER = 2.0f;
        private const float MIN_TRANSFER_AMOUNT = 0.5f;

        // 확산 속도 (1틱당 인접 타일과 농도 차이의 몇 %를 교환할 것인가)
        private const float DIFFUSION_RATE = 0.05f;

        // 화재 시뮬레이션 상수
        private const float FIRE_SPREAD_CHANCE    = 0.01f; // 틱당 전파 확률
        private const float FIRE_GROWTH_RATE      = 0.5f;  // 틱당 강화량
        private const float FIRE_DECAY_RATE       = 0.5f;  // 틱당 진화량
        private const float FIRE_INITIAL_LEVEL    = 10f;   // 전파 시 초기 화재값
        private const float FIRE_OXYGEN_THRESHOLD = 50f;   // 전파·강화 기준 산소값
        private readonly Random _random    = new Random();

        private readonly OxygenRoomLogic _oxygenRoom;
        private readonly SpaceShipManager _shipManager;

        public ShipSimulationManager(SpaceShipManager shipManager)
        {
            _shipManager = shipManager;
            _oxygenRoom = shipManager.GetOxygenRoom();
        }

        public void OnTickUpdate()
        {
            // 1. 브리치 목록 찾기
            var breaches = GetBreachTiles();

            if (breaches.Count > 0)
            {
                // 2. 브리치를 향한 '거리 지도(Flow Field)' 만들기
                var distanceMap = BuildDistanceMap(breaches);

                // 3. 거리에 비례하여 공기 이동시키기 (바람)
                SimulateVacuumWind(distanceMap);
            }
            // 브리치가 없으면 평소처럼 잔잔한 확산(Diffusion) 로직 실행
            // SimulateOxygenDiffusion();

            SimulateFire();
        }

        private void SimulateFire()
        {
            var newFires = new List<TileCoord>(); // 더블 버퍼링: 이번 틱에 새로 붙을 타일

            foreach (var tile in _shipManager.GetAllTiles())
            {
                if (tile.FireLevel <= 0f) continue;

                var room = _shipManager.GetRoomAt(tile.TileCoord);
                float roomOxygen = room?.AverageOxygen ?? 0f;

                // ─── 전파 (방 평균 산소 >= 50) ──────────────────────────────
                if (roomOxygen >= FIRE_OXYGEN_THRESHOLD)
                {
                    foreach (var neighborCoord in _shipManager.GetConnectedNeighbors(tile.TileCoord))
                    {
                        var door = _shipManager.GetDoorBetween(tile.TileCoord, neighborCoord);
                        if (door != null && !door.IsOpen) continue;

                        var neighbor = _shipManager.GetTileAt(neighborCoord);
                        if (neighbor.FireLevel > 0f) continue;

                        if (_random.NextDouble() < FIRE_SPREAD_CHANCE && !newFires.Contains(neighborCoord))
                            newFires.Add(neighborCoord);
                    }
                }

                // ─── 강화 / 진화 (타일 산소 기준) ───────────────────────────
                if (tile.OxygenLevel >= FIRE_OXYGEN_THRESHOLD)
                    tile.FireLevel = Mathf.Min(100f, tile.FireLevel + FIRE_GROWTH_RATE);
                else
                    tile.FireLevel = Mathf.Max(0f, tile.FireLevel - FIRE_DECAY_RATE);
            }

            // 새 불 일괄 적용
            foreach (var coord in newFires)
                _shipManager.GetTileAt(coord).FireLevel = FIRE_INITIAL_LEVEL;
        }

        private List<ITileLogic> GetBreachTiles()
        {
            List<ITileLogic> result = new();
            foreach (var tile in _shipManager.GetAllTiles())
                if (tile.BreachLevel > 1f)
                    result.Add(tile);

            return result;
        }

        private Dictionary<TileCoord, int> BuildDistanceMap(List<ITileLogic> breaches)
        {
            var distanceMap = new Dictionary<TileCoord, int>();
            var queue = new Queue<TileCoord>();

            // 브리치들을 거리 0으로 세팅하고 출발점에 넣음
            foreach (var breach in breaches)
            {
                distanceMap[breach.TileCoord] = 0;
                queue.Enqueue(breach.TileCoord);
            }

            // 물결 퍼지듯(BFS) 거리 기록
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentDist = distanceMap[current];

                foreach (var neighbor in _shipManager.GetConnectedNeighbors(current))
                {
                    // 닫힌 문은 통과할 수 없음 (바람이 막힘)
                    var door = _shipManager.GetDoorBetween(current, neighbor);
                    if (door != null && !door.IsOpen) continue;

                    // 아직 방문 안 한 타일이면 거리를 +1 해서 맵에 기록
                    if (!distanceMap.ContainsKey(neighbor))
                    {
                        distanceMap[neighbor] = currentDist + 1;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return distanceMap;
        }

        private void SimulateVacuumWind(Dictionary<TileCoord, int> distanceMap)
        {
            var oxygenChanges = new Dictionary<TileCoord, float>();

            var generatedOxygen = _oxygenRoom.GetOxygenGeneration();

            foreach (var tile in _shipManager.GetAllTiles()) oxygenChanges[tile.TileCoord] = generatedOxygen;
            foreach (var kvp in distanceMap)
            {
                var myCoord = kvp.Key;
                var myDist = kvp.Value;
                var myTile = _shipManager.GetTileAt(myCoord);

                // 브리치 당사자(거리 0)는 우주로 공기를 즉시 증발시킴
                if (myDist == 0)
                {
                    if (!oxygenChanges.ContainsKey(myCoord)) oxygenChanges[myCoord] = 0f;
                    oxygenChanges[myCoord] -= myTile.OxygenLevel * 0.5f; // 자비없이 날아감
                    continue;
                }

                // 💡 핵심: 내 주변 타일 중 "나보다 거리가 짧은(브리치에 가까운) 타일" 찾기
                var bestTarget = myCoord;
                var shortestDist = myDist;


                foreach (var neighbor in _shipManager.GetConnectedNeighbors(myCoord))
                    if (distanceMap.TryGetValue(neighbor, out var neighborDist))
                        if (neighborDist < shortestDist)
                        {
                            shortestDist = neighborDist;
                            bestTarget = neighbor;
                        }

                // 나보다 브리치에 가까운 타일(bestTarget)이 있다면 그쪽으로 공기를 몰아줌!
                if (bestTarget != myCoord)
                {
                    // 거리가 멀수록(myDist가 클수록) 적게, 가까울수록 많이 빨려가게 수학적 계산 가능
                    var suckAmount = myTile.OxygenLevel * WIND_SPEED_MULTIPLIER;

                    if (!oxygenChanges.ContainsKey(myCoord)) oxygenChanges[myCoord] = 0f;
                    if (!oxygenChanges.ContainsKey(bestTarget)) oxygenChanges[bestTarget] = 0f;

                    oxygenChanges[myCoord] -= suckAmount;
                    oxygenChanges[bestTarget] += suckAmount;
                }
            }

            // 버퍼 일괄 적용 (기존 코드와 동일)
            // ...
            // 🌟 4. 모든 계산이 끝난 후, 버퍼에 담긴 변화량을 실제 데이터에 한 번에 적용합니다.


            foreach (var kvp in oxygenChanges)
            {
                var tile = _shipManager.GetTileAt(kvp.Key);
                tile.OxygenLevel += kvp.Value;

                // 산소량은 0~100 사이를 유지하도록 클램핑
                tile.OxygenLevel = Mathf.Clamp(tile.OxygenLevel, 0f, 100f);
            }
        }

        private void SimulateOxygenDiffusion()
        {
            // 🌟 핵심: '동시성 오류'와 '방향 편향성'을 막기 위한 변화량 버퍼 (Double Buffering)
            // 이번 틱에 각 타일의 산소가 얼마나 더해지고 빠질지만 기록해 둡니다.
            var oxygenChanges = new Dictionary<TileCoord, float>();

            var generatedOxygen = _oxygenRoom.GetOxygenGeneration();
            // (주의: SpaceShipManager 안에 GetAllTileLogics() 같은 원본 딕셔너리를 반환하는 함수가 있다고 가정합니다)
            //Debug.Log($"{_shipManager.GetAllTiles().Count}");
            foreach (var currentTile in _shipManager.GetAllTiles())
            {
                //기본 산소 제공

                oxygenChanges[currentTile.TileCoord] = +generatedOxygen;

                if (currentTile.BreachLevel > 1f) oxygenChanges[currentTile.TileCoord] = -100;

                var currentCoord =
                    new TileCoord((currentTile as TileLogic).Data.X, (currentTile as TileLogic).Data.Y);
                if (!oxygenChanges.ContainsKey(currentCoord)) oxygenChanges[currentCoord] = 0f;

                // 1. 내 주변의 연결된 타일들을 가져옵니다.
                foreach (var neighborCoord in _shipManager.GetConnectedNeighbors(currentCoord))
                {
                    var neighborTile = _shipManager.GetTileAt(neighborCoord);

                    var door = _shipManager.GetDoorBetween(currentCoord, neighborCoord);
                    if (door != null && !door.IsOpen) continue;

                    // 3. 농도 차이 계산 (내 산소가 더 많을 때만 나눠줍니다)
                    var diff = currentTile.OxygenLevel - neighborTile.OxygenLevel;

                    if (diff > 0)
                    {
                        var transferAmount = diff * DIFFUSION_RATE;

                        // 🌟 핵심: 계산된 값이 너무 작으면, '최소 보장량'으로 끌어올립니다!
                        if (transferAmount < MIN_TRANSFER_AMOUNT) transferAmount = MIN_TRANSFER_AMOUNT;

                        // 🛡️ 방어 코드: 단, 주려는 양이 격차의 절반보다 크면 산소량이 역전(핑퐁 진동)되므로 제한을 겁니다.
                        transferAmount = Mathf.Min(transferAmount, diff / 2f);

                        // 적용
                        oxygenChanges[currentCoord] -= transferAmount;

                        if (!oxygenChanges.ContainsKey(neighborCoord)) oxygenChanges[neighborCoord] = 0f;
                        oxygenChanges[neighborCoord] += transferAmount;
                    }
                }
            }

            // 🌟 4. 모든 계산이 끝난 후, 버퍼에 담긴 변화량을 실제 데이터에 한 번에 적용합니다.
            foreach (var kvp in oxygenChanges)
            {
                var tile = _shipManager.GetTileAt(kvp.Key);
                tile.OxygenLevel += kvp.Value;

                // 산소량은 0~100 사이를 유지하도록 클램핑
                tile.OxygenLevel = Mathf.Clamp(tile.OxygenLevel, 0f, 100f);
            }
        }
    }
}