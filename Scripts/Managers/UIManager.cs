using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 背包核心管理类
/// </summary>
public class UIManager : SingleTon<UIManager>
{
    [Header("库存菜单预制体")]
    [Header("UI组件")]
    public GameObject inventoryMenuPrefab;

    private GameObject inventoryMenu;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        InventoryController();
    }

    /// <summary>
    /// 控制背包界面
    /// </summary>
    private void InventoryController()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //如果游戏暂停中，按下退出键，游戏继续
            if (GameManager.Instance.isPaused)
            {
                Resume();
            }
            
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            //如果游戏正在运行，按下I，游戏停止，弹出背包
            if (!GameManager.Instance.isPaused)
            {
                Pause();
            }
        }
    }

    /// <summary>
    /// 游戏暂停类，实现暂停后游戏的一些操作
    /// </summary>
    private void Pause()
    {
        //游戏暂停，打开背包界面
        if(inventoryMenu!=null)
            inventoryMenu.gameObject.SetActive(true);
        else
        {
            //生成背包系统UI
            inventoryMenu = Instantiate(inventoryMenuPrefab);
            
            inventoryMenu.gameObject.SetActive(true);
        }

        Time.timeScale = 0.0f;
        GameManager.Instance.isPaused = true;
    }

    /// <summary>
    /// 游戏继续类，实现继续后游戏的一些操作
    /// </summary>
    private void Resume()
    {
        //游戏继续，关闭背包界面
        inventoryMenu.gameObject.SetActive(false);
        Time.timeScale = 1.0f;
        GameManager.Instance.isPaused = false;
    }
}
