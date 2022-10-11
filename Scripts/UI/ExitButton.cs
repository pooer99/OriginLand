using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 登录界面退出按键类
/// </summary>
public class ExitButton : MonoBehaviour
{
    private Button button;

    private bool canExit = true;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Update()
    {
        button.onClick.AddListener(ExitGame);
    }
    private void ExitGame()
    {
        if (canExit)
        {
            canExit = false;
            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            Application.Quit();
        }
        
    }
}