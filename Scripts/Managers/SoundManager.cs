using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// “Ù¿÷π‹¿Ì∫À–ƒ¿‡
/// </summary>
public class SoundManager : SingleTon<SoundManager>
{

    //“Ù‘¥
    private AudioSource sound;

    [Header("“Ù¿÷∆¨∂Œ◊È")]
    public List<AudioClip> clips;

    // «∑Ò—≠ª∑
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
    /// ≤•∑≈“Ù¿÷
    /// </summary>
    /// <param name="num">–Ú∫≈</param>
    public void PlayeMusic(int num)
    {
        sound.loop = isCycle;
        sound.clip = clips[num];
        sound.Play();
    }

    /// <summary>
    /// ≤•∑≈∂Ã“Ù–ß£¨∞¥≈• ≤√¥µƒ
    /// </summary>
    /// <param name="num"></param>
    public void PlayOnShot(int num)
    {
        sound.PlayOneShot(clips[num]);
    }

    /// <summary>
    /// ‘›Õ£“Ù¿÷
    /// </summary>
    /// <param name="num">–Ú∫≈</param>
    public void PasueMusic(int num)
    {
        sound.clip = clips[num];
        sound.Pause();
    }

    /// <summary>
    /// Ω· ¯“Ù¿÷
    /// </summary>
    /// <param name="num">–Ú∫≈</param>
    public void StopMusic(int num)
    {
        sound.clip = clips[num];
        sound.Stop();
    }

}
