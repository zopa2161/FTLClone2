using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Data.Event
{
    [Serializable]
    public class DialogChoice
    {
        public string ChoiceText;
        [Tooltip("null이면 이 선택지에서 이벤트 종료")]
        public SubEventBaseSO NextEvent;
    }

    [CreateAssetMenu(fileName = "NewDialogSubEvent", menuName = "FTL/Event/DialogSubEvent")]
    public class DialogSubEventSO : SubEventBaseSO
    {
        [TextArea(2, 6)] public string DialogText;
        public List<DialogChoice> Choices = new();
    }
}
