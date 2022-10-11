using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 切换登录、注册界面按键类
/// </summary>
public class ChangeButton : MonoBehaviour
{
    [Header("登录布局")]
    public GameObject loginBody;

    [Header("注册布局")]
    public GameObject registerBody;

    //true为loginBody,false为registerBody
    private bool isLoginBody;

    //能否切换,防止按下按键后多次执行监听事件
    private bool canChange;
    //等待时间
    public float waitTime = 1f;
    //定时器
    private float timer;

    //自身文本
    private Text text;
    //自身按钮
    private Button button;

    private void Awake()
    {
        text = transform.GetChild(0).GetComponent<Text>();
        button = GetComponent<Button>();

        isLoginBody = true;
        canChange = true;

        timer = waitTime;
    }

    private void Update()
    {
        button.onClick.AddListener(changeBody);

        //倒计时
        if (!canChange)
            timer -= Time.deltaTime;

        //时间到，重置定时器，允许切换布局
        if (timer <= 0)
        {
            timer = waitTime;
            canChange = true;
        }
    }

    /// <summary>
    /// 切换布局
    /// </summary>
    private void changeBody()
    {
        if (canChange)
        {
            canChange = false;

            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            if (isLoginBody)
            {
                //切换注册
                text.text = "Login";

                loginBody.SetActive(false);

                registerBody.SetActive(true);

                isLoginBody = false;
            }
            else
            {
                //切换登录
                text.text = "Register";

                loginBody.SetActive(true);

                registerBody.SetActive(false);

                isLoginBody = true;
            }
        }
        
    }

}
