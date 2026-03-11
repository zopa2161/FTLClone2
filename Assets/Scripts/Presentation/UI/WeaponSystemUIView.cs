using UnityEngine;
using UnityEngine.UIElements;
using Logic.SpaceShip;
using Logic.System;
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
            var overlay = root.Q<VisualElement>("overlay");
            overlay.pickingMode = PickingMode.Ignore;
            
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
            var slot = new VisualElement();
            slot.AddToClassList("weapon-slot");
            
            var label = new Label();
            label.AddToClassList("weapon-label");
            label.text = weaponLogic.BaseData.WeaponName; 

            // 3. 조립: 텍스트를 슬롯에 넣고, 슬롯을 메인 패널에 넣기
            var chargeBg = new VisualElement();
            chargeBg.AddToClassList("weapon-charge-bg");

            var chargeFill = new VisualElement();
            chargeFill.AddToClassList("weapon-charge-fill");
            chargeBg.Add(chargeFill); // 배경 안에 알맹이를 넣음

            // 박스에 텍스트와 게이지를 순서대로 조립
            slot.Add(label);
            slot.Add(chargeBg);
            _weaponPanel.Add(slot);

            // ==========================================
            // 💡 4. 로직과 UI 연동 (데이터 바인딩)
            // ==========================================
            
            // 장전 비율이 변할 때마다 호출되는 함수
            weaponLogic.OnChargeUpdated += (progress) =>
            {
                // UI Toolkit에서 퍼센트(%) 너비를 조절하는 방법
                chargeFill.style.width = new StyleLength(Length.Percent(progress * 100f));

                // 100% 장전 완료 시 색상 변경용 클래스 추가
                if (progress >= 1f) chargeFill.AddToClassList("ready");
                else chargeFill.RemoveFromClassList("ready");
            };

            // 🌟 전력 상태 갱신 (여기에 테두리와 글자 색상 변경 로직 추가)
            weaponLogic.OnPowerStateChanged += (isPowered) =>
            {
                if (isPowered)
                {
                    chargeFill.RemoveFromClassList("unpowered");
                    slot.RemoveFromClassList("off");   // 테두리 하얗게 복구
                    label.RemoveFromClassList("off");  // 글자 하얗게 복구
                }
                else
                {
                    chargeFill.AddToClassList("unpowered");
                    slot.AddToClassList("off");        // 테두리 회색으로
                    label.AddToClassList("off");       // 글자 회색으로
                }
            };

            // 처음 UI가 그려질 때 현재 상태로 한 번 초기화
            chargeFill.style.width = new StyleLength(Length.Percent(weaponLogic.ChargeProgress * 100f));
            if (!weaponLogic.IsPowered)
            {
                chargeFill.AddToClassList("unpowered");
                slot.AddToClassList("off");
                label.AddToClassList("off");
            }

            // ==========================================
            // 💡 5. 클릭 이벤트 (무기 ON/OFF)
            // ==========================================
            slot.RegisterCallback<PointerDownEvent>(evt =>
            {
                // 🖱️ 좌클릭 (0): 무기 켜기
                if (evt.button == 0) 
                {
                    // 이미 켜져있다면 아무것도 하지 않음
                    if (weaponLogic.IsPowered) return;

                    bool success = _weaponManager.TryToggleWeaponPower(index, true);
        
                    if (!success)
                    {
                        UnityEngine.Debug.Log("전력이 부족하여 무기를 켤 수 없습니다!");
                        // (추후 에러 사운드나 UI 흔들림 연출 추가)
                    }
                }
                // 🖱️ 우클릭 (1): 무기 끄기
                else if (evt.button == 1) 
                {
                    // 이미 꺼져있다면 아무것도 하지 않음
                    if (!weaponLogic.IsPowered) return;

                    // 끄는 것은 전력 한도와 상관없이 언제나 성공함
                    _weaponManager.TryToggleWeaponPower(index, false);
                }
            });
        }
    }
}