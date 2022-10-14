using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 丢弃物品按钮类
/// </summary>
public class RemoveItemButton : MonoBehaviour
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
        button.onClick.AddListener(RemoveItem);

        if (!canPress)
            timer -= 0.02f;

        if (timer <= 0)
        {
            canPress = true;
            timer = waitTime;
        }
    }

    private void RemoveItem()
    {
        if (canPress)
        {
            canPress = false;

            UIManager.Instance.RemoveItems();
        }
    }
}
