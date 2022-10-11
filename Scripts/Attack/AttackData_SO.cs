using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ScriptableObject
/// 功能：生成角色攻击数据的ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Data", menuName = "Attack Data/Data")]
public class AttackData_SO : ScriptableObject
{
    [Header("最小伤害")]
    public int minDamage;

    [Header("最大伤害")]
    public int maxDamage;

    [Header("攻击距离")]
    public float attackRange;

    [Header("技能距离")]
    public float skillRange;

    [Header("普攻CD")]
    public float coolDown;

    [Header("技能CD")]
    public float skillCoolDown;

    [Header("暴击伤害加成百分比")]
    public float criticalMul;

    [Header("暴击率")]
    public float criticalChance;
}
