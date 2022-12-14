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
    [Header("基础血量")] [Header("基本设置")]
    public int health;

    [Header("当前血量")]
    public int currentHealth;

    [Header("基础防御")]
    public int defence;

    [Header("当前防御")]
    public int currentDefence;

    [Header("死亡掉落经验")]
    public int killPoint;

    [Header("当前等级")] [Header("升级")]
    public int currentLevel;

    [Header("最高等级")]
    public int maxLevel;

    [Header("基础经验")]
    public int baseExp;

    [Header("当前经验")]
    public int currentExp;

    [Header("等级经验加成")]
    public float levelBuff; //下一个等级的经验 = 当前等级经验*( 1 + levelBuff )
}
