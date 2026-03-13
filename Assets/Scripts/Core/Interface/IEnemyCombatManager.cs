using System;
using Core.Data.Event;

namespace Core.Interface
{
    /// <summary>
    /// 적군 전투 시뮬레이션 인터페이스.
    /// CombatSubEventSO 시작 시 적군 우주선 Logic을 조립하고 틱 루프에 등록합니다.
    /// </summary>
    public interface IEnemyCombatManager
    {
        /// <summary>현재 전투 중인 적군 우주선 Logic API. 적 체력 표시 등에 활용합니다.</summary>
        IShipAPI EnemyShipAPI { get; }

        /// <summary>전투가 종료되었을 때 발화됩니다. View 정리에 활용합니다.</summary>
        event Action OnCombatEnded;

        /// <summary>전투를 시작합니다. 적군 Logic을 조립하고 SimulationCore에 등록합니다.</summary>
        void StartCombat(CombatSubEventSO combatEvent);

        /// <summary>전투를 종료합니다. 적군 tickable을 SimulationCore에서 제거합니다.</summary>
        void EndCombat();
    }
}
