using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敌人状态枚举
/// </summary>
public enum EnemyStates
{
    //空状态，避免游戏开始就进入初始状态
    None,
    //登场
    Born,
    //站桩
    GUARD,
    //巡逻
    PATROL,
    //追击
    CHASE,
    //死亡
    DEAD
}

[RequireComponent(typeof(NavMeshAgent))]
/// <summary>
/// 怪兽控制类
/// </summary>
public class EnemyController : MonoBehaviour
{
    private NavMeshAgent agent;

    private Animator animator;

    //敌人状态
    private EnemyStates enemyStates;

    //攻击目标
    private GameObject attackTarget;

    //初始速度
    private float speed;

    //敌人巡逻位置
    private Vector3 wayPoint;

    //敌人初始位置
    private Vector3 GuardPoint;

    //到巡逻点后观察周围的时间
    public float lookAtTime;
    //计时
    private float remainLookAtPoint;

    [Header("敌人可视范围")]
    public float sightRadius;

    [Header("敌人是否站桩")]
    public bool isGuard;

    [Header("敌人巡逻范围")]
    public float patrolRange;

    //动画标志
    private bool firstBorn;//第一次登场
    private bool afterBorn;//登场后
    private bool isBorn;
    private bool isWalk;//巡逻时，走路
    private bool isChase;//追击
    private bool isFollow;//跟随

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        

    }

    private void Start()
    {
        speed = agent.speed;
        GuardPoint = this.transform.position;
        remainLookAtPoint = lookAtTime;

        firstBorn = true;
        isBorn = false;
        afterBorn = false;
    }

    private void Update()
    {
        //实时切换敌人状态
        SwitchStates();

        //实时切换敌人动画
        SwitchAnimation();
    }

    //绘制可视范围边线，可修改绘制对象
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(this.transform.position, sightRadius);
    }

    private void OnDestroy()
    {
        
    }

    /// <summary>
    /// 切换敌人状态
    /// </summary>
    private void SwitchStates()
    {
        //检测Player
        if (FoundPlayer())
        {
            if (firstBorn&&!afterBorn)
            {
                //初登场
                enemyStates = EnemyStates.Born;
                firstBorn = false;
            }
            else if(!firstBorn&&afterBorn)
            {
                isBorn = false;
                afterBorn = false;

                //登场后，判断敌人类型，进入初始状态
                if (isGuard)
                {
                    enemyStates = EnemyStates.GUARD;
                }
                else
                {
                    //若是巡逻类型，则需要给初始巡逻点
                    enemyStates = EnemyStates.PATROL;
                    GetNewWayPoint();
                }
            }
            else
            {
                enemyStates = EnemyStates.CHASE;
                Debug.Log("追击");
            }      
        }


        switch (enemyStates)
        {
            case EnemyStates.Born:
                //第一次发现敌人，登场
                isBorn = true;

                //显示敌人本体
                this.transform.GetChild(0).gameObject.SetActive(true);

                afterBorn = true;

                break;
            case EnemyStates.GUARD:
                Debug.Log("站桩怪");


                break;
            case EnemyStates.PATROL:
                Debug.Log("巡逻怪");

                if (!FoundPlayer())
                {
                    //停止追击
                    isChase = false;

                    //敌人巡逻时速度较慢，为原速度一半
                    agent.speed = speed * 0.4f;

                    //若自身到随机生成的巡逻点距离小于agent的停止距离，说明敌人已经移动到该巡逻点
                    if (Vector3.Distance(wayPoint, this.transform.position) <= agent.stoppingDistance)
                    {
                        //停止走路
                        isWalk = false;

                        if (remainLookAtPoint > 0)
                            remainLookAtPoint -= Time.deltaTime;

                        //重新生成巡逻点
                        GetNewWayPoint();
                    }
                    else
                    {
                        //否则移动到巡逻点
                        isWalk = true;
                        agent.destination = wayPoint;
                    }
                }
                

                break;
            case EnemyStates.CHASE:
                //TODO:追击Player
                //TODO:播放追击动画
                //TODO:在攻击范围内攻击
                //TODO:播放攻击动画
                //取消巡逻,开始追击
                isWalk = false;
                isChase = true;

                //设定：敌人巡逻时速度较慢，为原速度一半，追击时速度复原
                agent.speed = speed;
                
                if (!FoundPlayer())
                {
                    //拉脱
                    isFollow = false;
                    agent.destination = this.transform.position;

                    //回到上一个状态
                    if (isGuard)
                    {
                        enemyStates = EnemyStates.GUARD;
                    }
                    else
                    {
                        //若是巡逻类型，则需要给初始巡逻点
                        enemyStates = EnemyStates.PATROL;
                        GetNewWayPoint();
                    }
                }
                else
                {
                    //追击
                    isFollow = true;
                    agent.destination = attackTarget.transform.position;
                }
                break;
            case EnemyStates.DEAD:
                break;
        }
    }

    /// <summary>
    /// 检测Player
    /// </summary>
    /// <returns></returns>
    private bool FoundPlayer()
    {
        //获取敌人可视范围内的碰撞体
        var colliders = Physics.OverlapSphere(this.transform.position,sightRadius);

        //检索并查找是否有Player
        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                //将攻击目标设置为Player
                attackTarget = target.gameObject;
                return true;
            }
                
        }

        //未找到
        attackTarget = null;
        return false;
    }

    /// <summary>
    /// 切换敌人动画
    /// </summary>
    private void SwitchAnimation()
    {
        animator.SetBool("Born", isBorn);
        animator.SetBool("Walk",isWalk);
        animator.SetBool("Chase",isChase);
        animator.SetBool("Follow",isFollow);
    }

    //随机生成巡逻点
    private void GetNewWayPoint()
    {
        //重置观察时间
        remainLookAtPoint = lookAtTime;

        float randomX = Random.Range(-patrolRange,patrolRange);
        float randomZ = Random.Range(-patrolRange,patrolRange);

        Vector3 randomPoint = new Vector3(GuardPoint.x + randomX,
            this.transform.position.y,
            GuardPoint.z + randomZ);

        //解决随机生成的点是agent不可移动到的点的问题
        //SamplePosition返回bool值，检测该点是否可移动
        //若不可移动则将巡逻点赋值为当前坐标，否则赋值为hit.position,即randomPoint
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1)? hit.position:this.transform.position;
    }
}
