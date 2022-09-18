using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// MonoBehaviour
/// 功能：管理角色数据的ScriptableObject
/// </summary>
public class CharacterStats : MonoBehaviour
{
    //获取角色SO
    public CharacterData_SO characterData;

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
    #endregion
}
