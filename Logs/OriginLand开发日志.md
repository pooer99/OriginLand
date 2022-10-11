# OriginLand

## 安装第三方插件

### 1.3D项目升级URP渲染线

下载URP并安装

在项目新建一个文件夹命名为“PipLine”

在该文件夹下创建一个URP Asset文件

打开Edit-->Project Settings

在Graphics-->Scriptable Render Pipline Settings中选择创建的URP Asset文件

在Quality-->Rendering中选择创建的URP Asset文件

### 2.安装XLua

#### 下载

打开https://github.com/Tencent/xLua，下载zip文件并解压

将Assets文件夹中的文件拖入Unity中

#### 实现XLua环境管理单例类

```c#
using System.IO;
using UnityEngine;
using XLua;

/// <summary>
/// XLua加载环境管理类---单例模式
/// </summary>
public class XLuaEnv
{
    //唯一的Lua加载环境
    private LuaEnv env;

    private static XLuaEnv instance = null;

    //属性
    public static XLuaEnv Instance
    {
        get
        {
            if (instance == null)
                instance = new XLuaEnv();
            return instance;
        }
    }


    //自定义加载的目录
    private string path;

    /// <summary>
    /// 返回Lua环境的全局变量
    /// </summary>
    public LuaTable Global
    {
        get
        {
            return env.Global;
        }
    }

    private XLuaEnv()
    {

        Debug.Log("创建--LuaEnv");
        env = new LuaEnv();

        path = InitPath();

        //加载自定义加载器
        env.AddLoader(Custom_Loader);
    }

    /// <summary>
    /// Lua加载Lua文件---自定义加载器
    /// 自定义加载器，会系统内置加载器，加载到文件后，后续加载器将不会继续执行加载
    /// 当Lua代码执行require函数时，自定义加载器会尝试获取文件的内容
    /// 若文件不存在，应该返回null
    /// </summary>
    private byte[] Custom_Loader(ref string filepath)
    {
        //filepath来着Luad的require('文件名')
        //因此需要构造路径，使require加载的文件指向我们想放Lua的路径内的文件
        //因为Application.dataPath在上线的代码中无法获取,所以上线时，需要将lua的存储路径指向Application.persistentDataPath

        path = path + filepath + ".lua";

        //将Lua文件读取为字节数组
        //XLua的解析环境，会执行我们自定义加载器返回的Lua代码
        if (File.Exists(path))
            return File.ReadAllBytes(path);

        return null;
    }

    /// <summary>
    /// 初始化Lua文件加载路径
    /// </summary>
    /// <returns></returns>
    private string InitPath()
    {
        string path = Application.dataPath;

        path += "/LuaScripts/src/";

        return path;
    }

    /// <summary>
    /// 执行代码函数
    /// </summary>
    /// <param name="code">代码</param>
    public object[] DoString(string code)
    {
        return env.DoString(code);
    }

    /// <summary>
    /// 释放Lua环境
    /// </summary>
    public void FreeEnv()
    {
        //销毁加载环境
        env.Dispose();

        instance = null;

        Debug.Log("销毁--LuaEnv");
    }
}

```

#### 实现Lua引导核心Table

新增lua脚本只需要添加进BootSrtap表中，每次使用只需要在C#中加载BootSrtap文件即可

```lua
---
--- Created by Pooer.
--- DateTime: 2022-09-15 16:32
---
--- Lua核心向导Table

package.path = package.path..";D:\\Unity Project\\OriginLand\\Assets\\LuaScripts\\src\\?.lua;"
------------------定义----------------------------
BootStrap = {}

--- 控制器核心Table
BootStrap["Controllers/PlayerController"] = require("PlayerController_Lua")

------------------生命周期-------------------------
```



#### 实现Lua"生命周期"函数

由于Lua没有生命周期，所以需要使用一个类或者结构体来接收需要在Unity生命周期中运行的函数，在C#中使用委托来定义类或者结构体中的函数，因此在释放XLua环境的时候需要对其赋值为Null

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public delegate void LifeCycle();
//单参数
public delegate void LifeCycleOneP(Collision collider);

/// <summary>
/// Lua生命周期向导结构体，用于接收Lua的表
/// </summary>
[GCOptimize]
[CSharpCallLua]
public struct BootStrapStruct
{
    public LifeCycle Awake;
    public LifeCycle Start;
    public LifeCycle FixedUpDate;
    public LifeCycleOneP OnCollisionEnter;
    public LifeCycle UpDate;
    public LifeCycle LateUpDate;
    public LifeCycle OnDestroy;

    /// <summary>
    /// 清除委托方法
    /// </summary>
    public void Clear()
    {
        this.Awake = null;
        this.Start = null;
        this.FixedUpDate = null;
        this.OnCollisionEnter = null;
        this.UpDate = null;
        this.LateUpDate = null;
        this.OnDestroy = null;
    }
}

```



## 一、创建地形

### 安装Terrain Tool

由于该插件是Preview Packages

所以需要在**Edit-->Project Settings-->Package Manager**中勾选**Enable Preview Packages**才能在Package Manager中搜索到

### 创建Terrain

点击**Window-->Terrain-->Terrain Toolbox**

修改参数创建适合的Terrain

勾选Gizmo可查看地形的范围（**一个蓝色的立方体**），若创建的地形在这个最边上则会导致地形无法上升或下降需要调整**Terrain Toolbox**中的**Start Position**

### 关于Terrain组件

笔刷模块可以实现地形升降、修改纹理等功能

树和草模块可以实现批量生成植物，若想改变植物在摄像机拉远后消失的距离可以在设置中修改**Tree Distance**和**Detail Distance**

## 二、Player

- [ ] 在这里Player的移动采用transfrom.translate，由于该移动方式属于强制位移，所以在透过墙体时，即使墙体中启动了Mesh Collider的Canvex（凸体），仍能强制透过墙体，后期需要改进

### C#代码

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Player
{
    [LuaCallCSharp]
    /// <summary>
    /// 玩家控制类
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        //Lua向导类
        private BootStrapStruct core;

        //摄像机变换组件
        public Transform cameraTransfrom;

        [HideInInspector]
        public float currentVelocity;

        [Header("Player移动速度")]
        public float Speed;

        [Header("Player转动速度")]
        public float smoothTime = 1f;

        [Header("Player起跳力度")]
        public float jumpGravity = 500f;

        private void Awake()
        {
            //加载Lua脚本
            XLuaEnv.Instance.DoString("require 'BootStrap'");

            //设置Lua中的self为c#的this
            XLuaEnv.Instance.Global.Set("self", this);

            //获取Lua中PlayerController表
            LuaTable table = XLuaEnv.Instance.Global.Get<LuaTable>("BootStrap");
            core = table.Get<BootStrapStruct>("Controllers/PlayerController");

            if (core.Awake != null)
                core.Awake();
        }

        private void Start()
        {

            if (core.Start != null)
                core.Start();

        }

        private void OnCollisionEnter(Collision collision)
        {
            if (core.OnCollisionEnter != null)
                core.OnCollisionEnter(collision);
        }

        private void Update()
        {
            if (core.UpDate != null)
                core.UpDate();

        }

        private void OnDestroy()
        {
            //清空委托，防止报错:try to dispose a LuaEnv with C# callback!
            core.Clear();

            //销毁XLua环境
            XLuaEnv.Instance.FreeEnv();
        }

    }
}
```



