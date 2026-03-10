using System;

namespace Core.Data.Weapon
{
    [Serializable]
    public class WeaponData
    {
        // 💡 세이브 파일에 저장될 때, 무기의 모든 정보를 저장할 필요 없이 ID만 저장합니다.
        // 로드할 때 이 ID를 바탕으로 WeaponBaseSO를 찾아 연결(Bind)합니다.
        public string WeaponID;

        // ==========================================
        // 런타임 동적 상태 (게임 중 계속 변하는 값들)
        // ==========================================

        // 이 무기에 유저가 전력을 넣어주었는가?
        public bool IsPowered;

        // 현재 장전 진행도 (0f ~ BaseData.BaseCooldown)
        public float CurrentChargeTimer;

        // 유저가 이 무기를 '자동 발사(Auto-Fire)' 모드로 두었는가?
        public bool IsAutoFire;

        // 현재 발사할 타겟(적 우주선의 특정 방 ID 등)이 지정되어 있는가?
        public int TargetRoomID = -1; // -1이면 타겟 없음

        // 런타임에 SO에서 읽어온 참조 (직렬화 안 함)
        [NonSerialized] public WeaponBaseSO BaseData;
    }
}