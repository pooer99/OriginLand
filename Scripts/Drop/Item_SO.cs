using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 物品类型枚举
/// </summary>
public enum ItemType
{
    None,
    HealthPotion,
    ExpPotion
}

/// <summary>
/// ScriptableObject
/// 功能：用于生成掉落物品的ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Data", menuName = "Item Data/Data")]
public class Item_SO : ScriptableObject
{
    [Header("物品种类")]
    public ItemType itemType;

    [Header("数量")]
    public int count;

    [Header("描述")]
    [TextArea]
    public string description;
}
