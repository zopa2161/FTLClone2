using System;
using Core.Data.Event;
using Core.Interface;
using UnityEngine;

namespace Logic.Event
{
    /// <summary>
    /// 이벤트 진행을 총괄하는 Logic 계층 매니저.
    /// 현재 이벤트/서브이벤트 상태를 추적하고, 각 서브이벤트 완료 시 다음으로 전진합니다.
    /// 보상 적용은 IResourceManager에 위임합니다.
    /// MonoBehaviour 없음 — 순수 C#.
    /// </summary>
    public class EventLogicManager : IEventLogic
    {
        public bool IsEventActive => _currentEvent != null && _currentSubEvent != null;
        public EventSO CurrentEvent => _currentEvent;
        public SubEventBaseSO CurrentSubEvent => _currentSubEvent;

        public event Action<SubEventBaseSO> OnSubEventChanged;
        public event Action OnEventFinished;

        private EventSO _currentEvent;
        private SubEventBaseSO _currentSubEvent;
        private EventSaveData _saveData = new EventSaveData();

        private readonly IResourceManager _resourceManager;
        private readonly ICombatManager _combatManager;

        public EventLogicManager(IResourceManager resourceManager, ICombatManager combatManager)
        {
            _resourceManager = resourceManager;
            _combatManager = combatManager;
        }

        /// <summary>
        /// 세이브 데이터로부터 이벤트 진행 상황을 복원합니다.
        /// 이벤트가 진행 중이었다면 OnSubEventChanged를 즉시 발행해 Presentation이 UI를 복원할 수 있도록 합니다.
        /// </summary>
        public void Initialize(EventSaveData saveData,
            Func<string, EventSO> getEvent,
            Func<string, SubEventBaseSO> getSubEvent)
        {
            _saveData = saveData ?? new EventSaveData();

            if (!_saveData.IsEventActive) return;

            var restoredEvent = getEvent(_saveData.ActiveEventID);
            var restoredSubEvent = getSubEvent(_saveData.ActiveSubEventID);

            if (restoredEvent == null || restoredSubEvent == null)
            {
                Debug.LogWarning($"[EventLogicManager] 이벤트 복원 실패 — EventID: {_saveData.ActiveEventID}, SubEventID: {_saveData.ActiveSubEventID}");
                _saveData = new EventSaveData();
                return;
            }

            _currentEvent = restoredEvent;
            _currentSubEvent = restoredSubEvent;
            OnSubEventChanged?.Invoke(_currentSubEvent);
        }

        /// <summary>새 이벤트를 시작하고 첫 번째 서브이벤트로 진입합니다.</summary>
        public void StartEvent(EventSO eventSO)
        {
            if (eventSO == null)
            {
                Debug.LogWarning("[EventLogicManager] StartEvent: null EventSO");
                return;
            }

            _currentEvent = eventSO;
            AdvanceTo(eventSO.StartEvent);
        }

        /// <summary>Dialog 서브이벤트 완료 — 선택지 인덱스에 따라 다음 서브이벤트로 전진합니다.</summary>
        public void CompleteDialogSubEvent(int choiceIndex)
        {
            if (_currentSubEvent is not DialogSubEventSO dialog)
            {
                Debug.LogWarning("[EventLogicManager] CompleteDialogSubEvent: 현재 서브이벤트가 Dialog가 아닙니다.");
                return;
            }

            if (choiceIndex < 0 || choiceIndex >= dialog.Choices.Count)
            {
                Debug.LogWarning($"[EventLogicManager] CompleteDialogSubEvent: 잘못된 choiceIndex({choiceIndex})");
                return;
            }

            var next = dialog.Choices[choiceIndex].NextEvent;
            FinishCurrentAndAdvance(dialog, next);
        }

        /// <summary>Combat 서브이벤트 완료 — 전투 승리 후 호출됩니다.</summary>
        public void CompleteCombatSubEvent()
        {
            if (_currentSubEvent is not CombatSubEventSO combat)
            {
                Debug.LogWarning("[EventLogicManager] CompleteCombatSubEvent: 현재 서브이벤트가 Combat이 아닙니다.");
                return;
            }

            FinishCurrentAndAdvance(combat, combat.NextEvent);
        }

        /// <summary>Reward 서브이벤트 완료 — 보상을 즉시 적용하고 다음으로 전진합니다.</summary>
        public void CompleteRewardSubEvent()
        {
            if (_currentSubEvent is not RewardSubEventSO reward)
            {
                Debug.LogWarning("[EventLogicManager] CompleteRewardSubEvent: 현재 서브이벤트가 Reward가 아닙니다.");
                return;
            }

            ApplyRewards(reward);
            FinishCurrentAndAdvance(reward, reward.NextEvent);
        }

        /// <summary>현재 이벤트 진행 상황을 세이브 데이터로 반환합니다.</summary>
        public EventSaveData GetSaveData() => _saveData;

        // ─── 내부 메서드 ───────────────────────────────────────────────────

        /// <summary>다음 서브이벤트로 전환합니다. null이면 이벤트 전체 종료.</summary>
        private void AdvanceTo(SubEventBaseSO next)
        {
            //여기까지는 Debug.Log가 반응함
            _currentSubEvent = next;
            UpdateSaveData();
            _combatManager.SetCombatState(next is CombatSubEventSO);
            OnSubEventChanged?.Invoke(_currentSubEvent);
        }

        /// <summary>현재 서브이벤트를 완료하고, IsFinished 또는 next==null이면 종료, 아니면 전진합니다.</summary>
        private void FinishCurrentAndAdvance(SubEventBaseSO current, SubEventBaseSO next)
        {
            if (current.IsFinished || next == null)
            {
                _currentEvent = null;
                _currentSubEvent = null;
                UpdateSaveData();
                _combatManager.SetCombatState(false);
                OnEventFinished?.Invoke();
                return;
            }

            AdvanceTo(next);
        }

        /// <summary>RewardSubEventSO의 보상 목록을 IResourceManager에 적용합니다.</summary>
        private void ApplyRewards(RewardSubEventSO reward)
        {
            foreach (var entry in reward.Rewards)
            {
                switch (entry.Type)
                {
                    case RewardType.Scrap:            _resourceManager.AddScrap(entry.Amount);    break;
                    case RewardType.Fuel:             _resourceManager.AddFuel(entry.Amount);     break;
                    case RewardType.Missiles:         _resourceManager.AddMissiles(entry.Amount); break;
                    case RewardType.Drones:           _resourceManager.AddDrones(entry.Amount);   break;
                    case RewardType.MaxReactorPower:
                        Debug.Log($"[EventLogicManager] MaxReactorPower 보상 미구현 (amount={entry.Amount})");
                        break;
                    case RewardType.Weapon:
                        Debug.Log($"[EventLogicManager] Weapon 보상 미구현 (weaponID={entry.WeaponID})");
                        break;
                }
            }
        }

        private void UpdateSaveData()
        {
            _saveData.IsEventActive    = IsEventActive;
            _saveData.ActiveEventID    = _currentEvent?.EventID    ?? "";
            _saveData.ActiveSubEventID = _currentSubEvent?.SubEventID ?? "";
        }
    }
}
