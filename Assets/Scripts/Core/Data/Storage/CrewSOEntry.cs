using System;
using Core.Data.Crews;

namespace Core.Data.Storage
{
    [Serializable]
    public struct CrewSOEntry
    {
        public string CrewDataID; // 예: "Human_Engineer"
        public CrewBaseSO BaseDataSO; // 실제 종족/직업 기획 데이터 원본
    }
}