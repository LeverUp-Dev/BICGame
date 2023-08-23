using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    public bool isDestroyProtected;

    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
                throw new System.Exception($"{typeof(T).Name} 싱글톤 instance 레퍼런스가 유실되었습니다.");

            return instance;
        }

        private set
        {
            instance = value;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
            instance = this as T;
        else
            Destroy(gameObject);

        if (isDestroyProtected)
            DontDestroyOnLoad(gameObject);
    }
}