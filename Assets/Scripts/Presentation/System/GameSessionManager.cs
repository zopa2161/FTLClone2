using Core.Data.Map;
using Core.Data.SpaceShip;
using Core.Data.Storage;
using UnityEngine;

namespace Presentation.System
{
    public class GameSessionManager : Singleton<GameSessionManager>
    {
        public string SaveFileName = "GameSave";

        /// <summary>현재 진행 중인 런의 전체 세이브 데이터</summary>
        public GameSaveData CurrentGameData { get; private set; }

        /// <summary>ShipSetupManager용 편의 접근자</summary>
        public ShipSaveData ShipData => CurrentGameData?.Ship;

        /// <summary>
        /// MapSetupManager용 편의 접근자.
        /// null이면 새 맵 생성이 필요한 상태입니다.
        /// </summary>
        public MapData MapData => CurrentGameData?.Map;

        public void Awake()
        {
            StartGameWithData(SaveFileName);
        }

        public void StartGameWithData(string saveFileName)
        {
            CurrentGameData = SaveLoadManager.Load<GameSaveData>(saveFileName);

            if (CurrentGameData != null)
            {
                Debug.Log("[GameSessionManager] 기존 세이브 로드 완료");
                //SceneManager.LoadScene("MapScene");
            }
            else
            {
                Debug.LogWarning("[GameSessionManager] 세이브 없음 — 새 게임 데이터 생성");
                CurrentGameData = new GameSaveData();
            }
        }

        /// <summary>현재 상태를 파일에 저장합니다.</summary>
        public void SaveGame()
        {
            if (CurrentGameData == null) return;
            SaveLoadManager.Save(CurrentGameData, SaveFileName);
        }

        /// <summary>맵 데이터를 세션에 반영합니다. MapSetupManager가 생성 후 호출합니다.</summary>
        public void SetMapData(MapData mapData)
        {
            if (CurrentGameData == null) return;
            CurrentGameData.Map = mapData;
        }
    }
}
