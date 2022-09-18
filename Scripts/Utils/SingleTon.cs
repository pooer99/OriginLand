using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 工具类：泛型的单例模式模板
/// </summary>
public class SingleTon<T> : MonoBehaviour where T:SingleTon<T>
{
    private static T instance;

    public static T Instance
    {
        get { return instance; }
    }

    //便于子类初始化
    protected virtual void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = (T)this;

    }

    /// <summary>
    /// 周期结束直接销毁，控制只有一个单例
    /// </summary>
    protected void Clear()
    {
        if (instance == this)
            instance = null;
    }

    //判断当前单例是否已经生成
    public static bool isInitialized
    {
        get { return instance != null; }
    }

}
