using Core.Data.SpaceShip;
using UnityEngine;

namespace Core.Data.Event
{
    [CreateAssetMenu(fileName = "NewEnemyShip", menuName = "FTL/Event/EnemyShip")]
    public class EnemyShipSO : ScriptableObject
    {
        [Tooltip("에디터 식별용 이름 (파일명 자동 리네임 기준)")]
        public string Title;

        public ShipSaveData ShipData = new();
    }
}
