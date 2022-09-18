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

        [Header("武器")]
        public GameObject weapon;

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