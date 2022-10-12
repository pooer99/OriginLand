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
public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private NavMeshAgent agent;

    private Animator animator;

    private CharacterStats enemyStatsData;

    private bool playerIsDead;

    //敌人状态
    private EnemyStates enemyStates;

    //攻击目标
    protected GameObject attackTarget;

    //碰撞体
    private Collider enemyCollider;

    //初始速度
    private float speed;

    //敌人巡逻位置
    private Vector3 wayPoint;

    //敌人初始位置
    private Vector3 guardPoint;

    //敌人初始角度
    private Quaternion guardRotation;

    //到巡逻点后观察周围的时间
    public float lookAtTime;
    //计时
    private float remainLookAtPoint;

    //攻击的CD
    private float attackTime;

    //技能CD
    private float skillTime;

    [Header("敌人可视范围")]
    public float sightRadius;

    [Header("敌人是否站桩")]
    public bool isGuard;

    [Header("敌人巡逻范围")]
    public float patrolRange;

    //控制每次只会生成一次掉落物
    private bool canCreateItems=true;

    //动画标志
    private bool firstBorn;//第一次登场
    private bool afterBorn;//登场后
    private bool isBorn;
    private bool isWalk;//巡逻时，走路
    private bool isChase;//追击
    private bool isFollow;//跟随
    private bool isDead;//死亡

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyStatsData = GetComponent<CharacterStats>();
        enemyCollider = GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        //添加游戏结束广播订阅
        GameManager.Instance.AddObserver(this);

        speed = agent.speed;
        guardPoint = this.transform.position;
        guardRotation = this.transform.rotation;
        remainLookAtPoint = lookAtTime;

        firstBorn = true;
        isBorn = false;
        afterBorn = false;

        playerIsDead = false;
 
    }

    private void Update()
    {
        //敌人死亡
        if (enemyStatsData.CurrentHealth == 0)
            isDead = true;

        if (!playerIsDead)
        {
            //实时切换敌人状态
            SwitchStates();

            //实时切换敌人动画
            SwitchAnimation();

            //计时攻击时间,由于任何时期都应该计算攻击的CD，所以应Update中计时
            attackTime -= Time.deltaTime;

            skillTime -= Time.deltaTime;
        }
        
    }

    //绘制可视范围边线，可修改绘制对象
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(this.transform.position, sightRadius);
    }

    private void OnDisable()
    {
        //若GameManager没有生成，则return，防止编辑器报错
        if (!GameManager.isInitialized)
            return;
        //取消游戏结束广播订阅
        GameManager.Instance.RemoveObserver(this);
    }

    private void OnDestroy()
    {
        
    }

    /// <summary>
    /// 切换敌人状态
    /// </summary>
    private void SwitchStates()
    {
        if (isDead)
        {
            enemyStates = EnemyStates.DEAD;
        }
        //检测Player
        else if (FoundPlayer())
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
                //Debug.Log("站桩怪");
                //停止追击
                isChase = false;

                //回到原点
                if(this.transform.position != guardPoint)
                {
                    isWalk = true;
                    agent.isStopped = false;

                    agent.destination = guardPoint;

                    //判断是否回到
                    //可以使用Distance计算差值,但是有时性能开销比下面的方法大
                    //也可以使用 SqrMagnitude:返回这个向量的长度的平方
                    if (Vector3.SqrMagnitude(guardPoint - transform.position) <= agent.stoppingDistance)
                    {
                        //立即停止走路
                        isWalk = false;

                        //重置回初始角度,平滑旋转
                        transform.rotation = Quaternion.Lerp(transform.rotation,guardRotation,0.008f);
                    }
                }

                break;
            case EnemyStates.PATROL:
                //Debug.Log("巡逻怪");

                if (!FoundPlayer())
                {
                    //停止追击
                    isChase = false;

                    //敌人巡逻时速度较慢，为原速度一半
                    agent.speed = speed * 0.5f;

                    //若自身到随机生成的巡逻点距离小于agent的停止距离，说明敌人已经移动到该巡逻点
                    if (Vector3.Distance(wayPoint, this.transform.position) <= agent.stoppingDistance)
                    {
                        //停止走路
                        isWalk = false;

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

                //取消巡逻,开始追击
                isWalk = false;
                isChase = true;

                //设定：敌人巡逻时速度较慢，为原速度一半，追击时速度复原
                agent.speed = speed;
                
                if (!FoundPlayer())
                {
                    //拉脱
                    isFollow = false;

                    //停下来观察
                    if (remainLookAtPoint > 0)
                    {
                        agent.destination = this.transform.position;
                        remainLookAtPoint -= Time.deltaTime;
                    }
                    else if (isGuard) //回到上一个状态
                    {
                        enemyStates = EnemyStates.GUARD;
                    }
                    else
                    {
                        enemyStates = EnemyStates.PATROL;
                    }
                }
                else
                {
                    //追击
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }

                //检测目标是否在攻击范围
                if (TargetInAttackRange()||TargetInSkillRange())
                {
                    //停止追击，开始攻击
                    isFollow = false;
                    agent.isStopped = true;

                    //CD结束，开始攻击
                    if(attackTime < 0)
                    {
                        //重置攻击CD
                        attackTime = enemyStatsData.attackData.coolDown;

                        //执行攻击
                        Attack();
                    }
 
                }

                break;
            case EnemyStates.DEAD:
                //关闭碰撞体，防止Player检测，报错
                enemyCollider.enabled = false;

                //关闭agent
                agent.enabled = false;

                //生成掉落物品
                if (canCreateItems)
                {
                    canCreateItems = false;
                    UIManager.Instance.InstantiateItems(this.transform);
                }
                

                Destroy(this.gameObject,2f);

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
        animator.SetBool("Death",isDead);
    }

    //随机生成巡逻点
    private void GetNewWayPoint()
    {
        //重置观察时间
        remainLookAtPoint = lookAtTime;

        float randomX = Random.Range(-patrolRange,patrolRange);
        float randomZ = Random.Range(-patrolRange,patrolRange);

        Vector3 randomPoint = new Vector3(guardPoint.x + randomX,
            this.transform.position.y,
            guardPoint.z + randomZ);

        //解决随机生成的点是agent不可移动到的点的问题
        //SamplePosition返回bool值，检测该点是否可移动
        //若不可移动则将巡逻点赋值为当前坐标，否则赋值为hit.position,即randomPoint
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1)? hit.position:this.transform.position;
    }

    /// <summary>
    /// 判断攻击目标是否在普通攻击距离范围内
    /// </summary>
    private bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, this.transform.position) <= enemyStatsData.AttackRange;
        return false;
    }

    /// <summary>
    /// 判断攻击目标是否在技能攻击距离范围内
    /// </summary>
    private bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, this.transform.position) <= enemyStatsData.SkillRange;
        return false;
    }

    /// <summary>
    /// 攻击
    /// </summary>
    private void Attack()
    {
        //暴击判断
        enemyStatsData.isCritical = Random.value < enemyStatsData.CriticalChance;
        

        //看向攻击目标
        this.transform.LookAt(attackTarget.transform);

        //技能或远程攻击:需要暴击和CD才能发动
        if (TargetInSkillRange()&&skillTime<0&&enemyStatsData.isCritical)
        {
            //播放远程攻击动画
            animator.SetBool("Critical", enemyStatsData.isCritical);
            animator.SetTrigger("Skill");

            //重置技能CD
            attackTime = enemyStatsData.attackData.skillCoolDown;

        }

        //普通或近身攻击
        if (TargetInAttackRange())
        {
            //播放近身攻击动画
            animator.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// Animation event：攻击
    /// </summary>
    public void Hit()
    {
        //Player在范围内使
        if(attackTarget != null)
        {
            //若Player的距离小于敌人的普攻范围，则造成伤害，否则算作没击中
            //todo:根据不同的敌人，可能需要修改判定是否造成伤害的方法，有些敌人的技能范围和普攻范围略有区别
            if (Vector3.Distance(attackTarget.transform.position , transform.position)<=enemyStatsData.AttackRange)
            {
                CharacterStats targetStats = attackTarget.GetComponent<CharacterStats>();

                targetStats.TakeDamage(enemyStatsData, targetStats);
            }
            
        }
        
    }

    public void EndNotify()
    {
        //播放获胜动画
        //停止所有行为
        //停止agent
        Debug.Log("玩家死亡！");

        animator.SetBool("Win", true);

        playerIsDead = true;

        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
