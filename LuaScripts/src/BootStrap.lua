---
--- Created by Pooer.
--- DateTime: 2022-09-15 16:32
---
--- Lua核心向导Table

package.path = package.path..";D:\\Unity Project\\OriginLand\\Assets\\LuaScripts\\src\\?.lua;"
------------------定义----------------------------
BootStrap = {}

CU = CS.UnityEngine
Vector3 = CU.Vector3
Vector2 = CU.Vector2
Mathf = CU.Mathf
Input = CU.Input

--- 控制器核心Table
BootStrap["ShowCSharpRef"] = require ("xlua.util")
BootStrap["Controllers/PlayerController"] = require("PlayerController_Lua")

------------------测试C#委托-------------------------
ShowCSharpRef = function()
    print(BootStrap["ShowCSharpRef"].print_func_ref_by_csharp())
end
