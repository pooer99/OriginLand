using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 传送类型枚举
/// </summary>
public enum TransitionType
{
    SameScene,
    DifferentScene
}

/// <summary>
/// 传送起点类
/// </summary>
public class TransitionStart : MonoBehaviour
{
    [Header("传送类型")] [Header("传送信息")]
    public TransitionType transitionType;

    [Header("终点场景名称")]
    public string sceneName;

    [Header("传送终点标签")]
    public DestinationTag destinationTag;

    //能否传送
    private bool canTrans;

    private void Update()
    {
        //按E传送
        if (Input.GetKeyDown(KeyCode.E) && canTrans)
        {
            //todo:SceneController 传送
            Debug.Log(SceneController.isInitialized);

            if(SceneController.Instance != null)
                SceneController.Instance.TransitionToDestination(this);
        }
    }

    // 在触发器中时
    private void OnTriggerStay(Collider other)
    {
        Debug.Log("传送？");
        if (other.CompareTag("Player"))
            canTrans = true;
    }

    //离开触发器时
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            canTrans = false;
    }
}
