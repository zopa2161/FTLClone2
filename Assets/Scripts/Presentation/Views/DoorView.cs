using Core.Interface;
using UnityEngine;

namespace Presentation.Views
{
    public class DoorView : MonoBehaviour
    {
        [SerializeField] public int DoorID;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] public Sprite OpenedDoor;
        [SerializeField] public Sprite ClosedDoor;
        private Animator _animator;

        public IDoorLogic Logic { get; private set; }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnDestroy()
        {
            if (Logic != null) Logic.OnDoorStateChanged -= UpdateDoorVisuals;
        }

        public void Bind(IDoorLogic logic)
        {
            Logic = logic;
            UpdateDoorVisuals(Logic.IsOpen);
            // 문의 열림/닫힘 구독
            logic.OnDoorStateChanged += UpdateDoorVisuals;
        }

        private void UpdateDoorVisuals(bool isOpen)
        {
            _spriteRenderer.sprite = isOpen ? OpenedDoor : ClosedDoor;
            //_animator.SetBool("IsOpen", isOpen);
        }
    }
}