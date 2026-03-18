using Core.Data.Crews;
using Core.enums;
using Core.Interface;
using UnityEngine;

namespace Presentation.Views
{
    public class CrewView : MonoBehaviour
    {
        [Header("이동 연출 설정")] public float MoveSpeed = 5f;

        [Header("상태 마크")] [SerializeField] private GameObject _workingMark;
        [SerializeField] private GameObject _fireFightingMark;

        [Header("체력바 UI")] public GameObject HealthBarContainer;

        // 💡 실제 체력 비율에 따라 길이가 줄어들 알맹이 이미지의 Transform
        public Transform HealthBarFill;

        private SpriteRenderer _renderer;

        // (선택) private Animator _animator;
        private SpaceShipView _shipView;
        private Vector3 _targetWorldPosition;
        public ICrewLogic Logic { get; private set; }

        private void Awake()
        {
            _renderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            // 목표 위치와 현재 위치가 다르다면, 목표를 향해 스르륵 걸어갑니다.
            if (transform.position != _targetWorldPosition)
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    _targetWorldPosition,
                    MoveSpeed * Time.deltaTime
                );
        }

        private void OnDestroy()
        {
            // 메모리 누수 방지를 위해 뷰가 파괴될 때 구독 해제
            if (Logic != null)
            {
                Logic.OnPositionChanged -= HandlePositionChanged;
                Logic.OnHealthChanged -= HandleHealthChanged;
                Logic.OnDied -= HandleDied;
                Logic.OnStateChanged -= HandleStateChanged;
            }
        }


        public void Bind(CrewBaseSO baseData, ICrewLogic logic, SpaceShipView ShipView)
        {
            _shipView = ShipView;
            // 1. 불변 데이터(SO)로 외형 초기화
            _renderer.sprite = baseData.DefaultSprite;
            gameObject.name = $"CrewView_{logic.Data.CrewName}";

            Logic = logic;

            // 2. 현재 상태 데이터(Logic)로 초기 위치 세팅

            _targetWorldPosition = _shipView.GetWorldPosition(logic.Data.CurrentX, logic.Data.CurrentY);
            transform.position = _targetWorldPosition;
            Logic.OnPositionChanged += HandlePositionChanged;
            logic.OnHealthChanged += HandleHealthChanged;
            logic.OnDied += HandleDied;
            logic.OnStateChanged += HandleStateChanged;
        }

        private void HandlePositionChanged(int logicalX, int logicalY, MoveDirection direction)
        {
            // 🌟 1. 방향에 맞춰 스프라이트 회전시키기!
            RotateSpriteByDirection(direction);
            // 목표 월드 좌표만 갱신해 둡니다. (실제 이동은 Update에서 처리)
            _targetWorldPosition = _shipView.GetWorldPosition(logicalX, logicalY);

            // (선택) 여기서 캐릭터가 걸어가는 애니메이션(Animator)을 재생하도록 명령할 수도 있습니다.
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            // 🌟 기획 조건: 체력이 100(Max)이면 체력바 숨기기
            if (currentHealth >= maxHealth)
            {
                if (HealthBarContainer.activeSelf)
                    HealthBarContainer.SetActive(false);
                return;
            }

            // 100 미만이면 체력바를 켭니다.
            if (!HealthBarContainer.activeSelf)
                HealthBarContainer.SetActive(true);

            // 비율 계산 (0.0 ~ 1.0)
            var healthRatio = currentHealth / maxHealth;

            // 💡 x축 스케일을 조절하여 체력바를 깎습니다.
            // (Pivot이 왼쪽 끝으로 설정되어 있어야 자연스럽게 줄어듭니다!)
            HealthBarFill.localScale = new Vector3(healthRatio, 1f, 1f);

            // (선택 사항) 체력이 30% 이하일 때 색상을 붉은색으로 바꾸기
            /*
            var sr = HealthBarFill.GetComponent<SpriteRenderer>();
            sr.color = healthRatio <= 0.3f ? Color.red : Color.green;
            */
        }

        private void HandleDied(ICrewLogic deadCrew)
        {
            // 1. 사망 애니메이션 재생 또는 시체 스프라이트로 변경
            // animator.SetTrigger("Die");

            // (옵션) 2D 게임에서는 보통 그 자리에 피를 흘리는 '시체 오브젝트'를 하나 새로 스폰하고,
            // 살아 움직이던 이 껍데기(CrewView)는 파괴하는 방식을 많이 씁니다.
            // Instantiate(CorpsePrefab, transform.position, Quaternion.identity);

            // 2. 나 자신(게임 오브젝트)을 씬에서 파괴!
            Destroy(gameObject);
        }

        private void HandleStateChanged(CrewStateType stateType)
        {
            Debug.Log("상태변화 탐지");
            Debug.Log($"바뀐 상태 : {stateType}");
            _workingMark?.SetActive(stateType == CrewStateType.Working);
            _fireFightingMark?.SetActive(stateType == CrewStateType.FireFighting);
        }

        public void Highlight(bool b)
        {
            if (b) _renderer.color = Color.green;
            else _renderer.color = Color.white;
        }

        private void RotateSpriteByDirection(MoveDirection direction)
        {
            var targetAngle = 0f;

            // 💡 원본 스프라이트 이미지가 '오른쪽'을 보고 있다고 가정합니다.
            switch (direction)
            {
                case MoveDirection.Right:
                    targetAngle = -90f;
                    break;
                case MoveDirection.Up:
                    targetAngle = 0f;
                    break;
                case MoveDirection.Left:
                    targetAngle = 90f;
                    break;
                case MoveDirection.Down:
                    targetAngle = 180f; // 270f 와 동일
                    break;
                case MoveDirection.None:
                    return; // 제자리일 때는 회전하지 않고 그대로 유지
            }

            // SpriteRenderer가 붙어있는 게임 오브젝트의 Z축을 회전시킵니다.
            _renderer.transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
    }
}