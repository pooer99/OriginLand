using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 物品图标按钮，点击可显示信息并选中
/// </summary>
public class ItemButton : MonoBehaviour
{
    private Button button;

    //Slot的父对象
    private Transform slotParentTransform;

    //复制点击按钮的一瞬间多次执行点击事件
    private bool canPress;
    private float waitTime;
    private float timer;

    private void Awake()
    {
        button = GetComponent<Button>();
        slotParentTransform = this.transform.parent.parent;

        canPress = true;
        waitTime = 2.0f;
        timer = waitTime;
    }

    private void Update()
    {
        button.onClick.AddListener(SelectItemSlot);

        //注意：由于打开背包的时候暂停了游戏，导致Time.deltaTime = 0,所以采用固定时间
        if (!canPress)
            timer -= 0.02f;

        if (timer <= 0)
        {
            canPress = true;
            timer = waitTime;
        }
        
    }



    /// <summary>
    /// 按钮点击事件，选中按钮，并显示信息
    /// </summary>
    private void SelectItemSlot()
    {
        if (canPress)
        {
            canPress = false;
            UIManager.Instance.SelectItem(FindPosition());
        }
        
    }

    /// <summary>
    /// 检测在列表中的位置
    /// </summary>
    /// <returns>位置</returns>
    private int FindPosition()
    {
        for (int i = 0; i < slotParentTransform.childCount; i++)
        {
            //寻找对应的slot的名称
            if(slotParentTransform.GetChild(i).name == this.transform.parent.name)
            {
                return i;
            }
        }
        

        return 0;
    }

}
