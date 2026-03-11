using System;
using System.IO;
using UnityEngine;

namespace Core.Data.Storage
{
    public class SaveLoadManager
    {
        private static string GetSaveDirectory()
        {
            var path = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        private static string GetFullPath(string fileName)
        {
            if (!fileName.EndsWith(".json")) fileName += ".json";
            return Path.Combine(GetSaveDirectory(), fileName);
        }

        public static void Save<T>(T data, string fileName)
        {
            var fullPath = GetFullPath(fileName);
            try
            {
                File.WriteAllText(fullPath, JsonUtility.ToJson(data, true));
                Debug.Log($"[SaveLoadManager] 저장 성공: {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadManager] 저장 오류: {e.Message}");
            }
        }

        /// <summary>파일이 없으면 null을 반환합니다.</summary>
        public static T Load<T>(string fileName) where T : class
        {
            var fullPath = GetFullPath(fileName);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[SaveLoadManager] 세이브 파일 없음: {fileName}");
                return null;
            }
            try
            {
                var loaded = JsonUtility.FromJson<T>(File.ReadAllText(fullPath));
                Debug.Log($"[SaveLoadManager] 로드 성공: {fileName}");
                return loaded;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadManager] 로드 오류 (데이터 오염 가능): {e.Message}");
                return null;
            }
        }
    }
}
