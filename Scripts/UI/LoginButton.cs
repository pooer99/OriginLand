using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 登录按钮类
/// </summary>
public class LoginButton : MonoBehaviour
{

    [Header("账号输入")]
    public GameObject accountText;

    [Header("密码输入")]
    public GameObject passwordText;

    [Header("登录按钮")]
    private Button loginButton;

    [Header("登录错误提示框")]
    public Text tipText;

    //输入账号
    private string account;

    //输入密码
    private string password;

    //是否能提交账号密码，用于反正按下按钮那一瞬间一直执行监听事件
    private bool canSubmit;
    //提交等待时间
    private float waitSubmitTime = 2f;
    //计时器
    private float submitTime;

    private void Awake()
    {
        loginButton = GetComponent<Button>();

        canSubmit = true;
        submitTime = waitSubmitTime;
    }

    private void Update()
    {
        //监听按钮
        loginButton.onClick.AddListener(Submit);

        if (!canSubmit)
            submitTime -= Time.deltaTime;

        //时间到了，可以继续提交了
        if(submitTime<=0)
        {
            canSubmit = true;
            submitTime = waitSubmitTime;
        }
    }

    /// <summary>
    /// 按钮监听事件
    /// </summary>
    private void Submit()
    {
        if (canSubmit)
        {
            //禁止连续提交
            canSubmit = false;

            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            //获取输入框账号、密码
            account = accountText.GetComponent<Text>().text;
            password = passwordText.GetComponent<Text>().text;

            //判断是否登录成功
            LoginErrorType Type = LoginManager.Instance.LoginAccount(account, password);
            
            switch (Type)
            {
                case LoginErrorType.ACCESS:
                    //todo:登陆成功
                    //tipText.GetComponent<Text>().text = "成功！";

                    //切换过程状态,菜单状态
                    LoginManager.Instance.loginProgressStats = LoginProgressStats.MENU;
                    break;
                case LoginErrorType.ACCOUTERROR:
                    tipText.GetComponent<Text>().text = "账号不存在！";
                    break;
                case LoginErrorType.PASSWORDERROR:
                    tipText.GetComponent<Text>().text = "密码错误！";
                    break;
            }
        }
       
    }
}
