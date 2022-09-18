using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 
/// </summary>
public class SoundManager : SingleTon<SoundManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnDestroy()
    {
        base.Clear();
    }
}
