using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 开始提示类
/// </summary>
public class StartTip : MonoBehaviour
{
    private Text tip;

    [Header("字体渐变间隔时间")]
    public float fadeTime;
    //计时器
    private float waitTime;

    //渐变标记
    private bool canFade;

    //初始化颜色
    private Color textColor;

    private void Awake()
    {
        tip = GetComponent<Text>();

        waitTime = fadeTime;

        canFade = true;

        textColor = tip.GetComponent<Text>().color;

    }


    private void Update()
    {
        //检测回车键
        if (Input.GetKeyDown(KeyCode.Return))
        {
            LoginManager.Instance.loginProgressStats = LoginProgressStats.LOGIN;

            Destroy(this);
        }

        Fade();

        //等待
        if (!canFade)
            waitTime -= Time.deltaTime;

        //时间到，开始渐变
        if (waitTime <= 0)
        {
            canFade = true;
            //重置计时器
            waitTime = fadeTime;

            //重置透明度
            tip.GetComponent<Text>().color = textColor;
        }


    }

    /// <summary>
    /// 实现渐变效果
    /// </summary>
    private void Fade()
    {
        if (canFade)
        {
            canFade = false;
            //透明度从0到255
            tip.DOFade(0, 2f);
        }
           
    }
}