### Lua代码

```lua
---
--- Created by Pooer.
--- DateTime: 2022-09-09 14:28
---
------------------定义----------------------------
local PlayerController = {}

local animator
local rb

-- 允许跳跃标志
local canJump = false
-- 起跳
local isJump = false
-- 是否在降落
local isFall = false
-- 武器显示标志
local isShow = false

-- 攻击模式
local attackMode = {}
attackMode.Attack1 = 1
attackMode.Attack2 = 2
-- 当前攻击方式
attackMode.nowMode = 1

----------------生命周期--------------------------
PlayerController.Awake = function()

    -- 获取动画组件
    animator =  self:GetComponent(typeof(CS.UnityEngine.Animator))

    -- 获取刚体组件
    rb = self:GetComponent(typeof(CS.UnityEngine.Rigidbody))

end

PlayerController.Start = function()
    -- 隐藏光标
    CU.Cursor.lockState = CU.CursorLockMode.Locked
end

PlayerController.OnCollisionEnter = function(collision)
    -- 若Player与地面发生碰撞
    if (collision.gameObject.tag == "Ground")
    then
        -- 允许跳跃标志
        canJump = true
        -- 播放起跳动画
        animator:SetBool("CanJump",canJump);

        isJump = false
        animator:SetBool("IsJump",isJump)

        -- 是否在降落
        isFall = false
        animator:SetBool("IsFall",isFall)

    end

end

-- Player移动函数
local PlayerMove = function()

    --- 1.获取键盘输入，并将其进行标准化，向量方向保持不变，但其长度为 1.0
    --- Input.GetAxisRaw：返回一个不使用平滑滤波器的虚拟轴值
    --- 平滑[-1,1] 不平滑{-1,0,1}
    local inputDir = Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized

    --- 2.获取Player的旋转角度：旋转角度 = 根据键盘输入的值，以x轴开始的角度 + 相机绕y轴旋转的角度
    --- Mathf.Atan2：返回值是在x轴和一个二维向量开始于0个结束在(x,y)处之间的角
    --- Mathf.Rad2Deg 弧度转换为度
    --- eulerAngles.y 欧拉角，绕y轴旋转y度，返回绝对值
    local targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + self.cameraTransfrom.eulerAngles.y

    --- 若有移动
    if (inputDir ~= Vector2.zero)
    then
        -- 获取平滑阻尼角度
        local DampAngle
        --- 3.平滑缓冲，东西不是僵硬的移动而是做减速缓冲运动到指定位置
        --- 平滑阻尼角度：Mathf.SmoothDampAngle(当前位置，目标位置，当前速度，到达目标的平滑时间)
        DampAngle,self.currentVelocity = Mathf.SmoothDampAngle(self.transform.eulerAngles.y, targetRotation, self.currentVelocity, self.smoothTime)
        --- 4.实现Player原地绕y轴转动某一角度
        self.transform.eulerAngles = Vector3.up * DampAngle

        --- 实现Player移动时，受碰撞影响：采用刚体移动方式
        --- 实际上只使用Translate，也可，只是没有碰撞力的效果
        --- 调用Rigidbody.MovePosition使物体在任意两帧之间平滑过渡，而不是瞬移（Rigidbody.position）
        rb:MovePosition(self.transform.position + self.transform.forward * CU.Time.deltaTime * self.Speed)
        --- 5.相对于世界坐标移动
        self.transform:Translate(self.transform.forward * self.Speed * CU.Time.deltaTime, CU.Space.World)

        animator:SetBool("Move", true)
    else
        animator:SetBool("Move", false)
    end
end

-- Player跳跃函数
local PlayerJump = function()
    isFall = true
    animator:SetBool("IsFall",isFall)

    isJump = true
    animator:SetBool("IsJump",isJump)
    --- 添加向上的速度使刚体跳跃
    rb.velocity = Vector3(rb.velocity.x,self.jumpGravity*CU.Time.deltaTime,rb.velocity.z)
end

-- PlayerAttack函数
local PlayerAttack = function()
    -- 切换攻击方式
    if(attackMode.nowMode == attackMode.Attack1)
    then
        animator:SetTrigger("Attack1")
        attackMode.nowMode = attackMode.nowMode + 1
    elseif (attackMode.nowMode == attackMode.Attack2)
    then
        animator:SetTrigger("Attack2")
        attackMode.nowMode = 1
    end

end

PlayerController.UpDate = function()
    --- Player攻击
    if(Input.GetKeyDown(CU.KeyCode.Mouse0))
    then
        PlayerAttack()
    end

    --- Player移动
    PlayerMove()

    --- Player跳跃
    if (Input.GetKeyDown(CU.KeyCode.Space) and canJump == true)
    then
        PlayerJump()

        canJump = false
        animator:SetBool("CanJump",canJump);
    end

    -- 显示武器
    if(Input.GetKeyDown(CU.KeyCode.Tab))
    then
        if(isShow == false)
        then
            self.weapon.transform:GetChild(0).gameObject:SetActive(true)
            isShow = true
        else
            self.weapon.transform:GetChild(0).gameObject:SetActive(false)
            isShow = false
        end
    end

    -- 唤醒光标
    if (Input.GetKey(CU.KeyCode.LeftAlt))
    then
        CU.Cursor.lockState = CU.CursorLockMode.None;
    end

end

PlayerController.OnDestroy = function()
    print("OnDestroy")
end

return PlayerController
```



## 三、相机跟随

有关摄像机的跟随的代码应该放入到生命周期--LateUpDate中执行，若在UpDate中执行则会导致人物移动卡顿

