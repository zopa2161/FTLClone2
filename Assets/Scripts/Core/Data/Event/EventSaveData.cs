using System;

namespace Core.Data.Event
{
    /// <summary>
    /// 이벤트 진행 상황 저장 데이터.
    /// 이벤트 자체(SO)가 아닌 "현재 어느 단계에 있는가"만 기록합니다.
    /// GameSaveData에 포함되어 세이브/로드 시 이벤트 진행 상황을 유지합니다.
    /// </summary>
    [Serializable]
    public class EventSaveData
    {
        /// <summary>현재 이벤트가 진행 중인지 여부</summary>
        public bool IsEventActive = false;

        /// <summary>진행 중인 이벤트의 ID (EventSO.EventID)</summary>
        public string ActiveEventID = "";

        /// <summary>진행 중인 서브이벤트의 ID (SubEventBaseSO.SubEventID)</summary>
        public string ActiveSubEventID = "";
    }
}
