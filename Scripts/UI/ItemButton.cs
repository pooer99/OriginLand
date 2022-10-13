using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// ��Ʒͼ�갴ť���������ʾ��Ϣ��ѡ��
/// </summary>
public class ItemButton : MonoBehaviour
{
    private Button button;

    //Slot�ĸ�����
    private Transform slotParentTransform;

    //���Ƶ����ť��һ˲����ִ�е���¼�
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

        //ע�⣺���ڴ򿪱�����ʱ����ͣ����Ϸ������Time.deltaTime = 0,���Բ��ù̶�ʱ��
        if (!canPress)
            timer -= 0.02f;

        if (timer <= 0)
        {
            canPress = true;
            timer = waitTime;
        }
        
    }



    /// <summary>
    /// ��ť����¼���ѡ�а�ť������ʾ��Ϣ
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
    /// ������б��е�λ��
    /// </summary>
    /// <returns>λ��</returns>
    private int FindPosition()
    {
        for (int i = 0; i < slotParentTransform.childCount; i++)
        {
            //Ѱ�Ҷ�Ӧ��slot������
            if(slotParentTransform.GetChild(i).name == this.transform.parent.name)
            {
                return i;
            }
        }
        

        return 0;
    }

}