```c#
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
        yaw += Input.GetAxis("Mouse X")*mouseMoveSpeed;
        pitch -= Input.GetAxis("Mouse Y")*mouseMoveSpeed;
        this.transform.eulerAngles = new Vector3(pitch, yaw, 0);

        this.transform.position = playerTransfrom.position - transform.forward * cameraDir;
    }
}

```

## 四、Enemy

敌人核心控制器

敌人拥有登场、站桩、巡逻、追击、死亡等五种状态

初始当Player进入到敌人可视范围，敌人切换为登场状态；

登场状态后，根据敌人类型确定进入站桩状态还是巡逻状态；

站桩或巡逻时发现Player则进入追击状态，速度加倍，追击敌人;

```c#
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

```



## 五、保存数据

### Scriptable Object

使用SO进行存储角色的基础数据，如生命、防御等



#### **生成角色状态数据的SO：**

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ScriptableObject
/// 功能：生成角色数据的ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("基本设置")]
    //基础血量
    public int health;

    //当前血量
    public int currentHealth;

    //基础防御
    public int defence;

    //当前防御
    public int currentDefence;
}

```

#### 管理角色攻击数据的SO：

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ScriptableObject
/// 功能：生成角色攻击数据的ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Data", menuName = "Attack Data/Data")]
public class AttackData_SO : ScriptableObject
{
    [Header("最小伤害")]
    public int minDamage;

    [Header("最大伤害")]
    public int maxDamage;

    [Header("攻击距离")]
    public float attackRange;

    [Header("技能距离")]
    public float skillRange;

    [Header("普攻CD")]
    public float coolDown;

    [Header("技能CD")]
    public float skillCoolDown;

    [Header("暴击伤害加成百分比")]
    public float criticalMul;

    [Header("暴击率")]
    public float criticalChance;
}

```



#### 总-**管理角色数据的SO：**

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// MonoBehaviour
/// 功能：管理角色数据的ScriptableObject
/// </summary>
public class CharacterStats : MonoBehaviour
{
    //获取角色SO
    [Header("角色状态数据")]
    public CharacterData_SO characterData;

    [Header("角色攻击数据")]
    public AttackData_SO attackData;

    //是否暴击
    [HideInInspector]
    public bool isCritical;

    #region 基础数据属性

    //基础血量
    public int Health 
    {
        get
        {
            if (characterData != null)
                return characterData.health;
            return 0;
        }
        set
        {
            characterData.health = value;
        }
    }

    //当前血量
    public int CurrentHealth
    {
        get
        {
            if (characterData != null)
                return characterData.currentHealth;
            return 0;
        }
        set
        {
            characterData.currentHealth = value;
        }
    }

    //基础防御
    public int Defence
    {
        get
        {
            if (characterData != null)
                return characterData.defence;
            return 0;
        }
        set
        {
            characterData.defence = value;
        }
    }

    //当前防御
    public int CurrentDefence
    {
        get
        {
            if (characterData != null)
                return characterData.currentDefence;
            return 0;
        }
        set
        {
            characterData.currentDefence = value;
        }
    }

    #endregion

    #region 基础攻击属性

    //最小伤害
    public int MinDamage
    {
        get
        {
            if (attackData != null)
                return attackData.minDamage;
            return 0;
        }
        set
        {
            attackData.minDamage = value;
        }
    }

    //最大伤害
    public int MaxDamage
    {
        get
        {
            if (attackData != null)
                return attackData.maxDamage;
            return 0;
        }
        set
        {
            attackData.maxDamage = value;
        }
    }

    //攻击距离
    public float AttackRange
    {
        get
        {
            if (attackData != null)
                return attackData.attackRange;
            return 0;
        }
        set
        {
            attackData.attackRange = value;
        }
    }

    //技能距离
    public float SkillRange
    {
        get
        {
            if (attackData != null)
                return attackData.skillRange;
            return 0;
        }
        set
        {
            attackData.skillRange = value;
        }
    }

    //普攻CD
    public float CoolDown
    {
        get
        {
            if (attackData != null)
                return attackData.coolDown;
            return 0;
        }
        set
        {
            attackData.coolDown = value;
        }
    }

    //技能CD
    public float SkillCoolDown
    {
        get
        {
            if (attackData != null)
                return attackData.skillCoolDown;
            return 0;
        }
        set
        {
            attackData.skillCoolDown = value;
        }
    }

    //暴击伤害加成百分比
    public float CriticalMul
    {
        get
        {
            if (attackData != null)
                return attackData.criticalMul;
            return 0;
        }
        set
        {
            attackData.criticalMul = value;
        }
    }

    //暴击率
    public float CriticalChance
    {
        get
        {
            if (attackData != null)
                return attackData.criticalChance;
            return 0;
        }
        set
        {
            attackData.criticalChance = value;
        }
    }

    #endregion
}

```



## 六、GameManager

游戏需要一个全局唯一的单例管理者进行统一管理

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 游戏核心管理类
/// </summary>
public class GameManager : SingleTon<GameManager>
{
    //Player状态数据
    public CharacterStats playerStats;

    //所有订阅了游戏结束广播的敌人
    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();

    /// <summary>
    /// 广播注册Player状态
    /// </summary>
    /// <param name="stats"></param>
    public void RigisterPlayer(CharacterStats stats)
    {
        playerStats = stats;
    }

    /// <summary>
    /// 将所有订阅了广播的敌人添加到列表
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }

    /// <summary>
    /// 移除订阅了广播的敌人
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    } 

    /// <summary>
    /// 执行所以订阅的广播方法
    /// </summary>
    public void NotifyObserrvers()
    {
        foreach (var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnDestroy()
    {
        GameManager.Instance.Clear();
    }
}

```

## 七、实现观察者模式接口

在GameManager中实现

```c#
/// <summary>
/// 游戏结束观察者接口
/// 功能：广播、订阅
/// </summary>
public interface IEndGameObserver
{
    /// <summary>
    /// 游戏结束广播
    /// </summary>
    void EndNotify();
}

```

## 八、血条UI显示

血条、经验等更新UII的功能应写在LateUpdate中

### 敌人血条

在CharacterStats类中添加一个event事件，用于更新血条显示，添加相关方法

创建HealthBar类和HealthBar预制体，在场景中创建一个HealthBar Canvas，之后代码中所有生成的血条都放入其中统一管理

血条可设置是否总是可见，若未勾选则在被攻击后显示，若未攻击时间超过可见时间则自动消失

