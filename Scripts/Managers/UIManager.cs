using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 背包核心管理类
/// </summary>
public class UIManager : SingleTon<UIManager>
{
    [Header("背包预制体")]
    [Header("UI组件")]
    public GameObject inventoryMenuPrefab;

    //背包
    private GameObject inventoryMenu;

    [Header("掉落物品预制体")]
    public List<GameObject> itemPrefabs;

    [Header("装载掉落物品的父对象")]
    public GameObject itemsParent;

    [Header("背包列表")]
    //背包物品列表,存放物品的SO
    public List<Item_SO> itemsList;

    private Text descriptionText;

    //库存面，用于显示物品
    private GameObject inventoryPanel;

    //空物品格图标、颜色
    public Sprite emptyIcon;
    public Color emptyColor;

    //当前选中的Slot位置
    private int currentSlot = 0;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        itemsList = new List<Item_SO>();

    }

    private void Update()
    {
        InventoryController();
    }

    private void LateUpdate()
    {
        if(inventoryMenu!=null)
            UpdateBackpiack();

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

            //todo:有bug，捡起物品前必须先打开背包一次
            inventoryPanel = inventoryMenu.transform.GetChild(2).GetChild(0).gameObject;

            //信息栏
            descriptionText = inventoryMenu.transform.GetChild(2).GetChild(1).GetChild(0).GetChild(1).GetComponent<Text>();

            //初始化背包
            InitBackpiack();
        }

        Time.timeScale = 0.0f;
        GameManager.Instance.isPaused = true;

        //清空信息栏
        descriptionText.text = "";
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

    /// <summary>
    /// 拾起掉落物品
    /// </summary>
    /// <param name="so">物品的ScriptableObject</param>
    public void PickUpItems(Item_SO so)
    {
        //判断List是否已满,背包容量为24
        if (itemsList.Count == 24)
        {
            Debug.Log("背包已满");
            //todo:背包已满操作
        }
        else
        {
            if (!itemsList.Contains(so))
            {
                //若背包中没有物品相应的SO
                itemsList.Add(so);

            }
            //已有，则不操作
        }

    }

    /// <summary>
    /// 生成掉落物品
    /// </summary>
    /// <param name="pos">生成位置，与怪物相同</param>
    public void InstantiateItems(Transform pos)
    {
        //后期需要考虑概率的问题,目前不考虑
        Instantiate(itemPrefabs[0],pos.position,Quaternion.identity).transform.parent = itemsParent.transform;
    }

    /// <summary>
    /// 初始化背包
    /// </summary>
    public void InitBackpiack()
    {
        //检测掉落物品预制体中对应的SO中物品的数量
        for(int i = 0; i < itemPrefabs.Count; i++)
        {
            //大于0则在背包中显示
            if (itemPrefabs[i].GetComponent<Item>().Count > 0)
            {
                itemsList.Add(itemPrefabs[i].GetComponent<Item>().itemSO);
            }
        }
    }

    /// <summary>
    /// 用于更新、显示背包物品图标和数量
    /// </summary>
    public void UpdateBackpiack()
    {
        GameObject tempSlot;
        Color changeColor = new Color(255, 255, 255, 255);

        //物品格元素
        GameObject itemImage;
        GameObject count;
        GameObject leave;

        for (int i = 0; i < 24; i++)
        {
            //按顺序判断背包列表中的物体数量
            if (itemsList.Count-1 >= i)
            {
                //获取物品格
                tempSlot = inventoryPanel.transform.GetChild(i).gameObject;

                changeColor.a = 255;

                //获取图标区
                itemImage = tempSlot.transform.GetChild(0).gameObject;
                
                //修改物品格图标
                itemImage.GetComponent<Image>().sprite = itemsList[i].icon;
                itemImage.GetComponent<Image>().color = changeColor;

                //获取丢弃图标、物品数量区
                count = tempSlot.transform.GetChild(1).gameObject;
                leave = tempSlot.transform.GetChild(2).gameObject;

                //修改数量字体和退出图标透明度,显示他们
                //修改真实数量
                count.GetComponent<Text>().text = itemsList[i].count.ToString();
                count.GetComponent<Text>().color = changeColor;
                leave.GetComponent<Image>().color = changeColor;

            }
            else
            {
                //获取物品格
                tempSlot = inventoryPanel.transform.GetChild(i).gameObject;

                //获取图标区
                itemImage = tempSlot.transform.GetChild(0).gameObject;

                //修改物品格图标为空
                itemImage.GetComponent<Image>().sprite = emptyIcon;
                itemImage.GetComponent<Image>().color = emptyColor;

                //获取丢弃图标、物品数量区
                count = tempSlot.transform.GetChild(1).gameObject;
                leave = tempSlot.transform.GetChild(2).gameObject;

                //修改数量字体和退出图标透明度,显示他们
                //修改数量为0
                count.GetComponent<Text>().text = "0";
                changeColor.a = 0;
                count.GetComponent<Text>().color = changeColor;
                leave.GetComponent<Image>().color = changeColor;

            }
        }

    }

    /// <summary>
    /// 用于获取点击的item的位置
    /// </summary>
    /// <param name="pos">点击的Slot的位置</param>
    public void SelectItem(int pos)
    {
        //逻辑上Slot的位置与背包列表的相同
        currentSlot = pos;

        //显示物品信息

        //点击空格则显示空白信息
        if (itemsList.Count - 1 >= pos)
            descriptionText.text = itemsList[pos].description;
        else
            descriptionText.text = "";
    }

    /// <summary>
    /// 使用物品，提供给按钮使用
    /// </summary>
    public void UseItems()
    {

        if (itemsList.Count-1>=currentSlot)
        {
            //检测当前的物品的类型
            switch (itemsList[currentSlot].itemType)
            {
                //根据背包中选中的物品的类型去背包预制体列表中查询出相应的预制体，目的是为了拿到其脚本
                case ItemType.HealthPotion:

                    //执行使用回复药水的方法
                    itemPrefabs[0].GetComponent<HealthPotion>().UseItem();

                    break;
                case ItemType.ExpPotion:

                    //执行使用回复药水的方法
                    itemPrefabs[1].GetComponent<ExpPotion>().UseItem();

                    break;
            }

            //用完，将其SO移除背包
            if (itemsList[currentSlot].count == 0)
            {
                //清除信息框信息
                descriptionText.text = "";

                itemsList.Remove(itemsList[currentSlot]);
            }
        }
       
    }

}
