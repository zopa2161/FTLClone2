using System;
using System.Collections.Generic;
using Core.Data.SpaceShip;
using Core.enums;
using Core.Interface;
using Logic.SpaceShip.CrewState;
using Logic.System;
using UnityEngine;

namespace Logic.SpaceShip
{
    public class CrewLogic : ICrewLogic, ITickable
    {
        private Queue<TileCoord> _currentPath = new();
        private ICrewState _currentState;
        private bool _isDead;

        public IGridMap GridMap { get; private set; }

        public TileCoord CurrentCoord => new(Data.CurrentX, Data.CurrentY);
        public event Action<int, int, MoveDirection> OnPositionChanged;
        public CrewData Data { get; private set; }

        public int CrewID => Data.CrewID;

        public float CurrentHealth => Data.CurrentHealth;
        public float MaxHealth => Data.MaxHealth;

        public event Action<float, float> OnHealthChanged;
        public event Action<ICrewLogic> OnDied;

        public void CommandMoveTo(TileCoord targetCoord)
        {
            _currentPath = AStarPathfinder.FindPath(GridMap, CurrentCoord, targetCoord);

            // 💡 이동 경로가 생겼으니, 강제로 '이동 상태(MovingState)'로 두뇌를 교체합니다!
            if (_currentPath.Count > 0) ChangeState(new CrewMovingState());
        }

        public void TakeDamage(float amount)
        {
            // 체력을 깎고 (회복일 경우 음수가 들어옴), 0~Max 사이로 고정
            Data.CurrentHealth -= amount;
            Data.CurrentHealth = Mathf.Clamp(Data.CurrentHealth, 0f, Data.MaxHealth);

            OnHealthChanged?.Invoke(Data.CurrentHealth, Data.MaxHealth);

            // (추후 추가) 체력이 0이 되었을 때의 사망 처리 로직
            if (Data.CurrentHealth <= 0f) Die();
        }

        public void OnTickUpdate()
        {
            if (_isDead) return;
            _currentState?.Execute(this);
            Breathe();
        }


        public void Initialize(CrewData crewData, IGridMap gridMap)
        {
            Data = crewData;
            GridMap = gridMap;

            var spawnCoord = new TileCoord(Data.CurrentX, Data.CurrentY);

            // 💡 1. 내가 스폰된 위치의 방을 가져옵니다.
            var currentRoom = GridMap.GetRoomAt(spawnCoord);

            // 💡 2. 똑똑한 초기 상태 결정 로직
            // 이 방이 존재하고, 스폰된 자리가 하필 '작업 타일(0번)'이라면?
            if (currentRoom != null && currentRoom.IsWorkingTile(spawnCoord))
            {
                // 스폰되자마자 즉시 작업 모드로 돌입! (콘솔 방향으로 몸 틀기 포함)
                // 전력 상태가 1이상임을 확인해야함.
                if (currentRoom.Data.CurrentAllocatedPower > 0) ChangeState(new CrewWorkingState(currentRoom));


            }
            else
            {
                // 작업 자리가 아니면 (복도, 1번 타일 등) 평소처럼 대기 상태로 스폰
                ChangeState(new CrewIdleState());
            }
        }

        public void ChangeState(ICrewState newState)
        {
            // 1. 기존 상태 퇴장
            _currentState?.Exit(this);

            // 2. 새로운 상태 장착
            _currentState = newState;

            // 3. 새로운 상태 입장
            _currentState?.Enter(this);
        }

        // --- 내부 헬퍼 함수들 (State들이 쓰기 편하게 열어둠) ---
        public bool HasPath()
        {
            return _currentPath.Count > 0;
        }

        public TileCoord PeekNextPath()
        {
            return _currentPath.Peek();
        }

        public void MoveToNextPath()
        {
            // 1. 현재 좌표와 다음 좌표 준비
            var currentCoord = CurrentCoord;
            var nextCoord = _currentPath.Dequeue();

            // 🌟 2. [핵심 작업] 두 좌표를 비교해서 논리적 방향 계산!
            var moveDir = CalculateDirection(currentCoord, nextCoord);

            // 3. 데이터 업데이트
            Data.CurrentX = nextCoord.X;
            Data.CurrentY = nextCoord.Y;

            // 4. 📢 뷰에게 무전 발송 (X, Y, 계산된 방향)
            OnPositionChanged?.Invoke(nextCoord.X, nextCoord.Y, moveDir);
        }

        public void LookAt(MoveDirection direction)
        {
            OnPositionChanged?.Invoke(Data.CurrentX, Data.CurrentY, direction);
        }

        private void Breathe()
        {
            // 1. 내가 현재 밟고 있는 타일을 지도에게 물어봅니다. (아주 가벼운 연산!)
            var myTile = GridMap.GetTileAt(CurrentCoord);

            if (myTile != null)
                // 2. 내 발밑 타일의 산소가 20% 미만이라면? (질식 시작)
                if (myTile.OxygenLevel < 50f)
                    // 1틱당 1의 데미지를 입습니다. (초당 10번 틱이 돈다면 초당 10 데미지)
                    TakeDamage(0.5f);
            // (선택) 여기서 뷰에게 "나 숨막혀!" 하는 기침 애니메이션 무전을 칠 수도 있습니다.
            // (추후 화재 데미지도 여기서 검사하면 완벽합니다)
            // if (myTile.Data.IsOnFire) TakeDamage(2f);
        }

        private void Die()
        {
            _isDead = true;

            // 1. 유언 발송! "나(this) 죽었소!"
            OnDied?.Invoke(this);

            // 2. 혹시 진행 중이던 상태(FSM)가 있다면 정리
            _currentState?.Exit(this);
            _currentState = null;
        }

        private MoveDirection CalculateDirection(TileCoord from, TileCoord to)
        {
            if (to.X > from.X) return MoveDirection.Right;
            if (to.X < from.X) return MoveDirection.Left;
            if (to.Y > from.Y) return MoveDirection.Up;
            if (to.Y < from.Y) return MoveDirection.Down;
            return MoveDirection.None;
        }
    }
}