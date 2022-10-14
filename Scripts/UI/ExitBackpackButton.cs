using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 退出背包按钮类
/// </summary>
public class ExitBackpackButton : MonoBehaviour
{
    private Button button;

    private bool canPress;
    private float waitTime = 1f;
    private float timer;

    private void Awake()
    {
        button = GetComponent<Button>();

        canPress = true;

        timer = waitTime;
    }

    private void Update()
    {
        button.onClick.AddListener(Exit);

        if (!canPress)
            timer -= 0.02f;

        if (timer <= 0)
        {
            canPress = true;
            timer = waitTime;
        }
    }

    private void Exit()
    {
        if (canPress)
        {
            canPress = false;

            //如果游戏暂停中，按下退出键，游戏继续
            if (GameManager.Instance.isPaused)
            {
                UIManager.Instance.Resume();
            }
        }
    }
}
