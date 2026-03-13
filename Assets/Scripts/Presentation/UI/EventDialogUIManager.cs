using Core.Data.Event;
using Core.Interface;
using UnityEngine;
using UnityEngine.UIElements;

namespace Presentation.UI
{
    /// <summary>
    /// Dialog 및 Reward 서브이벤트를 화면 중앙 오버레이로 표시합니다.
    /// IEventLogic.OnSubEventChanged를 구독하여 서브이벤트 유형에 따라 UI를 전환합니다.
    /// GameHUD UIDocument를 공유합니다.
    /// </summary>
    public class EventDialogUIManager : MonoBehaviour
    {
        private VisualElement _eventOverlay;
        private Label         _eventTitle;
        private Label         _eventDialogText;
        private VisualElement _rewardContainer;
        private VisualElement _choicesContainer;
        private Button        _acceptButton;

        private IEventLogic _eventLogic;

        public void Initialize(IEventLogic eventLogic)
        {
            _eventLogic = eventLogic;

            var uiDoc = GetComponent<UIDocument>();
            var root  = uiDoc.rootVisualElement;

            _eventOverlay    = root.Q<VisualElement>("EventOverlay");
            _eventTitle      = root.Q<Label>("EventTitle");
            _eventDialogText = root.Q<Label>("EventDialogText");
            _rewardContainer  = root.Q<VisualElement>("RewardContainer");
            _choicesContainer = root.Q<VisualElement>("ChoicesContainer");
            _acceptButton    = root.Q<Button>("AcceptButton");

            if (_eventOverlay == null)
            {
                
                UnityEngine.Debug.LogError("[EventDialogUIManager] EventOverlay를 찾을 수 없습니다. GameHUD.uxml을 확인하세요.");
                return;
            }

            _eventLogic.OnSubEventChanged += OnSubEventChanged;
            _eventLogic.OnEventFinished   += HideOverlay;
        }

        private void OnDestroy()
        {
            if (_eventLogic == null) return;
            _eventLogic.OnSubEventChanged -= OnSubEventChanged;
            _eventLogic.OnEventFinished   -= HideOverlay;
        }

        // ─── 서브이벤트 분기 ───────────────────────────────────────────

        private void OnSubEventChanged(SubEventBaseSO subEvent)
        {
            Debug.Log("이벤트 변화 탐지");
            switch (subEvent)
            {
                case DialogSubEventSO dialog:
                    ShowDialog(dialog);
                    break;
                case RewardSubEventSO reward:
                    ShowReward(reward);
                    break;
                default:
                    // CombatSubEvent 등 — 오버레이 숨김
                    HideOverlay();
                    break;
            }
        }

        // ─── Dialog UI ────────────────────────────────────────────────

        private void ShowDialog(DialogSubEventSO dialog)
        {
            Debug.Log("ShowDialog");
            _eventTitle.text      = dialog.Title;
            _eventDialogText.text = dialog.DialogText;

            _rewardContainer.style.display  = DisplayStyle.None;
            _acceptButton.style.display     = DisplayStyle.None;
            _choicesContainer.style.display = DisplayStyle.Flex;

            _choicesContainer.Clear();
            for (int i = 0; i < dialog.Choices.Count; i++)
            {
                int capturedIndex = i;
                var btn = new Button(() => _eventLogic.CompleteDialogSubEvent(capturedIndex))
                {
                    text = dialog.Choices[i].ChoiceText
                };
                btn.AddToClassList("event-choice-button");
                _choicesContainer.Add(btn);
            }

            ShowOverlay();
        }

        // ─── Reward UI ────────────────────────────────────────────────

        private void ShowReward(RewardSubEventSO reward)
        {
            _eventTitle.text      = reward.Title;
            _eventDialogText.text = "";

            _choicesContainer.style.display  = DisplayStyle.None;
            _rewardContainer.style.display   = DisplayStyle.Flex;
            _acceptButton.style.display      = DisplayStyle.Flex;

            _rewardContainer.Clear();
            foreach (var entry in reward.Rewards)
            {
                var label = new Label(FormatRewardEntry(entry));
                label.AddToClassList("event-reward-entry");
                _rewardContainer.Add(label);
            }

            _acceptButton.clicked -= OnAcceptClicked;
            _acceptButton.clicked += OnAcceptClicked;

            ShowOverlay();
        }

        private void OnAcceptClicked() => _eventLogic.CompleteRewardSubEvent();

        // ─── 공통 ─────────────────────────────────────────────────────

        private void ShowOverlay()
            => _eventOverlay.style.display = DisplayStyle.Flex;

        private void HideOverlay()
        {
            _eventOverlay.style.display = DisplayStyle.None;
            _choicesContainer?.Clear();
            _rewardContainer?.Clear();
        }

        private static string FormatRewardEntry(RewardEntry entry)
        {
            return entry.Type switch
            {
                RewardType.Weapon => $"무기 획득: {entry.WeaponID}",
                _                 => $"{entry.Type}: {(entry.Amount >= 0 ? "+" : "")}{entry.Amount}"
            };
        }
    }
}