```c#
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 血条UI类
/// </summary>
public class HealthBar : MonoBehaviour
{
    //角色当前状态
    private CharacterStats currentStats;

    [Header("Canvas")]
    public GameObject healthBarCanvas;

    [Header("预制体")]
    public GameObject healthBarPrefab;

    [Header("血条位置")]
    public Transform barPoint;

    [Header("总是可见")]
    public bool alwaysVisible;

    [Header("可视化时间")]
    public float visibleTime;
    //剩余显示时间
    private float timeLeft;

    //获取预制体的血条的滑动条
    private Image healthSlider;

    //获取生成的血条的位置，之后需要将血条移到角色头顶
    private Transform UIBar;

    //主相机
    private Transform mCamera;

    private void Awake()
    {
        currentStats = GetComponent<CharacterStats>();

        //添加血条更新方法
        currentStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    private void OnEnable()
    {
        mCamera = Camera.main.transform;

        //todo:遍历场景中所有WorldSpace的canvas，并生成

        //获取生成血条的transfrom
        UIBar = Instantiate(healthBarPrefab, healthBarCanvas.transform).transform;

        //获取血条滑动条
        healthSlider = UIBar.GetChild(0).GetComponent<Image>();

        //总是可见？
        UIBar.gameObject.SetActive(alwaysVisible);

    }

    private void LateUpdate()
    {
        if (UIBar != null)
        {
            //将生成的血条移到角色头顶
            UIBar.position = barPoint.position;

            //始终面向玩家
            UIBar.forward = -mCamera.forward;

            //未勾选"总是可见"并且计时结束则不显示血条
            if (timeLeft <= 0 && !alwaysVisible)
            {
                UIBar.gameObject.SetActive(false);
            }
            else
                timeLeft -= Time.deltaTime;
        }
    }


    /// <summary>
    /// 更新血条方法
    /// </summary>
    /// <param name="currentHealth">当前生命</param>
    /// <param name="Health">初始生命</param>
    private void UpdateHealthBar(int currentHealth, int Health)
    {
        //血条小于0，不显示UI
        if (currentHealth <= 0)
            Destroy(UIBar.gameObject);
        else
        {
            //被攻击时，显示
            UIBar.gameObject.SetActive(true);

            //计时显示血条
            timeLeft = visibleTime;

            //实时计算滑动条百分比
            float sliderPercent = (float)currentHealth / Health;

            healthSlider.fillAmount = sliderPercent;
        }

        
    }
}

```



### Player血条

在场景中制作Player的UI并保存为预制体，将脚本挂载

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Player状态信息UI类
/// </summary>
public class PlayerStatsInfo : MonoBehaviour
{
    [Header("Player")]
    public GameObject player;
    private CharacterStats PlayerStatsData;

    [Header("名称")]
    public Text nameText;
    public string playerName;

    [Header("血条")]
    public Image healthBar;

    [Header("经验")]
    public Image expBar;

    [Header("等级")]
    public Text levelText;

    private void Awake()
    {
        PlayerStatsData = player.GetComponent<CharacterStats>();
    }

    private void Start()
    {
        //修改姓名
        nameText.text = playerName;

        //修改血条
        healthBar.transform.GetChild(0).GetComponent<Image>().fillAmount = PlayerStatsData.CurrentHealth / PlayerStatsData.Health;

        //修改经验
        expBar.transform.GetChild(0).GetComponent<Image>().fillAmount = PlayerStatsData.CurrentExp / PlayerStatsData.BaseExp;

        //修改等级
        levelText.text = PlayerStatsData.CurrentLevel.ToString();
    }

    private void LateUpdate()
    {
        UpdateStatsInfo();
    }

    /// <summary>
    /// 更新Player状态信息
    /// </summary>
    private void UpdateStatsInfo()
    {

        //修改血条
        healthBar.transform.GetChild(0).GetComponent<Image>().fillAmount = (float)PlayerStatsData.CurrentHealth / PlayerStatsData.Health;

        //修改经验
        expBar.transform.GetChild(0).GetComponent<Image>().fillAmount = (float)PlayerStatsData.CurrentExp / PlayerStatsData.BaseExp;

        //修改等级
        levelText.text = PlayerStatsData.CurrentLevel.ToString();
    }
}

```



## 九、传送门

传送范围同场景传送和异场景传送

传送的逻辑：

​		创建一个SceneController的单例管理类用于管理场景传送和切换(本来应该为SceneManager,但是Unity已经有这个类了，所以只能改名)

在异步加载场景时，有两种思路：

​	1.在新场景生成新的Player，但是由于UI和相机都绑定了Player，所有如果使用这种方法，生成场景后会报错找不到Player

​	2.在新场景提前导入Player，异步加载时，改变其位置到传送门

两种方法的Player都直接使用同一个SO数据，若需要采用JSON存档，只需要读档和存档都在第一时间将其赋值给Player的数据SO即可

需要注意的是：

​		在测试异步传送时，发现使用 playerTransform = GameManager.Instance.playerStats.gameObject.transform;获取的是空值，原因不明，因此采用playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

​		多次进行异步传送，在SecondScene传送回MainScene后，GameManager和SceneController都被销毁且没有重新生成，原因是

我在这两个单例的OnDestroy中添加了instance置空的方法，将其注释即可

### SceneController

```c#
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Player;


/// <summary>
/// 场景传送核心控制类
/// </summary>
public class SceneController : SingleTon<SceneController>
{
    private Transform playerTransform;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void OnDestroy()
    {
        Debug.Log("传送单例销毁");
        //SceneController.Instance.Clear();
    }

    /// <summary>
    /// 功能：传送到终点
    /// </summary>
    /// <param name="point">传送起点</param>
    public void TransitionToDestination(TransitionStart startPoint)
    {
        switch (startPoint.transitionType)
        {
            case TransitionType.SameScene:
                //同场景
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, startPoint.destinationTag));
                
                break;
            case TransitionType.DifferentScene:
                //异场景
                StartCoroutine(Transition(startPoint.sceneName,startPoint.destinationTag));

