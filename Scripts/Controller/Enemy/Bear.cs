using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Boss熊控制类
/// </summary>
public class Bear : EnemyController
{
    [Header("击飞力度")][Header("技能")]
    public int kickForce;

    private Vector3 direction;

    /// <summary>
    /// 击飞
    /// </summary>
    public void KickOff()
    {
        if (attackTarget != null)
        {
            transform.LookAt(attackTarget.transform);

            direction = (attackTarget.transform.position - transform.position).normalized;

            Debug.Log("击退: "+direction);

            //播放Player伤害动画
            attackTarget.GetComponent<Animator>().SetTrigger("Hurt");
            //todo:bug
            //attackTarget.GetComponent<Rigidbody>().AddForce(direction*kickForce,ForceMode.Impulse);

        }
    }
}
