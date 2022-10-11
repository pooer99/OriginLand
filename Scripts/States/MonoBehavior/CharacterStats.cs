using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// MonoBehaviour
/// 功能：管理角色数据的ScriptableObject
/// </summary>
public class CharacterStats : MonoBehaviour
{
    //复制相同的敌人共用一个SO
    [Header("模板数据")]
    public CharacterData_SO templateData;

    //获取角色SO
    [Header("角色状态数据")]
    public CharacterData_SO characterData;

    [Header("角色攻击数据")]
    public AttackData_SO attackData;

    //是否暴击
    [HideInInspector]
    public bool isCritical;

    //更新血条事件
    public event Action<int, int> UpdateHealthBarOnAttack;

    #region 基础数据属性

    //基础血量
    public int Health 
    {
        get
        {
            if (characterData != null)
                return characterData.health;
            return 0;
        }
        set
        {
            characterData.health = value;
        }
    }

    //当前血量
    public int CurrentHealth
    {
        get
        {
            if (characterData != null)
                return characterData.currentHealth;
            return 0;
        }
        set
        {
            characterData.currentHealth = value;
        }
    }

    //基础防御
    public int Defence
    {
        get
        {
            if (characterData != null)
                return characterData.defence;
            return 0;
        }
        set
        {
            characterData.defence = value;
        }
    }

    //当前防御
    public int CurrentDefence
    {
        get
        {
            if (characterData != null)
                return characterData.currentDefence;
            return 0;
        }
        set
        {
            characterData.currentDefence = value;
        }
    }

    //死亡掉落经验
    public int KillPoint
    {
        get
        {
            if (characterData != null)
                return characterData.killPoint;
            return 0;
        }
        set
        {
            characterData.killPoint = value;
        }
    }

    //升级
    //当前等级
    public int CurrentLevel
    {
        get
        {
            if (characterData != null)
                return characterData.currentLevel;
            return 0;
        }
        set
        {
            characterData.currentLevel = value;
        }
    }

    //最高等级
    public int MaxLevel
    {
        get
        {
            if (characterData != null)
                return characterData.maxLevel;
            return 0;
        }
        set
        {
            characterData.maxLevel = value;
        }
    }

    //基础经验
    public int BaseExp
    {
        get
        {
            if (characterData != null)
                return characterData.baseExp;
            return 0;
        }
        set
        {
            characterData.baseExp = value;
        }
    }

    //当前经验
    public int CurrentExp
    {
        get
        {
            if (characterData != null)
                return characterData.currentExp;
            return 0;
        }
        set
        {
            characterData.currentExp = value;
        }
    }

    //基础等级经验加成
    //下一个等级的经验 = 当前等级经验*( 1 + levelBuff )
    public float LevelBuff 
    {
        get
        {
            if (characterData != null)
                return characterData.levelBuff;
            return 0;
        }
        set
        {
            characterData.levelBuff = value;
        }
    }

    //当前等级经验加成
    //随着等级的提升，等级经验加成也会提升
    public float LevelMul
    {
        get
        {
            return 1 + (CurrentLevel - 1) * LevelBuff;
        }
    }

    #endregion

    #region 基础攻击属性

    //最小伤害
    public int MinDamage
    {
        get
        {
            if (attackData != null)
                return attackData.minDamage;
            return 0;
        }
        set
        {
            attackData.minDamage = value;
        }
    }

    //最大伤害
    public int MaxDamage
    {
        get
        {
            if (attackData != null)
                return attackData.maxDamage;
            return 0;
        }
        set
        {
            attackData.maxDamage = value;
        }
    }

    //攻击距离
    public float AttackRange
    {
        get
        {
            if (attackData != null)
                return attackData.attackRange;
            return 0;
        }
        set
        {
            attackData.attackRange = value;
        }
    }

    //技能距离
    public float SkillRange
    {
        get
        {
            if (attackData != null)
                return attackData.skillRange;
            return 0;
        }
        set
        {
            attackData.skillRange = value;
        }
    }

    //普攻CD
    public float CoolDown
    {
        get
        {
            if (attackData != null)
                return attackData.coolDown;
            return 0;
        }
        set
        {
            attackData.coolDown = value;
        }
    }

    //技能CD
    public float SkillCoolDown
    {
        get
        {
            if (attackData != null)
                return attackData.skillCoolDown;
            return 0;
        }
        set
        {
            attackData.skillCoolDown = value;
        }
    }

    //暴击伤害加成百分比
    public float CriticalMul
    {
        get
        {
            if (attackData != null)
                return attackData.criticalMul;
            return 0;
        }
        set
        {
            attackData.criticalMul = value;
        }
    }

    //暴击率
    public float CriticalChance
    {
        get
        {
            if (attackData != null)
                return attackData.criticalChance;
            return 0;
        }
        set
        {
            attackData.criticalChance = value;
        }
    }

    #endregion

    #region 攻击
    /// <summary>
    /// 功能：造成伤害
    /// </summary>
    /// <param name="attacker">攻击者</param>
    /// <param name="defener">防御者</param>
    public void TakeDamage(CharacterStats attacker,CharacterStats defener)
    {
        if (defener == null)
            return;

        //造成伤害最小为1
        int damage = Mathf.Max(attacker.CurrentDamage() - defener.CurrentDefence,1);

        Debug.Log(attacker.name+"-造成伤害："+damage);

        //血量最小为0
        defener.CurrentHealth = Mathf.Max(defener.CurrentHealth - damage,0);

        //如果防御者被暴击则播放受伤动画
        if (isCritical)
        {
            Debug.Log(defener.name + "被击倒");
            defener.GetComponent<Animator>().SetTrigger("Hurt");
        }

        //血条UI
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, Health);

        //添加经验
        if (defener.CurrentHealth <= 0)
            attacker.UpdateExp(defener.KillPoint);
    }

    /// <summary>
    /// 根据最大、最小伤害，计算随机伤害值
    /// </summary>
    /// <returns></returns>
    private int CurrentDamage()
    {
        float damage = UnityEngine.Random.Range(attackData.minDamage,attackData.maxDamage);

        //暴击伤害 = 计算的随机伤害*暴击伤害百分比
        if (isCritical)
        {
            damage *= attackData.criticalMul;

            Debug.Log("暴击！造成伤害："+ damage);
        }
            

        return (int)damage;
    }

    #endregion

    #region 升级

    /// <summary>
    /// 更新经验值
    /// </summary>
    /// <param name="killPoint">掉落经验</param>
    public void UpdateExp(int killPoint)
    {
        CurrentExp += killPoint;

        //升级
        if (CurrentExp >= BaseExp)
            LevelUp();
    }

    /// <summary>
    /// 升级你想提升的数据方法
    /// </summary>
    private void LevelUp()
    {
        //修改等级 范围：(0,maxLevel)
        CurrentLevel = Mathf.Clamp(CurrentLevel + 1,0,MaxLevel);

        //修改经验
        BaseExp += (int)(BaseExp*LevelMul);
        CurrentExp = 0;

        //修改生命
        Health = (int)(Health*(1+LevelBuff));
        CurrentHealth = Health;

        //修改防御
        Defence = (int)(Defence*(1+LevelBuff));
        CurrentDefence = Defence;

        //修改攻击力
        MinDamage = (int)(MinDamage*(1+LevelBuff));
        MaxDamage = (int)(MaxDamage*(1+LevelBuff));

        Debug.Log("升级！血量："+Health+",防御："+Defence+",攻击力："+MinDamage+",经验："+BaseExp);
    }

    #endregion

    private void Awake()
    {
        if (templateData != null)
            characterData = Instantiate(templateData);
    }

}
