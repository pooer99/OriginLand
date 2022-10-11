using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 用于在菜单界面注册Player状态信息
/// </summary>
public class MenuConnectStats : MonoBehaviour
{
    private CharacterStats stats;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
    }

    private void Start()
    {
        //注册Player状态，若不在开始时注册，则SaveManager无法获取Player状态
        GameManager.Instance.RigisterPlayer(stats);
    }
}
