using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 游戏核心管理类
/// </summary>
public class GameManager : SingleTon<GameManager>
{
    //游戏是否暂停
    public bool isPaused;

    //Player状态数据
    public CharacterStats playerStats;

    //所有订阅了游戏结束广播的敌人
    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();

    /// <summary>
    /// 广播注册Player状态
    /// </summary>
    /// <param name="stats"></param>
    public void RigisterPlayer(CharacterStats stats)
    {
        playerStats = stats;
    }

    /// <summary>
    /// 将所有订阅了广播的敌人添加到列表
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }

    /// <summary>
    /// 移除订阅了广播的敌人
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    } 

    /// <summary>
    /// 执行所以订阅的广播方法
    /// </summary>
    public void NotifyObserrvers()
    {
        foreach (var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void OnDestroy()
    {
        //如果加了销毁，会使在连续的异步场景加载使丢失Instance
        //GameManager.Instance.Clear();
    }
}
