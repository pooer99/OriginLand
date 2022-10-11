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

