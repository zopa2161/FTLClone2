using System.Collections.Generic;
using Core.Interface;
using Logic.System;
using UnityEngine;

namespace Presentation.Views
{
    public class SpaceShipView : MonoBehaviour
    {
        [Header("그리드 시각화 설정")] public Vector3 GridOriginOffset = Vector3.zero;

        public List<TileView> TileViews = new();
        public List<RoomView> RoomViews = new();
        public List<DoorView> DoorViews = new();
        public ShieldView ShieldView;

        public SimulationCore SimulationCore { get; set; }
        // (선택) 타일 뷰나 승무원 뷰 리스트도 필요하다면 추가 가능

        public void Bind(IShipAPI shipLogic, IReadOnlyList<ITileLogic> tileLogics, IReadOnlyList<IRoomLogic> roomLogics,
            IReadOnlyList<IDoorLogic> doorLogics)
        {
            BindTiles(tileLogics);
            BindRooms(roomLogics);
            BindDoors(doorLogics);
        }

        public void BindShield(IShieldLogic shieldLogic)
        {
            ShieldView?.Bind(shieldLogic);
        }

        private void BindTiles(IReadOnlyList<ITileLogic> tileLogics)
        {
            foreach (var tileLogic in tileLogics)
                TileViews.Find(x => x.TileCoord == tileLogic.TileCoord).Bind(tileLogic);
        }

        private void BindRooms(IReadOnlyList<IRoomLogic> roomLogics)
        {
            foreach (var roomLogic in roomLogics) RoomViews.Find(x => x.RoomID == roomLogic.RoomID).Bind(roomLogic);
        }

        private void BindDoors(IReadOnlyList<IDoorLogic> doorLogics)
        {
            foreach (var doorLogic in doorLogics) DoorViews.Find(x => x.DoorID == doorLogic.DoorID).Bind(doorLogic);
        }


        public Vector3 GetWorldPosition(int logicalX, int logicalY)
        {
            return GridOriginOffset + new Vector3(logicalX, logicalY, 0f);
        }

        // ==========================================
        // 🎬 거시적 연출 함수들
        // ==========================================
        private void TriggerShipShakeEffect(float damageAmount)
        {
            // (카메라 쉐이크 또는 우주선 스프라이트 진동 로직)
        }

        private void TurnOffAllRoomLights()
        {
            // 부하 직원들(RoomView)의 불을 한 번에 다 꺼버릴 수도 있습니다.
            // (물론 각 RoomLogic이 알아서 끄게 놔두는 게 결합도가 낮아서 더 좋긴 합니다)
        }
    }
}