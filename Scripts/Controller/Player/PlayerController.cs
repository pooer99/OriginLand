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
        public GameObject attackTarget;

        [HideInInspector]
        public float currentVelocity;

        [Header("Player移动速度")]
        public float Speed;

        [Header("Player转动速度")]
        public float smoothTime = 1f;

        [Header("Player起跳力度")]
        public float jumpGravity = 500f;

        [Header("Player检测敌人的范围")]
        public float sightRadius;

        [Header("武器")]
        public GameObject weapon;

        //攻击委托方法
        private delegate void LuaFunction();
        LuaFunction Hit_Lua;

        private void Awake()
        {
            
            //加载Lua脚本
            XLuaEnv.Instance.DoString("require 'BootStrap'");

            //设置Lua中的self为c#的this
            XLuaEnv.Instance.Global.Set("self", this);

            //获取Lua中PlayerController表
            LuaTable table = XLuaEnv.Instance.Global.Get<LuaTable>("BootStrap");
            core = table.Get<BootStrapStruct>("Controllers/PlayerController");

            Hit_Lua = XLuaEnv.Instance.Global.Get<LuaFunction>("Hit");

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
            Hit_Lua = null;

            //销毁XLua环境
            XLuaEnv.Instance.FreeEnv();
        }

        /// <summary>
        /// 检测敌人
        /// </summary>
        /// <returns></returns>
        public bool FoundEnemy()
        {
            //获取敌人可视范围内的碰撞体
            var colliders = Physics.OverlapSphere(this.transform.position, sightRadius);

            //检索并查找是否有Player
            foreach (var target in colliders)
            {
                if (target.CompareTag("Enemy"))
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
        /// Animation event：攻击
        /// </summary>
        public void Hit()
        {
            Hit_Lua();
        }

        /// <summary>
        /// 注册Player状态信息
        /// </summary>
        public void RigisterPlayer(CharacterStats stat)
        {
            GameManager.Instance.RigisterPlayer(stat);
        }

        /// <summary>
        /// 游戏结束广播
        /// 功能：Player死亡后广播，让所以订阅了广播的敌人执行广播方法
        /// </summary>
        public void EndNotify()
        {
            GameManager.Instance.NotifyObserrvers();
        }
    }
}