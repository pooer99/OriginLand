

/// <summary>
/// 游戏结束观察者接口
/// 功能：广播、订阅
/// </summary>
public interface IEndGameObserver
{
    /// <summary>
    /// 游戏结束广播
    /// </summary>
    void EndNotify();
}
