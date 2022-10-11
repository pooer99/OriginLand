using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 注册按键类
/// </summary>
public class RegisterButton : MonoBehaviour
{
    private Button registerButton;

    [Header("账号输入框")]
    public Text accountText;

    [Header("密码输入框")]
    public Text passwordText;

    [Header("再次输入框")]
    public Text repeatText;

    [Header("提示框")]
    public Text tip;

    //防止点击一次多次调用监听事件
    private bool canRegister;
    //等待时间
    private float waitTime = 1f;
    //定时器
    private float timer;

    private void Awake()
    {
        registerButton = GetComponent<Button>();

        canRegister = true;
        timer = waitTime;
    }

    private void Update()
    {
        registerButton.onClick.AddListener(Register);

        //倒计时
        if (!canRegister)
            timer -= Time.deltaTime;

        //计时结束，允许再次运行事件
        if (timer <= 0)
        {
            canRegister = true;
            timer = waitTime;
        }
    }

    private void Register()
    {
        if (canRegister)
        {
            canRegister = false;

            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            string account = accountText.text;
            string password = passwordText.text;
            string repeat = repeatText.text;

            //两次密码输入不一致
            if (password != repeat)
            {
                tip.text = "密码输入不一致！";
            }
            else
            {
                RegisterErrorType errorType = LoginManager.Instance.RegisterAccount(account, password);

                switch (errorType)
                {
                    case RegisterErrorType.ACCESS:
                        tip.text = "注册成功！";
                        break;
                    case RegisterErrorType.ACCOUTERROR:
                        tip.text = "账号已存在！";
                        break;
                }
            }
        }
       
        
    }
}
