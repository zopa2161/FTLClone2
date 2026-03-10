using Core.Data.SpaceShip;
using Core.Data.Storage;
using UnityEngine;

namespace Presentation.System
{
    public class GameSessionManager : Singleton<GameSessionManager>
    {
        public string SaveFileNameForTest = "DefaultShipData";
        public ShipSaveData CurrentGameData { get; private set; }

        public void Awake()
        {
            StartGameWithData(SaveFileNameForTest);
        }

        public void StartGameWithData(string saveFileName)
        {
            // JSON 로드
            CurrentGameData = SaveLoadManager.Load(saveFileName);

            if (CurrentGameData != null)
            {
                //SceneManager.LoadScene("GameScene");
            }
            else
            {
                Debug.LogError("데이터를 불러오지 못했습니다.");
            }
        }

        public ShipSaveData HandOverData()
        {
            return CurrentGameData;
        }
    }
}