using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 摄像机控制类
/// </summary>
public class CameraController : MonoBehaviour
{
    private float yaw;
    private float pitch;

    [Header("鼠标移动速度")]
    public float mouseMoveSpeed = 2;

    [Header("相机距离")]
    public float cameraDir = 3;

    public Transform playerTransfrom;

    private void LateUpdate()
    {
        CameraMove();
    }

    private void CameraMove()
    {
        yaw += Input.GetAxis("Mouse X");
        pitch -= Input.GetAxis("Mouse Y");
        this.transform.eulerAngles = new Vector3(pitch, yaw, 0);

        this.transform.position = playerTransfrom.position - transform.forward * cameraDir;
    }
}
