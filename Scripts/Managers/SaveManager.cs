using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// 存储方式枚举
/// </summary>
public enum SaveType
{
    PlayerPrefs,
    Json
}

/// <summary>
/// 读、存档管理核心类
/// </summary>
public class SaveManager : SingleTon<SaveManager>
{
    [Header("存储形式")]
    public SaveType saveType;

    [Header("存储名称")]
    public string saveName;

    private string savePath;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        savePath = Application.dataPath + "/SaveData/Json/" + saveName + ".json";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SavePlayerData();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPlayerData();
        }
    }

    /// <summary>
    /// 存档
    /// </summary>
    /// <param name="data">存储数据</param>
    /// <param name="key">存储键值</param>
    /// <param name="type">存储方式</param>
    private void Save(Object data,string key ,SaveType type)
    {
        //将数据（这个数据可以是class、SO等）转换为字符串类型的json
        var jsonData = JsonUtility.ToJson(data,true);

        //选择存储方式
        switch (type)
        {
            case SaveType.PlayerPrefs:
                PlayerPrefs.SetString(key,jsonData);
                PlayerPrefs.Save();
                break;
            case SaveType.Json:
                try
                {
                    Debug.Log("存档！");
                    File.WriteAllText(savePath,jsonData);
                }
                catch (System.Exception e)
                {

                    Debug.Log(e.ToString());
                }
                break;
        }
    }

    /// <summary>
    /// 读档
    /// </summary>
    /// <param name="data">读取数据</param>
    /// <param name="key">读取键值</param>
    /// <param name="type">读取方式</param>
    private void Load(Object data,string key, SaveType type)
    {
        switch (type)
        {
            case SaveType.PlayerPrefs:

                //若存在key
                if (PlayerPrefs.HasKey(key))
                {
                    //解析json转换为原来的数据（class、SO等）
                    JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key),data);
                }

                break;
            case SaveType.Json:
                try
                {
                    Debug.Log("读档！");
                    string jsonData = File.ReadAllText(savePath);
                    JsonUtility.FromJsonOverwrite(jsonData,data);
                }
                catch (System.Exception e)
                {

                    Debug.Log(e.ToString());
                }
                break;
        }
    }

    public void SavePlayerData()
    {
        Debug.Log("存档！");
        Save(GameManager.Instance.playerStats.characterData,"PlayerData",saveType);
    }

    public void LoadPlayerData()
    {
        Debug.Log("读档！");
        Load(GameManager.Instance.playerStats.characterData, "PlayerData", saveType);
    }
}