                break;
        }
    }

    /// <summary>
    /// 加载场景协程
    /// </summary>
    /// <param name="name">场景名字</param>
    /// <param name="tag">传送终点标签</param>
    /// <returns></returns>
    IEnumerator Transition(string name,DestinationTag tag)
    {
        //todo:保存数据，存档

        //名字不相同，异场景传送
        if (SceneManager.GetActiveScene().name != name)
        {
            //等待异步加载场景，加载完，才继续
            yield return SceneManager.LoadSceneAsync(name);

            //获取Player的transform
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            //修改Player位置和旋转
            playerTransform.SetPositionAndRotation(GetDestination(tag).transform.position,
                GetDestination(tag).transform.rotation);

            ////这里是在新场景生成玩家，但是需要初始化的信息太多，先保留
            //yield return Instantiate(playerPrefab,GetDestination(tag).transform.position,GetDestination(tag).transform.rotation);

            yield break;
        }
        //相同，同场景传送
        else
        {
            //获取Player的transform
            playerTransform = GameManager.Instance.playerStats.gameObject.transform;

            //修改Player位置和旋转
            playerTransform.SetPositionAndRotation(GetDestination(tag).transform.position,
                GetDestination(tag).transform.rotation);

            yield return null;
        }
        
    }

    /// <summary>
    /// 根据Tag获取终点
    /// </summary>
    /// <param name="tag">终点标签</param>
    /// <returns></returns>
    private TransitionDestination GetDestination(DestinationTag tag)
    {
        //获取场景中所有终点
        var destinations = FindObjectsOfType<TransitionDestination>();

        //搜索指定Tag的终点
        foreach (var des in destinations)
        {
            if (des.destinationTag == tag)
                return des;
        }

        return null;
    }
}

```



传送门Prefab具有传送起点和终点

逻辑：传送门1(起点A，终点B) <--->传送门2(起点B，终点A)

### 起点

起点挂载了TransitionStart类

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 传送类型枚举
/// </summary>
public enum TransitionType
{
    SameScene,
    DifferentScene
}

/// <summary>
/// 传送起点类
/// </summary>
public class TransitionStart : MonoBehaviour
{
    [Header("传送类型")] [Header("传送信息")]
    public TransitionType transitionType;

    [Header("终点场景名称")]
    public string sceneName;

    [Header("传送终点标签")]
    public DestinationTag destinationTag;

    //能否传送
    private bool canTrans;

    private void Update()
    {
        //按E传送
        if (Input.GetKeyDown(KeyCode.E) && canTrans)
        {
            //todo:SceneController 传送
            SceneController.Instance.TransitionToDestination(this);
        }
    }

    // 在触发器中时
    private void OnTriggerStay(Collider other)
    {
        Debug.Log("传送？");
        if (other.CompareTag("Player"))
            canTrans = true;
    }

    //离开触发器时
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            canTrans = false;
    }
}

```



### 终点

终点挂载了TansitionDestination类

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 传送终点标签
/// </summary>
public enum DestinationTag
{
    None,A,B
}

/// <summary>
/// 传送终点类
/// </summary>
public class TransitionDestination : MonoBehaviour
{
    [Header("该终点标签")]
    public DestinationTag destinationTag;
}

```



## 十、主界面

### 1.初始界面

实现"PRESS ENTER"闪烁，在该脚本中使用了DOTween插件的DOFade函数，但是发现一个问题，

将字体从alpha值255到0时，功能正常，但是反之就无效。

于是只能实现“字体慢慢变淡到0，然后直接回到255”的效果

```c#
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 开始提示类
/// </summary>
public class StartTip : MonoBehaviour
{
    private Text tip;

    [Header("字体渐变间隔时间")]
    public float fadeTime;
    //计时器
    private float waitTime;

    //渐变标记
    private bool canFade;

    //初始化颜色
    private Color textColor;

    private void Awake()
    {
        tip = GetComponent<Text>();

        waitTime = fadeTime;

        canFade = true;

        textColor = tip.GetComponent<Text>().color;

    }


    private void Update()
    {
        //检测回车键
        if (Input.GetKeyDown(KeyCode.Return))
        {
            LoginManager.Instance.loginProgressStats = LoginProgressStats.LOGIN;

            Destroy(this);
        }

        Fade();

        //等待
        if (!canFade)
            waitTime -= Time.deltaTime;

        //时间到，开始渐变
        if (waitTime <= 0)
        {
            canFade = true;
            //重置计时器
            waitTime = fadeTime;

            //重置透明度
            tip.GetComponent<Text>().color = textColor;
        }


    }

    /// <summary>
    /// 实现渐变效果
    /// </summary>
    private void Fade()
    {
        if (canFade)
        {
            canFade = false;
            //透明度从0到255
            tip.DOFade(0, 2f);
        }
           
    }
}

```

### 2.登录、注册

#### 1.状态切换

使用一个转换按钮实现登录、注册页面的切换，本质上就是控制“LoginBody”和“RegisterBody”是否活动

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 切换登录、注册界面按键类
/// </summary>
public class ChangeButton : MonoBehaviour
{
    [Header("登录布局")]
    public GameObject loginBody;

    [Header("注册布局")]
    public GameObject registerBody;

    //true为loginBody,false为registerBody
    private bool isLoginBody;

    //能否切换,防止按下按键后多次执行监听事件
    private bool canChange;
    //等待时间
    public float waitTime = 1f;
    //定时器
    private float timer;

    //自身文本
    private Text text;
    //自身按钮
    private Button button;

    private void Awake()
    {
        text = transform.GetChild(0).GetComponent<Text>();
        button = GetComponent<Button>();

        isLoginBody = true;
        canChange = true;

        timer = waitTime;
    }

    private void Update()
    {
        button.onClick.AddListener(changeBody);

        //倒计时
        if (!canChange)
            timer -= Time.deltaTime;

        //时间到，重置定时器，允许切换布局
        if (timer <= 0)
        {
            timer = waitTime;
            canChange = true;
        }
    }

    /// <summary>
    /// 切换布局
    /// </summary>
    private void changeBody()
    {
        if (canChange)
        {
            canChange = false;

            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            if (isLoginBody)
            {
                //切换注册
                text.text = "Login";

                loginBody.SetActive(false);

                registerBody.SetActive(true);

                isLoginBody = false;
            }
            else
            {
                //切换登录
                text.text = "Register";

                loginBody.SetActive(true);

                registerBody.SetActive(false);

                isLoginBody = true;
            }
        }
        
    }

}

```

#### 2.登录验证

调用LoginManager的登录方法进行验证，具体请查看LoginManager的Login方法

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 登录按钮类
/// </summary>
public class LoginButton : MonoBehaviour
{

    [Header("账号输入")]
    public GameObject accountText;

    [Header("密码输入")]
    public GameObject passwordText;

    [Header("登录按钮")]
    private Button loginButton;

    [Header("登录错误提示框")]
    public Text tipText;

    //输入账号
    private string account;

    //输入密码
    private string password;

