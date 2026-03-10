using UnityEngine;
using UnityEngine.UIElements;
using Logic.SpaceShip;
using Presentation.System; // WeaponManager 위치

namespace Presentation.Views.UI
{
    public class WeaponSystemUIView : MonoBehaviour
    {
        public UIDocument Document;
        private WeaponManager _weaponManager;
        
        // 💡 무기들이 들어갈 부모 패널
        private VisualElement _weaponPanel; 

        // 총괄 조립자(ShipSetupManager)가 초기화할 때 호출
        public void Initialize(WeaponManager weaponManager)
        {
            _weaponManager = weaponManager;
            
            var root = Document.rootVisualElement;
            _weaponPanel = root.Q<VisualElement>("WeaponPanel");

            // 기존에 그려진 UI가 있다면 초기화
            _weaponPanel.Clear();

            // 🌟 매니저에 등록된 무기 개수(n)만큼 UI 슬롯을 동적으로 생성
            for (int i = 0; i < _weaponManager.Weapons.Count; i++)
            {
                var weaponLogic = _weaponManager.Weapons[i];
                CreateWeaponSlot(i, weaponLogic);
            }
        }

        private void CreateWeaponSlot(int index, Core.Interface.IWeaponLogic weaponLogic)
        {
            // 1. 무기 칸(박스) 생성
            var slot = new VisualElement();
            slot.AddToClassList("weapon-slot");

            // 2. 무기 이름 텍스트 생성
            var label = new Label();
            label.AddToClassList("weapon-label");
            
            // 데이터에서 무기 이름 가져와서 적기 (예: "Laser_Mk1")
            // (만약 이름이 너무 길면 앞부분만 자르는 로직을 넣어도 좋습니다)
            label.text = weaponLogic.Data.BaseData.WeaponName; 

            // 3. 조립: 텍스트를 슬롯에 넣고, 슬롯을 메인 패널에 넣기
            slot.Add(label);
            _weaponPanel.Add(slot);

            // ==========================================
            // 💡 클릭 이벤트 연동 (미리 세팅)
            // ==========================================
            slot.RegisterCallback<PointerDownEvent>(evt =>
            {
                // 클릭하면 무기의 ON/OFF 전력 스위치를 조작하도록 매니저에게 요청
                bool turnOn = !weaponLogic.IsPowered; 
                _weaponManager.TryToggleWeaponPower(index, turnOn);
            });
            
            // (참고) 나중에 여기서 무기가 켜졌는지(IsPowered)에 따라 
            // 테두리 색상을 초록색으로 바꾸는 등의 시각적 업데이트를 추가할 수 있습니다.
        }
    }
}