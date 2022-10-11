using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ���ֹ��������
/// </summary>
public class SoundManager : SingleTon<SoundManager>
{

    //��Դ
    private AudioSource sound;

    [Header("����Ƭ����")]
    public List<AudioClip> clips;

    //�Ƿ�ѭ��
    [HideInInspector]
    public bool isCycle;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        sound = GetComponent<AudioSource>();
        isCycle = false;
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="num">���</param>
    public void PlayeMusic(int num)
    {
        sound.loop = isCycle;
        sound.clip = clips[num];
        sound.Play();
    }

    /// <summary>
    /// ���Ŷ���Ч����ťʲô��
    /// </summary>
    /// <param name="num"></param>
    public void PlayOnShot(int num)
    {
        sound.PlayOneShot(clips[num]);
    }

    /// <summary>
    /// ��ͣ����
    /// </summary>
    /// <param name="num">���</param>
    public void PasueMusic(int num)
    {
        sound.clip = clips[num];
        sound.Pause();
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="num">���</param>
    public void StopMusic(int num)
    {
        sound.clip = clips[num];
        sound.Stop();
    }

}
