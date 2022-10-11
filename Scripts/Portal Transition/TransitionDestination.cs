using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 传送终点标签
/// </summary>
public enum DestinationTag
{
    None,A,B,
    ENTRY,
    EXPORT
}

/// <summary>
/// 传送终点类
/// </summary>
public class TransitionDestination : MonoBehaviour
{
    [Header("该终点标签")]
    public DestinationTag destinationTag;
}
