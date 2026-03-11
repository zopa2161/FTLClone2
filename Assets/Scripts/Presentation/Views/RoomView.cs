using Core.Interface;
using UnityEditor;
using UnityEngine;

namespace Presentation.Views
{
    public class RoomView : MonoBehaviour
    {
        [SerializeField] public int RoomID;

        [Header("시각적 컴포넌트")] public GameObject HighlightOverlay;

        public SpriteRenderer _renderer;


        //===에디터용 변수===
        private float lastAverageOxygen;
        public IRoomLogic Logic { get; private set; }


        private void OnDestroy()
        {
            if (Logic != null) Logic.OnOxygenChanged -= HandleOxygenChanged;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // 2. 씬 뷰에 텍스트 띄우기 세팅
            var style = new GUIStyle();
            style.normal.textColor = Color.red;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 24;

            // 3. 현재 위치에 좌표 글자 그리기!
            Handles.Label(transform.position, $"{lastAverageOxygen}", style);
        }
#endif

        public void Bind(IRoomLogic logic)
        {
            Logic = logic;

            Logic.OnOxygenChanged += HandleOxygenChanged;

            // 전력 변화 무전 구독 (예: 전력이 끊기면 방을 어둡게 만듦)
            //logic.OnPowerChanged += UpdateRoomLighting;
        }

        public void SetHighlight(bool isHovered)
        {
            if (HighlightOverlay != null) HighlightOverlay.SetActive(isHovered);

            // (선택) 만약 오버레이 이미지가 없다면, 방 자체의 색상을 바꿀 수도 있습니다.
            // GetComponent<SpriteRenderer>().color = isHovered ? Color.green : Color.white;
        }

        private void HandleOxygenChanged(float averageOxygen)
        {
            lastAverageOxygen = averageOxygen;
            if (_renderer == null) return;

            // 🌟 산소가 50% 밑으로 떨어지면 점점 붉은색이 진해지도록 Alpha 값을 계산합니다.
            // (산소가 50일 때 alpha=0, 산소가 0일 때 alpha=0.5 정도로 설정)
            var dangerAlpha = 0f;

            if (averageOxygen < 50f)
            {
                // 50에서 0으로 갈수록 비율이 0 -> 1이 됨
                var severity = 1f - averageOxygen / 50f;
                dangerAlpha = severity * 0.5f; // 최대 투명도 50% (너무 새빨개서 안 보이면 안 되므로)
            }

            // 오버레이의 색상(Alpha) 업데이트
            var color = _renderer.color;
            color.a = dangerAlpha;
            _renderer.color = color;
            
        }
    }
}