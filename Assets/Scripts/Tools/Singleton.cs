using UnityEngine;

namespace Tools
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();

                    if (instance == null)
                    {
                        var go = new GameObject(typeof(T).Name);
                        instance = go.AddComponent<T>();
                        DontDestroyOnLoad(go);
                    }
                }

                return instance;

                
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = (T)this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}