using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 掉落物品基类
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public class Item : MonoBehaviour
{
    [Header("物品SO")]
    public Item_SO itemSO;

    //物品类型
    public ItemType ItemType
    {
        get
        {
            return itemSO.itemType;
        }
        set
        {
            itemSO.itemType = value;
        }
    }

    //数量
    public int Count
    {
        get
        {
            return itemSO.count;
        }
        set
        {
            itemSO.count = value;
        }
    }

    //描述
    public string Description
    {
        get
        {
            return itemSO.description;
        }
    }

    protected bool canDrop;

    private void Awake()
    {
        canDrop = false;
    }

    private void Update()
    {
        if (canDrop)
        {
            //检测按键
            DropItemsController();
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //使用胶囊碰撞体作为触发器的载体，记得将IsTrigger打勾并调整胶囊体范围
        if (other.CompareTag("Player"))
        {
            canDrop = true;
            Debug.Log("可捡起");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canDrop = false;
            Debug.Log("不可捡起");
        }
    }

    /// <summary>
    /// 捡起物品
    /// </summary>
    public void DropItemsController()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            //todo:背包操作，物品加一等
            Debug.Log("捡起：" + ItemType);
            Count++;


            //销毁
            GameObject.Destroy(gameObject,0.2f);
        }
    }

    /// <summary>
    /// 提供子类重写使用物品后的效果
    /// </summary>
    public virtual void UseItem()
    {
        Count--;
        //一下是不同物品的不同效果，需要重写
    }
}
