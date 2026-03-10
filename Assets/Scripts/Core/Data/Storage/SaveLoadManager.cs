using System;
using System.IO;
using Core.Data.SpaceShip;
using UnityEngine;

namespace Core.Data.Storage
{
    public class SaveLoadManager
    {
        private static string GetSaveDirectory()
        {
            var path = Path.Combine(Application.persistentDataPath, "Saves");

            // 폴더가 없으면 미리 만들어둡니다.
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        private static string GetFullPath(string fileName)
        {
            // 확장자 .json을 자동으로 붙여줍니다.
            if (!fileName.EndsWith(".json")) fileName += ".json";
            return Path.Combine(GetSaveDirectory(), fileName);
        }

        public static void Save(ShipSaveData data, string fileName)
        {
            var fullPath = GetFullPath(fileName);

            try
            {
                // 1. 순수 C# 객체를 JSON 문자열로 변환합니다.
                // (true를 넣으면 줄바꿈이 예쁘게 들어가서 사람이 읽기 편해집니다)
                var jsonString = JsonUtility.ToJson(data, true);

                // 2. 하드디스크에 파일로 씁니다.
                File.WriteAllText(fullPath, jsonString);

                Debug.Log($"[SaveLoadManager] 데이터 저장 성공!\n경로: {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadManager] 저장 중 오류 발생: {e.Message}");
            }
        }

        // ==========================================
        // 📖 불러오기 (Load)
        // ==========================================
        public static ShipSaveData Load(string fileName)
        {
            var fullPath = GetFullPath(fileName);

            // 파일이 존재하는지 먼저 방어적으로 검사합니다.
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[SaveLoadManager] 세이브 파일이 없습니다: {fileName}");
                return null;
            }

            try
            {
                // 1. 하드디스크에서 JSON 문자열을 텍스트로 읽어옵니다.
                var jsonString = File.ReadAllText(fullPath);

                // 2. 문자열을 다시 C# 객체로 조립하여 반환합니다.
                var loadedData = JsonUtility.FromJson<ShipSaveData>(jsonString);

                Debug.Log($"[SaveLoadManager] 데이터 로드 성공: {fileName}");
                return loadedData;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadManager] 로드 중 오류 발생 (데이터 오염 가능성): {e.Message}");
                return null;
            }
        }
    }
}