using UnityEngine;

namespace Core.Data.Event
{
    [CreateAssetMenu(fileName = "NewCombatSubEvent", menuName = "FTL/Event/CombatSubEvent")]
    public class CombatSubEventSO : SubEventBaseSO
    {
        [Tooltip("전투에 등장하는 적군 기체 SO")]
        public EnemyShipSO EnemyShip;

        [Tooltip("전투 승리 후 진행할 세부 이벤트 (null이면 종료)")]
        public SubEventBaseSO NextEvent;
    }
}
