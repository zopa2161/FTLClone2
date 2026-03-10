using System;

namespace Core.Data.SpaceShip
{
    [Serializable]
    public class CrewData
    {
        public int CrewID;
        public string BaseDataID;
        public string CrewType; // (예: "Human", "Alien")
        public string CrewName;
        public float MaxHealth = 100f;
        public float CurrentHealth = 100f;


        // 현재 딛고 있는 타일의 좌표
        public int CurrentX;
        public int CurrentY;

        public CrewData(int crewID, string baseDataID, string crewName)
        {
            CrewID = crewID;
            BaseDataID = baseDataID;
            //CrewType = crewType;
            CrewName = crewName;
        }
    }
}