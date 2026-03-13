using System;
using Core.Data.Event;

namespace Core.Interface
{
    /// <summary>
    /// 이벤트 진행 로직 인터페이스.
    /// 현재 이벤트 상태 추적, 서브이벤트 완료 처리, 세이브 데이터 제공을 담당합니다.
    /// </summary>
    public interface IEventLogic
    {
        bool IsEventActive { get; }
        EventSO CurrentEvent { get; }
        SubEventBaseSO CurrentSubEvent { get; }

        /// <summary>새 서브이벤트로 전환될 때 발행됩니다. (null이면 이벤트 종료)</summary>
        event Action<SubEventBaseSO> OnSubEventChanged;

        /// <summary>이벤트 전체가 종료되었을 때 발행됩니다. → 점프 가능 상태로 복귀</summary>
        event Action OnEventFinished;

        /// <summary>이벤트를 시작하고 첫 번째 서브이벤트로 진입합니다.</summary>
        void StartEvent(EventSO eventSO);

        /// <summary>Dialog 서브이벤트: 선택지 인덱스를 전달해 완료 처리합니다.</summary>
        void CompleteDialogSubEvent(int choiceIndex);

        /// <summary>Combat 서브이벤트: 전투 승리 후 완료 처리합니다.</summary>
        void CompleteCombatSubEvent();

        /// <summary>Reward 서브이벤트: 보상 확인 후 완료 처리합니다. 보상이 즉시 적용됩니다.</summary>
        void CompleteRewardSubEvent();

        /// <summary>현재 이벤트 진행 상황을 세이브 데이터로 반환합니다.</summary>
        EventSaveData GetSaveData();
    }
}
