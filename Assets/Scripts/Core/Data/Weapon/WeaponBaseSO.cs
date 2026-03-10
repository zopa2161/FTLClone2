using Core.enums;
using UnityEngine;

namespace Core.Data.Weapon
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "SpaceShip/Weapon Data")]
    public class WeaponBaseSO : ScriptableObject
    {
        [Header("기본 정보")] public string WeaponID; // 고유 ID (세이브/로드용)

        public string WeaponName; // 표기 이름 (예: "Burst Laser Mk II")
        public string Description; // 무기 설명
        public WeaponType Type;

        [Header("전투 수치")] public int Damage = 1; // 한 발당 피해량

        public int ProjectileCount = 1; // 한 번 발사할 때 쏘는 발사체 수

        // 💡 방금 만든 전력 시스템과 연동될 핵심 수치!
        public int RequiredPower = 1; // 이 무기를 켜기 위해 필요한 전력 칸 수

        [Header("시간 수치")] public float BaseCooldown = 10f; // 장전 완료까지 걸리는 시간 (초)
    }
}