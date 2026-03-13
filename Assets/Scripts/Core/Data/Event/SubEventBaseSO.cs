using UnityEngine;

namespace Core.Data.Event
{
    public abstract class SubEventBaseSO : ScriptableObject
    {
        [Tooltip("세이브/로드 및 DB 조회용 고유 ID")]
        public string SubEventID;

        [Tooltip("Asset 파일 이름에 사용되는 제목 (에디터 자동 리네임)")]
        public string Title;

        [Tooltip("true이면 이벤트 전체 종료 → 게임 점프 가능 상태로 복귀")]
        public bool IsFinished;
    }
}
