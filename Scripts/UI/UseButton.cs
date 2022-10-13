using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 使用物品按钮
/// </summary>
public class UseButton : MonoBehaviour
{
    private Button button;

    private float waitTime = 1f;
    private float timer;
    private bool canPress;

    private void Awake()
    {
        button = GetComponent<Button>();

        timer = waitTime;
        canPress = true;
    }

    private void Update()
    {
        button.onClick.AddListener(UseItems);

        if (!canPress)
            timer -= 0.02f;

        if (timer <= 0)
        {
            canPress = true;
            timer = waitTime;
        }
    }

    private void UseItems()
    {
        if (canPress)
        {
            canPress = false;
            UIManager.Instance.UseItems();
        }
        
    }
}
