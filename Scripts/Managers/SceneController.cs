using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Player;
using UnityEngine.UI;


/// <summary>
/// 场景传送核心控制类
/// </summary>
public class SceneController : SingleTon<SceneController>
{
    [Header("过渡场景")]
    public GameObject loadProgressPrefab;

    private GameObject loadProgress;

    //进度条图片
    private Image progressImage;

    //进度条文字
    private Text progressText;

    private Transform playerTransform;

    //用于获取异步加载信息，制作进度条
    private AsyncOperation operation;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void OnDestroy()
    {
        Debug.Log("传送单例销毁");
        //SceneController.Instance.Clear();
    }

    /// <summary>
    /// 功能：传送到终点
    /// </summary>
    /// <param name="point">传送起点</param>
    public void TransitionToDestination(TransitionStart startPoint)
    {
        switch (startPoint.transitionType)
        {
            case TransitionType.SameScene:
                //同场景
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, startPoint.destinationTag));
                
                break;
            case TransitionType.DifferentScene:
                //异场景
                StartCoroutine(Transition(startPoint.sceneName,startPoint.destinationTag));
                break;
        }

    }

    /// <summary>
    /// 加载场景协程
    /// </summary>
    /// <param name="name">场景名字</param>
    /// <param name="tag">传送终点标签</param>
    /// <returns></returns>
    IEnumerator Transition(string name,DestinationTag tag)
    {
        //todo:保存数据，存档
        //存档
        SaveManager.Instance.SavePlayerData();


        //名字不相同，异场景传送
        if (SceneManager.GetActiveScene().name != name)
        {
            //播放过渡场景
            yield return loadProgress = Instantiate(loadProgressPrefab);

            yield return progressImage = loadProgress.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Image>();

            yield return progressText = loadProgress.transform.GetChild(1).GetChild(0).GetChild(1).GetComponent<Text>();

            //等待异步加载场景，加载完，才继续
            operation = SceneManager.LoadSceneAsync(name);

            //不允许场景自动跳转
            operation.allowSceneActivation = false;

            //场景加载没有完成时
            while (!operation.isDone)
            {
                //滑动条 = 场景加载进度
                progressImage.fillAmount = operation.progress;
                progressText.text = operation.progress * 100 + "%";

                //只能显示到0.9，之后的0.1需要手动添加
                if (operation.progress >= 0.9F)
                {
                    //速度太快了,做个延时
                    yield return new WaitForSeconds(2f);

                    progressImage.fillAmount = 1.0f;
                    progressText.text = "100%";

                    //允许场景自动跳转
                    operation.allowSceneActivation = true;
                }

                //跳出协程
                yield return null;
            }

            //做延时，待人物出现在新场景在读档
            yield return new WaitForSeconds(0.2f);

            //获取Player的transform
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            //修改Player位置和旋转
            playerTransform.SetPositionAndRotation(GetDestination(tag).transform.position,
                GetDestination(tag).transform.rotation);

            //读档
            SaveManager.Instance.LoadPlayerData();

            yield break;
        }
        //相同，同场景传送
        else
        {
            //获取Player的transform
            playerTransform = GameManager.Instance.playerStats.gameObject.transform;

            //修改Player位置和旋转
            playerTransform.SetPositionAndRotation(GetDestination(tag).transform.position,
                GetDestination(tag).transform.rotation);

            yield return null;
        }
        
    }

    /// <summary>
    /// 根据Tag获取终点
    /// </summary>
    /// <param name="tag">终点标签</param>
    /// <returns></returns>
    private TransitionDestination GetDestination(DestinationTag tag)
    {
        //获取场景中所有终点
        var destinations = FindObjectsOfType<TransitionDestination>();

        //搜索指定Tag的终点
        foreach (var des in destinations)
        {
            if (des.destinationTag == tag)
                return des;
        }

        return null;
    }
}
