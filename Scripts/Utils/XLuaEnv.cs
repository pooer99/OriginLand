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
