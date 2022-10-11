using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using DG.Tweening;

/// <summary>
/// 开始游戏的类型：是新游戏还是继续游戏
/// </summary>
public enum LoadGameType
{
    NEWGAME,
    CONTINUE
}

/// <summary>
/// 主菜单类
/// </summary>
public class Menu : MonoBehaviour
{
    [Header("菜单按钮")]
    public GameObject buttonBody;

    //菜单动画
    private PlayableDirector director;

    private Button newGameBtn;
    private Button continueBtn;
    private Button exitBtn;

    //显示按钮
    private bool showButton;

    //能否操作，反正点击按钮时，多次进行监听事件
    [HideInInspector]
    public bool canNewGame;
    [HideInInspector]
    public bool canContinue;
    [HideInInspector]
    public bool canExit;

    private void Awake()
    {
        showButton = true;

        director = FindObjectOfType<PlayableDirector>();
        //给动画结束时添加事件：开始游戏
        director.stopped += NewGame;

        newGameBtn = buttonBody.transform.GetChild(0).GetComponent<Button>();
        continueBtn = buttonBody.transform.GetChild(1).GetComponent<Button>();
        exitBtn = buttonBody.transform.GetChild(2).GetComponent<Button>();

        canNewGame = true;
        canContinue = true;
        canExit = true;
    }

    private void Update()
    {
      if(LoginManager.Instance.loginProgressStats == LoginProgressStats.MENU&&showButton)
        {
            showButton = false;
            buttonBody.SetActive(true);
        }

      //监听按钮
        newGameBtn.onClick.AddListener(PlayTimeline);
        continueBtn.onClick.AddListener(ContinueGame);
        exitBtn.onClick.AddListener(ExitGame);
    }

    /// <summary>
    /// 新游戏
    /// </summary>
    private void NewGame(PlayableDirector obj)
    {
        //异步加载场景
        LoginManager.Instance.LoadMainScene("MainScene", LoadGameType.NEWGAME);
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    private void ContinueGame()
    {
        if (canContinue)
        {
            canContinue = false;

            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            //异步加载场景
            LoginManager.Instance.LoadMainScene("MainScene", LoadGameType.CONTINUE);
        }
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    private void ExitGame()
    {
        if (canExit)
        {
            canExit = false;

            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            Application.Quit();
        }
    }

    /// <summary>
    /// 播放菜单动画
    /// </summary>
    private void PlayTimeline()
    {
        if (canNewGame)
        {
            canNewGame = false;

            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            //实现按钮图片效果逐渐消失
            newGameBtn.GetComponent<Image>().DOFade(0,1f);
            continueBtn.GetComponent<Image>().DOFade(0,1f);
            exitBtn.GetComponent<Image>().DOFade(0,1f);

            //播放动画
            director.Play();
        }
    }
}
