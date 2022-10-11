using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Player状态信息UI类
/// </summary>
public class PlayerStatsInfo : MonoBehaviour
{
    [Header("Player")]
    public GameObject player;
    private CharacterStats PlayerStatsData;

    [Header("名称")]
    public Text nameText;
    public string playerName;

    [Header("血条")]
    public Image healthBar;

    [Header("经验")]
    public Image expBar;

    [Header("等级")]
    public Text levelText;

    private void Awake()
    {
        PlayerStatsData = player.GetComponent<CharacterStats>();
    }

    private void Start()
    {
        //修改姓名
        nameText.text = playerName;

        //修改血条
        healthBar.transform.GetChild(0).GetComponent<Image>().fillAmount = PlayerStatsData.CurrentHealth / PlayerStatsData.Health;

        //修改经验
        expBar.transform.GetChild(0).GetComponent<Image>().fillAmount = PlayerStatsData.CurrentExp / PlayerStatsData.BaseExp;

        //修改等级
        levelText.text = PlayerStatsData.CurrentLevel.ToString();
    }

    private void LateUpdate()
    {
        UpdateStatsInfo();
    }

    /// <summary>
    /// 更新Player状态信息
    /// </summary>
    private void UpdateStatsInfo()
    {

        //修改血条
        healthBar.transform.GetChild(0).GetComponent<Image>().fillAmount = (float)PlayerStatsData.CurrentHealth / PlayerStatsData.Health;

        //修改经验
        expBar.transform.GetChild(0).GetComponent<Image>().fillAmount = (float)PlayerStatsData.CurrentExp / PlayerStatsData.BaseExp;

        //修改等级
        levelText.text = PlayerStatsData.CurrentLevel.ToString();
    }
}
