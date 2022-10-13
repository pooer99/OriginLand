using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 经验药水类
/// </summary>
public class ExpPotion : Item
{
    public override void UseItem()
    {
        base.UseItem();

        //加10经验
        GameManager.Instance.playerStats.CurrentExp += 10;
    }
}
