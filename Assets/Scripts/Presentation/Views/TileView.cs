using Core.Data.SpaceShip;
using Core.Interface;
using UnityEditor;
using UnityEngine;

namespace Presentation.Views
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private TileCoord _tileCoord;
        [SerializeField] private GameObject _fireObject;

        private ITileLogic _tileLogic;
        public TileCoord TileCoord => _tileCoord;

        private float _lastFireLevel;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // 1. 타일의 영역을 보여주는 얇은 테두리 (초록색 반투명)
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
            // 크기를 0.95f로 주면 타일 사이에 살짝 틈이 보여서 그리드(Grid) 구분이 잘 됩니다.
            Gizmos.DrawWireCube(transform.position, new Vector3(0.95f, 0.95f, 0f));

            // 2. 씬 뷰에 텍스트 띄우기 세팅
            var style = new GUIStyle();
            style.normal.textColor = Color.red;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 24;

            // 3. 현재 위치에 좌표 글자 그리기!
            Handles.Label(transform.position, $"{_tileCoord.X}, {_tileCoord.Y}", style);
        }
#endif

        public void Bind(ITileLogic logic)
        {
            _tileLogic = logic;
            logic.OnFireChanged += HandleFireChanged;
            HandleFireChanged(logic.FireLevel);
        }

        private void HandleFireChanged(float fireLevel)
        {
            _lastFireLevel = fireLevel;
            if (_fireObject != null)
            {
                _fireObject.SetActive(fireLevel > 0f);
            }
              
        }
    }
}