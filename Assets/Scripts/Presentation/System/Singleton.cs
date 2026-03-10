using UnityEngine;

namespace Presentation.System
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isQuitting; // 앱 종료 시 찌꺼기 생성 방지용

        public static T Instance
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning($"[Singleton] 앱이 종료되는 중입니다. {typeof(T)}의 인스턴스를 반환하지 않습니다.");
                    return null;
                }

                if (_instance == null)
                {
                    // 1. 씬에 이미 해당 객체가 있는지 찾아봅니다.
                    _instance = FindObjectOfType<T>();

                    // 2. 씬에 없다면 새로 빈 게임 오브젝트를 만들어서 컴포넌트를 붙여줍니다.
                    if (_instance == null)
                    {
                        var singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T) + " (Singleton)";

                        // 💡 기본적으로 씬이 넘어가도 파괴되지 않도록 설정합니다.
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            // 씬에 이미 다른 인스턴스가 존재한다면, 새로 생성된 자신을 파괴합니다. (중복 방지)
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                // 자신이 유일한 인스턴스라면 등록하고 파괴 방지 처리를 합니다.
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
        }

        // 게임 종료 시 불필요한 재생성을 막기 위한 안전장치
        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }
    }
}