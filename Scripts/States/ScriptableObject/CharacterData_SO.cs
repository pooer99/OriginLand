using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ScriptableObject
/// 功能：生成角色数据的ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("基本设置")]
    //基础血量
    public int health;

    //当前血量
    public int currentHealth;

    //基础防御
    public int defence;

    //当前防御
    public int currentDefence;
}