    //是否能提交账号密码，用于反正按下按钮那一瞬间一直执行监听事件
    private bool canSubmit;
    //提交等待时间
    private float waitSubmitTime = 2f;
    //计时器
    private float submitTime;

    private void Awake()
    {
        loginButton = GetComponent<Button>();

        canSubmit = true;
        submitTime = waitSubmitTime;
    }

    private void Update()
    {
        //监听按钮
        loginButton.onClick.AddListener(Submit);

        if (!canSubmit)
            submitTime -= Time.deltaTime;

        //时间到了，可以继续提交了
        if(submitTime<=0)
        {
            canSubmit = true;
            submitTime = waitSubmitTime;
        }
    }

    /// <summary>
    /// 按钮监听事件
    /// </summary>
    private void Submit()
    {
        if (canSubmit)
        {
            //禁止连续提交
            canSubmit = false;

            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            //获取输入框账号、密码
            account = accountText.GetComponent<Text>().text;
            password = passwordText.GetComponent<Text>().text;

            //判断是否登录成功
            LoginErrorType Type = LoginManager.Instance.LoginAccount(account, password);
            
            switch (Type)
            {
                case LoginErrorType.ACCESS:
                    //todo:登陆成功
                    //tipText.GetComponent<Text>().text = "成功！";

                    //切换过程状态,菜单状态
                    LoginManager.Instance.loginProgressStats = LoginProgressStats.MENU;
                    break;
                case LoginErrorType.ACCOUTERROR:
                    tipText.GetComponent<Text>().text = "账号不存在！";
                    break;
                case LoginErrorType.PASSWORDERROR:
                    tipText.GetComponent<Text>().text = "密码错误！";
                    break;
            }
        }
       
    }
}

```



#### 3.注册

调用LoginManager的注册方法进行验证，具体请查看LoginManager的Register方法

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 注册按键类
/// </summary>
public class RegisterButton : MonoBehaviour
{
    private Button registerButton;

    [Header("账号输入框")]
    public Text accountText;

    [Header("密码输入框")]
    public Text passwordText;

    [Header("再次输入框")]
    public Text repeatText;

    [Header("提示框")]
    public Text tip;

    //防止点击一次多次调用监听事件
    private bool canRegister;
    //等待时间
    private float waitTime = 1f;
    //定时器
    private float timer;

    private void Awake()
    {
        registerButton = GetComponent<Button>();

        canRegister = true;
        timer = waitTime;
    }

    private void Update()
    {
        registerButton.onClick.AddListener(Register);

        //倒计时
        if (!canRegister)
            timer -= Time.deltaTime;

        //计时结束，允许再次运行事件
        if (timer <= 0)
        {
            canRegister = true;
            timer = waitTime;
        }
    }

    private void Register()
    {
        if (canRegister)
        {
            canRegister = false;

            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            string account = accountText.text;
            string password = passwordText.text;
            string repeat = repeatText.text;

            //两次密码输入不一致
            if (password != repeat)
            {
                tip.text = "密码输入不一致！";
            }
            else
            {
                RegisterErrorType errorType = LoginManager.Instance.RegisterAccount(account, password);

                switch (errorType)
                {
                    case RegisterErrorType.ACCESS:
                        tip.text = "注册成功！";
                        break;
                    case RegisterErrorType.ACCOUTERROR:
                        tip.text = "账号已存在！";
                        break;
                }
            }
        }
       
        
    }
}

```



### 3.主菜单

分为"新游戏"，"继续游戏"，"退出" 三个功能

其中，新游戏需要播放使用TimeLine制作的动画，所以当点击按钮后，会调用播放动画的方法，然后将新游戏的方法添加到动画播放完毕事件上，动画播放完毕后会自动执行新游戏的方法。

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using DG.Tweening;

/// <summary>
/// 开始游戏的类型：是新游戏还是继续游戏
/// </summary>
public enum LoadGameType
{
    NEWGAME,
    CONTINUE
}

/// <summary>
/// 主菜单类
/// </summary>
public class Menu : MonoBehaviour
{
    [Header("菜单按钮")]
    public GameObject buttonBody;

    //菜单动画
    private PlayableDirector director;

    private Button newGameBtn;
    private Button continueBtn;
    private Button exitBtn;

    //显示按钮
    private bool showButton;

    //能否操作，反正点击按钮时，多次进行监听事件
    [HideInInspector]
    public bool canNewGame;
    [HideInInspector]
    public bool canContinue;
    [HideInInspector]
    public bool canExit;

    private void Awake()
    {
        showButton = true;

        director = FindObjectOfType<PlayableDirector>();
        //给动画结束时添加事件：开始游戏
        director.stopped += NewGame;

        newGameBtn = buttonBody.transform.GetChild(0).GetComponent<Button>();
        continueBtn = buttonBody.transform.GetChild(1).GetComponent<Button>();
        exitBtn = buttonBody.transform.GetChild(2).GetComponent<Button>();

        canNewGame = true;
        canContinue = true;
        canExit = true;
    }

    private void Update()
    {
      if(LoginManager.Instance.loginProgressStats == LoginProgressStats.MENU&&showButton)
        {
            showButton = false;
            buttonBody.SetActive(true);
        }

      //监听按钮
        newGameBtn.onClick.AddListener(PlayTimeline);
        continueBtn.onClick.AddListener(ContinueGame);
        exitBtn.onClick.AddListener(ExitGame);
    }

