using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 血条UI类
/// </summary>
public class HealthBar : MonoBehaviour
{
    //角色当前状态
    private CharacterStats currentStats;

    [Header("Canvas")]
    public GameObject healthBarCanvas;

    [Header("预制体")]
    public GameObject healthBarPrefab;

    [Header("血条位置")]
    public Transform barPoint;

    [Header("总是可见")]
    public bool alwaysVisible;

    [Header("可视化时间")]
    public float visibleTime;
    //剩余显示时间
    private float timeLeft;

    //获取预制体的血条的滑动条
    private Image healthSlider;

    //获取生成的血条的位置，之后需要将血条移到角色头顶
    private Transform UIBar;

    //主相机
    private Transform mCamera;

    private void Awake()
    {
        currentStats = GetComponent<CharacterStats>();

        //添加血条更新方法
        currentStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    private void OnEnable()
    {
        mCamera = Camera.main.transform;

        //todo:遍历场景中所有WorldSpace的canvas，并生成

        //获取生成血条的transfrom
        UIBar = Instantiate(healthBarPrefab, healthBarCanvas.transform).transform;

        //获取血条滑动条
        healthSlider = UIBar.GetChild(0).GetComponent<Image>();

        //总是可见？
        UIBar.gameObject.SetActive(alwaysVisible);

    }

    private void LateUpdate()
    {
        if (UIBar != null)
        {
            //将生成的血条移到角色头顶
            UIBar.position = barPoint.position;

            //始终面向玩家
            UIBar.forward = -mCamera.forward;

            //未勾选"总是可见"并且计时结束则不显示血条
            if (timeLeft <= 0 && !alwaysVisible)
            {
                UIBar.gameObject.SetActive(false);
            }
            else
                timeLeft -= Time.deltaTime;
        }
    }


    /// <summary>
    /// 更新血条方法
    /// </summary>
    /// <param name="currentHealth">当前生命</param>
    /// <param name="Health">初始生命</param>
    private void UpdateHealthBar(int currentHealth, int Health)
    {
        //血条小于0，不显示UI
        if (currentHealth <= 0)
            Destroy(UIBar.gameObject);
        else
        {
            //被攻击时，显示
            UIBar.gameObject.SetActive(true);

            //计时显示血条
            timeLeft = visibleTime;

            //实时计算滑动条百分比
            float sliderPercent = (float)currentHealth / Health;

            healthSlider.fillAmount = sliderPercent;
        }

        
    }
}
