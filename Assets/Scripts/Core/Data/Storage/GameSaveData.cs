using System;
using Core.Data.Map;
using Core.Data.SpaceShip;

namespace Core.Data.Storage
{
    /// <summary>
    /// 게임 전체를 포괄하는 최상위 세이브 데이터.
    /// ShipSaveData(선내 시뮬레이션)와 MapData(섹터 맵 항법)를 함께 보유합니다.
    /// GameSessionManager가 이 객체를 소유하며 씬 간 유지합니다.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>우주선 내부 데이터 (타일·방·문·승무원·무기·실드)</summary>
        public ShipSaveData Ship = new ShipSaveData();

        /// <summary>
        /// 섹터 맵 데이터. null이면 새 맵 생성이 필요한 상태입니다.
        /// MapSetupManager.BeginSetup()에서 null 여부를 확인합니다.
        /// </summary>
        public MapData Map;
    }
}