    /// <summary>
    /// 新游戏
    /// </summary>
    private void NewGame(PlayableDirector obj)
    {
        //异步加载场景
        LoginManager.Instance.LoadMainScene("MainScene", LoadGameType.NEWGAME);
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    private void ContinueGame()
    {
        if (canContinue)
        {
            canContinue = false;

            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            //异步加载场景
            LoginManager.Instance.LoadMainScene("MainScene", LoadGameType.CONTINUE);
        }
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    private void ExitGame()
    {
        if (canExit)
        {
            canExit = false;

            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            Application.Quit();
        }
    }

    /// <summary>
    /// 播放菜单动画
    /// </summary>
    private void PlayTimeline()
    {
        if (canNewGame)
        {
            canNewGame = false;

            //播放音按钮音效
            SoundManager.Instance.PlayOnShot(3);

            //实现按钮图片效果逐渐消失
            newGameBtn.GetComponent<Image>().DOFade(0,1f);
            continueBtn.GetComponent<Image>().DOFade(0,1f);
            exitBtn.GetComponent<Image>().DOFade(0,1f);

            //播放动画
            director.Play();
        }
    }
}

```



### 4.LoginManager

1.登录管理类，管理游戏登录的进程：PRESSENTER初始，LOGIN登录，MENU菜单

2.定义了登录的错误类型： ACCESS登录成功，ACCOUTERROR账号不存在，PASSWORDERROR密码错误

3.定义了注册的错误类型： ACCESS注册成功， ACCOUTERROR账号已存在，PASSWORDERROR密码输入不一致

4.提供了登录、注册方法，本质上是调用了MysqlManager的登录、注册方法

5.提供了"新游戏"、"继续游戏"加载场景的方法，以及退出方法

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 登录错误类型枚举，用于返回错误类型
/// </summary>
public enum LoginErrorType
{
    ACCESS,//登录成功
    ACCOUTERROR,//账号不存在
    PASSWORDERROR//密码错误
}

/// <summary>
/// 登录过程状态枚举
/// </summary>
public enum LoginProgressStats
{
    PRESSENTER,//初始
    LOGIN,//登录
    MENU//菜单
}

/// <summary>
/// 注册错误类型枚举
/// </summary>
public enum RegisterErrorType
{
    ACCESS,//登录成功
    ACCOUTERROR,//账号已存在
    PASSWORDERROR//密码输入不一致
}

/// <summary>
/// 登录管理核心类
/// </summary>
public class LoginManager : SingleTon<LoginManager>
{
    //记录登录过程状态，便于切换UI
    public LoginProgressStats loginProgressStats;

    [Header("登录框")]
    public GameObject LoginCanvas;

    [Header("加载界面预制体")]
    public GameObject loadCanvasPrefab;

    private GameObject loadProgress;

    //进度条图片
    private Image progressImage;

    //进度条文字
    private Text progressText;

    //用于获取异步加载信息，制作进度条
    private AsyncOperation operation;

    //是否已登录
    private bool isLogin;



    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        //初始状态
        loginProgressStats = LoginProgressStats.PRESSENTER;
        isLogin = false;
    }

    private void Start()
    {
        SoundManager.Instance.PlayeMusic(0);
    }

    private void Update()
    {
        //切换过程状态
        SwitchProgressStats();
    }

    /// <summary>
    /// 功能：调用数据库管理的登录方法
    /// </summary>
    /// <param name="account">账号</param>
    /// <param name="password">密码</param>
    /// <returns></returns>
    public LoginErrorType LoginAccount(string account, string password)
    {
        return MysqlManager.Instance.Login(account, password);

    }

    /// <summary>
    /// 功能：调用数据库管理的注册方法
    /// </summary>
    /// <param name="account">账号</param>
    /// <param name="password">密码</param>
    /// <returns></returns>
    public RegisterErrorType RegisterAccount(string account, string password)
    {
        return MysqlManager.Instance.Register(account,password);
    }

    private void SwitchProgressStats()
    {
        switch (loginProgressStats)
        {
            case LoginProgressStats.PRESSENTER:
                break;
            case LoginProgressStats.LOGIN:

                //激活登录框
                if (!LoginCanvas.activeInHierarchy)
                {
                    //播放音弹窗音效
                    SoundManager.Instance.PlayOnShot(4);

                    LoginCanvas.SetActive(true);
                }
                    
                break;
            case LoginProgressStats.MENU:
                isLogin = true;

                //销毁登录框
                if(LoginCanvas!=null)
                    Destroy(LoginCanvas);
                


                break;
        }
    }

    /// <summary>
    /// 加载主场景
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="loadGameType">新游戏还是继续游戏</param>
    public void LoadMainScene(string sceneName,LoadGameType loadGameType)
    {
        StartCoroutine(LoadScene(sceneName,loadGameType));
    }

    /// <summary>
    /// 加载主场景协程
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="loadGameType"></param>
    /// <returns></returns>
    IEnumerator LoadScene(string sceneName,LoadGameType loadGameType)
    {
        //停止播放背景音乐
        SoundManager.Instance.StopMusic(0);

        switch (loadGameType)
        {
            case LoadGameType.NEWGAME:

                //播放过渡场景
                yield return loadProgress = Instantiate(loadCanvasPrefab);

                yield return progressImage = loadProgress.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Image>();

                yield return progressText = loadProgress.transform.GetChild(1).GetChild(0).GetChild(1).GetComponent<Text>();

                //等待异步加载场景，加载完，才继续
                operation = SceneManager.LoadSceneAsync(sceneName);

                //不允许场景自动跳转
                operation.allowSceneActivation = false;

                //场景加载没有完成时
                while (!operation.isDone)
                {
                    //滑动条 = 场景加载进度
                    progressImage.fillAmount = operation.progress;
                    progressText.text = operation.progress * 100 + "%";

                    //只能显示到0.9，之后的0.1需要手动添加
                    if (operation.progress >= 0.9F)
                    {
                        //速度太快了,做个延时
                        yield return new WaitForSeconds(2f);

                        progressImage.fillAmount = 1.0f;
                        progressText.text = "100%";

                        //允许场景自动跳转
                        operation.allowSceneActivation = true;
                    }

                    //跳出协程
                    yield return null;
                }

                yield return new WaitForSeconds(0.2f);

                //播放主场景音乐
                SoundManager.Instance.isCycle = true;//循环
                SoundManager.Instance.PlayeMusic(1);
                break;
            case LoadGameType.CONTINUE:
                yield return SceneManager.LoadSceneAsync(sceneName);

                //播放过渡场景
                yield return loadProgress = Instantiate(loadCanvasPrefab);

                yield return progressImage = loadProgress.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Image>();

                yield return progressText = loadProgress.transform.GetChild(1).GetChild(0).GetChild(1).GetComponent<Text>();

                //等待异步加载场景，加载完，才继续
                operation = SceneManager.LoadSceneAsync(sceneName);

                //不允许场景自动跳转
                operation.allowSceneActivation = false;

                //场景加载没有完成时
                while (!operation.isDone)
                {
                    //滑动条 = 场景加载进度
                    progressImage.fillAmount = operation.progress;
                    progressText.text = operation.progress * 100 + "%";

                    //只能显示到0.9，之后的0.1需要手动添加
                    if (operation.progress >= 0.9F)
                    {
                        //速度太快了,做个延时
                        yield return new WaitForSeconds(2f);

                        progressImage.fillAmount = 1.0f;
                        progressText.text = "100%";

                        //允许场景自动跳转
                        operation.allowSceneActivation = true;
                    }

                    //跳出协程
                    yield return null;
                }

                yield return new WaitForSeconds(0.2f);
                //读档
                SaveManager.Instance.LoadPlayerData();

                //播放主场景音乐
                SoundManager.Instance.isCycle = true;//循环
                SoundManager.Instance.PlayeMusic(1);

                break;
        }
       
        yield return null;
    }
}

```

### 5.MysqlManager

管理数据库相关的操作，提供登录、注册方法，本质上是SQL查找。

连接数据库步骤：

1.创建MySqlConnection，传入数据库配置参数字符串

2.使用Open方法打开数据库

3.进行数据库操作：SELECT、UPDATE等

4.使用Close、Dispose方法关闭数据库

注意：

查找步骤：

​	1.使用MySqlDataAdapter类，传入sql和数据库引用，得到含有结果的数据，并使用Fill方法将其装入数据集DataSet中

​	2.创建一个DateTable对象接收数据集的第一个表的数据即可获取查询到的数据

​	3.使用循环获取表中的值

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MySql.Data.MySqlClient;
using System.Data;

/// <summary>
/// Mysql数据库连接管理类
/// </summary>
public class MysqlManager : SingleTon<MysqlManager>
{
	private MySqlConnection dbConnection;
	
	[Header("主机")]
	public string host = "localhost";

	[Header("端口")]
	public string port = "3306";

	[Header("用户名")]
	public string username = "root";

	[Header("密码")]
	public string pwd = "123456";

	[Header("数据库")]
	public string database = "unity_originland";

    protected override void Awake()
    {
        base.Awake();

		//打开数据库
		OpenSql();
    }

    private void OnDestroy()
    {
		//关闭数据库
		Close();
    }

    /// <summary>
    /// 连接数据库
    /// </summary>
    public void OpenSql()
	{
		try
		{
			string connectionString = string.Format("server = {0};port={1};user = {2};password = {3};database = {4};", host, port, username, pwd, database);
			
			dbConnection = new MySqlConnection(connectionString);
			Debug.Log("准备建立连接...");
			dbConnection.Open();
			Debug.Log("建立连接成功！");
		}
		catch (Exception e)
		{
			throw new Exception("服务器连接失败，请重新检查是否打开MySql服务。" + e.Message.ToString());
		}
	}

	/// <summary>
	/// 关闭数据库连接
	/// </summary>
	public void Close()
	{
		if (dbConnection != null)
		{
			dbConnection.Close();
			dbConnection.Dispose();
			dbConnection = null;
		}
	}

	/// <summary>
	/// 查询所有
	/// </summary>
	public void findAll()
    {
		//数据集：用于存储读取到的数据
		DataSet dataSet = new DataSet();

		//查询
		string sql = "select account,password from user where account = 'lch'";

		MySqlDataAdapter adapter = new MySqlDataAdapter(sql,dbConnection);

		//放入数据集中
		adapter.Fill(dataSet);

		//读取数据集中数据并显示
		DataTable dataTable = dataSet.Tables[0];

        foreach (DataRow row in dataTable.Rows)
        {
            foreach (DataColumn col in dataTable.Columns)
            {
                Debug.Log(row[col].ToString());
            }
        }
    }

	/// <summary>
	/// 功能：登录，向数据库查询账号、密码并核对
	/// </summary>
	/// <param name="account"></param>
	/// <param name="password"></param>
	/// <returns></returns>
	public LoginErrorType Login(string account,string password)
	{
		//数据集：用于存储读取到的数据
		DataSet dataSet = new DataSet();

		//查询表中所有数据
		string sql = "select account,password from user";

		MySqlDataAdapter adapter = new MySqlDataAdapter(sql, dbConnection);

		//放入数据集中
		adapter.Fill(dataSet);

		//读取数据集中数据并显示
		DataTable dataTable = dataSet.Tables[0];


        foreach (DataRow row in dataTable.Rows)
        {
			//匹配账号
            if (row[0].ToString() == account)
            {
				//匹配密码
                if (row[1].ToString() == password)
                    return LoginErrorType.ACCESS;
                else
                {
					return LoginErrorType.PASSWORDERROR;
                }
            }
        }

        return LoginErrorType.ACCOUTERROR;
	}

	public RegisterErrorType Register(string account,string password)
    {
		//数据集：用于存储读取到的数据
		DataSet dataSet = new DataSet();

		//查询表中所有数据
		string sql = "select account,password from user";

		MySqlDataAdapter adapter = new MySqlDataAdapter(sql, dbConnection);

		//放入数据集中
		adapter.Fill(dataSet);

		//读取数据集中数据并显示
		DataTable dataTable = dataSet.Tables[0];

        //查找该账号是否已经注册
        foreach (DataRow row in dataTable.Rows)
        {
			if (row[0].ToString() == account)
				return RegisterErrorType.ACCOUTERROR;
		}

		//未注册，注册
		sql = string.Format("insert into user(account,password) values('{0}','{1}')",account,password);

		MySqlCommand comd = new MySqlCommand(sql, dbConnection);

		int result = comd.ExecuteNonQuery();

		return RegisterErrorType.ACCESS;

	}
}


```



### 6.SoundManager

管理背景音乐播放

1.使用一个列表装入AudioClip，称为音乐组

2.提供调用音乐的方法，传入的参数是音乐在音乐组中的序号

注意：

想在播放背景音乐的同时，播放一些短暂的点击音效可以使用PlayOneShot方法

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 音乐管理核心类
/// </summary>
public class SoundManager : SingleTon<SoundManager>
{

    //音源
    private AudioSource sound;

    [Header("音乐片段组")]
    public List<AudioClip> clips;

    //是否循环
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
    /// 播放音乐
    /// </summary>
    /// <param name="num">序号</param>
    public void PlayeMusic(int num)
    {
        sound.loop = isCycle;
        sound.clip = clips[num];
        sound.Play();
    }

    /// <summary>
    /// 播放短音效，按钮什么的
    /// </summary>
    /// <param name="num"></param>
    public void PlayOnShot(int num)
    {
        sound.PlayOneShot(clips[num]);
    }

    /// <summary>
    /// 暂停音乐
    /// </summary>
    /// <param name="num">序号</param>
    public void PasueMusic(int num)
    {
        sound.clip = clips[num];
        sound.Pause();
    }

    /// <summary>
    /// 结束音乐
    /// </summary>
    /// <param name="num">序号</param>
    public void StopMusic(int num)
    {
        sound.clip = clips[num];
        sound.Stop();
    }

}

```

