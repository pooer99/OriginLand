using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 回血药水类
/// </summary>
public class HealthPotion : Item
{
    public override void UseItem()
    {
        base.UseItem();

        //加30血
        GameManager.Instance.playerStats.CurrentHealth += 30;
    }
}
