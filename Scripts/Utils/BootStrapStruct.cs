using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;


/// <summary>
/// //查找C#中引用Lua的函数
/// </summary>
public delegate void ShowCSharpRef();

public delegate void LifeCycle();
//单参数
public delegate void LifeCycleOneP(Collision collider);

/// <summary>
/// Lua生命周期向导结构体，用于接收Lua的表
/// </summary>
[GCOptimize]
[CSharpCallLua]
public struct BootStrapStruct
{
    public LifeCycle Awake;
    public LifeCycle Start;
    public LifeCycle FixedUpDate;
    public LifeCycleOneP OnCollisionEnter;
    public LifeCycle UpDate;
    public LifeCycle LateUpDate;
    public LifeCycle OnDestroy;

    /// <summary>
    /// 清除委托方法
    /// </summary>
    public void Clear()
    {
        this.Awake = null;
        this.Start = null;
        this.FixedUpDate = null;
        this.OnCollisionEnter = null;
        this.UpDate = null;
        this.LateUpDate = null;
        this.OnDestroy = null;
    }
}
