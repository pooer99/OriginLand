using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 登录错误类型枚举，用于返回错误类型
/// </summary>
public enum LoginErrorType
{
    ACCESS,//登录成功
    ACCOUTERROR,//账号不存在
    PASSWORDERROR//密码错误
}

/// <summary>
/// 登录过程状态枚举
/// </summary>
public enum LoginProgressStats
{
    PRESSENTER,//初始
    LOGIN,//登录
    MENU//菜单
}

/// <summary>
/// 注册错误类型枚举
/// </summary>
public enum RegisterErrorType
{
    ACCESS,//登录成功
    ACCOUTERROR,//账号已存在
    PASSWORDERROR//密码输入不一致
}

/// <summary>
/// 登录管理核心类
/// </summary>
public class LoginManager : SingleTon<LoginManager>
{
    //记录登录过程状态，便于切换UI
    public LoginProgressStats loginProgressStats;

    [Header("登录框")]
    public GameObject LoginCanvas;

    [Header("加载界面预制体")]
    public GameObject loadCanvasPrefab;

    private GameObject loadProgress;

    //进度条图片
    private Image progressImage;

    //进度条文字
    private Text progressText;

    //用于获取异步加载信息，制作进度条
    private AsyncOperation operation;

    //是否已登录
    private bool isLogin;



    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        //初始状态
        loginProgressStats = LoginProgressStats.PRESSENTER;
        isLogin = false;
    }

    private void Start()
    {
        SoundManager.Instance.PlayeMusic(0);
    }

    private void Update()
    {
        //切换过程状态
        SwitchProgressStats();
    }

    /// <summary>
    /// 功能：调用数据库管理的登录方法
    /// </summary>
    /// <param name="account">账号</param>
    /// <param name="password">密码</param>
    /// <returns></returns>
    public LoginErrorType LoginAccount(string account, string password)
    {
        return MysqlManager.Instance.Login(account, password);

    }

    /// <summary>
    /// 功能：调用数据库管理的注册方法
    /// </summary>
    /// <param name="account">账号</param>
    /// <param name="password">密码</param>
    /// <returns></returns>
    public RegisterErrorType RegisterAccount(string account, string password)
    {
        return MysqlManager.Instance.Register(account,password);
    }

    private void SwitchProgressStats()
    {
        switch (loginProgressStats)
        {
            case LoginProgressStats.PRESSENTER:
                break;
            case LoginProgressStats.LOGIN:

                //激活登录框
                if (!LoginCanvas.activeInHierarchy)
                {
                    //播放音弹窗音效
                    SoundManager.Instance.PlayOnShot(4);

                    LoginCanvas.SetActive(true);
                }
                    
                break;
            case LoginProgressStats.MENU:
                isLogin = true;

                //销毁登录框
                if(LoginCanvas!=null)
                    Destroy(LoginCanvas);
                


                break;
        }
    }

    /// <summary>
    /// 加载主场景
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="loadGameType">新游戏还是继续游戏</param>
    public void LoadMainScene(string sceneName,LoadGameType loadGameType)
    {
        StartCoroutine(LoadScene(sceneName,loadGameType));
    }

    /// <summary>
    /// 加载主场景协程
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="loadGameType"></param>
    /// <returns></returns>
    IEnumerator LoadScene(string sceneName,LoadGameType loadGameType)
    {
        //停止播放背景音乐
        SoundManager.Instance.StopMusic(0);

        switch (loadGameType)
        {
            case LoadGameType.NEWGAME:

                //播放过渡场景
                yield return loadProgress = Instantiate(loadCanvasPrefab);

                yield return progressImage = loadProgress.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Image>();

                yield return progressText = loadProgress.transform.GetChild(1).GetChild(0).GetChild(1).GetComponent<Text>();

                //等待异步加载场景，加载完，才继续
                operation = SceneManager.LoadSceneAsync(sceneName);

                //不允许场景自动跳转
                operation.allowSceneActivation = false;

                //场景加载没有完成时
                while (!operation.isDone)
                {
                    //滑动条 = 场景加载进度
                    progressImage.fillAmount = operation.progress;
                    progressText.text = operation.progress * 100 + "%";

                    //只能显示到0.9，之后的0.1需要手动添加
                    if (operation.progress >= 0.9F)
                    {
                        //速度太快了,做个延时
                        yield return new WaitForSeconds(2f);

                        progressImage.fillAmount = 1.0f;
                        progressText.text = "100%";

                        //允许场景自动跳转
                        operation.allowSceneActivation = true;
                    }

                    //跳出协程
                    yield return null;
                }

                yield return new WaitForSeconds(0.2f);

                //播放主场景音乐
                SoundManager.Instance.isCycle = true;//循环
                SoundManager.Instance.PlayeMusic(1);
                break;
            case LoadGameType.CONTINUE:
                yield return SceneManager.LoadSceneAsync(sceneName);

                //播放过渡场景
                yield return loadProgress = Instantiate(loadCanvasPrefab);

                yield return progressImage = loadProgress.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Image>();

                yield return progressText = loadProgress.transform.GetChild(1).GetChild(0).GetChild(1).GetComponent<Text>();

                //等待异步加载场景，加载完，才继续
                operation = SceneManager.LoadSceneAsync(sceneName);

                //不允许场景自动跳转
                operation.allowSceneActivation = false;

                //场景加载没有完成时
                while (!operation.isDone)
                {
                    //滑动条 = 场景加载进度
                    progressImage.fillAmount = operation.progress;
                    progressText.text = operation.progress * 100 + "%";

                    //只能显示到0.9，之后的0.1需要手动添加
                    if (operation.progress >= 0.9F)
                    {
                        //速度太快了,做个延时
                        yield return new WaitForSeconds(2f);

                        progressImage.fillAmount = 1.0f;
                        progressText.text = "100%";

                        //允许场景自动跳转
                        operation.allowSceneActivation = true;
                    }

                    //跳出协程
                    yield return null;
                }

                yield return new WaitForSeconds(0.2f);
                //读档
                SaveManager.Instance.LoadPlayerData();

                //播放主场景音乐
                SoundManager.Instance.isCycle = true;//循环
                SoundManager.Instance.PlayeMusic(1);

                break;
        }
       
        yield return null;
    }
}
