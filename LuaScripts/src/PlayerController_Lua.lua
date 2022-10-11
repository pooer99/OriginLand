---
--- Created by Pooer.
--- DateTime: 2022-09-09 14:28
---
------------------定义----------------------------
local PlayerController = {}

-- 动画组件
local animator
-- 刚体组件
local rb
-- Player 状态数据
local PlayerStatData

-- 允许移动标志
local canMove = true
-- 允许跳跃标志
local canJump = false
-- 起跳
local isJump = false
-- 是否在降落
local isFall = false
-- 武器显示标志
local isShow = false
-- 死亡
local isDead = false
-- 攻击
local canAttack = true

-- 攻击模式
local attackMode = {}
attackMode.Attack1 = 1
attackMode.Attack2 = 2
-- 当前攻击方式
attackMode.nowMode = 1
-- 攻击缓存时间
local waitAttackTime = 1.0
local attacktimer

----------------生命周期--------------------------
PlayerController.Awake = function()

    -- 获取动画组件
    animator =  self:GetComponent(typeof(CS.UnityEngine.Animator))

    -- 获取刚体组件
    rb = self:GetComponent(typeof(CS.UnityEngine.Rigidbody))

    -- 获取Player的状态数据
    PlayerStatData = self:GetComponent(typeof(CS.CharacterStats));

end

PlayerController.Start = function()
    -- 隐藏光标
    CU.Cursor.lockState = CU.CursorLockMode.Locked

    -- 向GameManager注册Player状态
    self:RigisterPlayer(PlayerStatData)

    -- 初始化攻击时间
    attacktimer = waitAttackTime
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
    if(canAttack)
    then
        -- 不允许再次攻击，等待计时
        canAttack = false

        -- 锁定敌人
        if(self:FoundEnemy())
        then
            self.transform:LookAt(self.attackTarget.transform)
        end

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
end

-- Animation event
Hit = function()
    if(self.attackTarget ~= nil)then
        local targetStats = self.attackTarget:GetComponent(typeof(CS.CharacterStats))
        if(targetStats~=nil)then
            targetStats:TakeDamage(PlayerStatData,targetStats)
        end
    end
end

PlayerController.UpDate = function()
    if(PlayerStatData.CurrentHealth == 0)then
        -- Player死亡
        isDead = true
        animator:SetBool("Death",isDead);

        -- 广播给所有敌人
        self:EndNotify()
    else

        --- Player攻击
        if(Input.GetKeyDown(CU.KeyCode.Mouse0))
        then
            PlayerAttack()
        end

        -- 攻击计时
        if(canAttack == false)then
            attacktimer = attacktimer - CU.Time.deltaTime
        end

        -- 攻击计时结束，允许攻击
        if(attacktimer <= 0)then
            canAttack = true

            -- 重置计时器
            attacktimer =waitAttackTime
        end

        --- Player移动
        if(canMove)then
            PlayerMove()
        end

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
end

PlayerController.OnDestroy = function()
    print("OnDestroy")
end

return PlayerController