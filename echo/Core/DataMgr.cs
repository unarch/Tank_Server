using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


public class DataMgr
{
    MySqlConnection sqlConn;

    // 单例模式
    public static DataMgr? instance;
    public DataMgr()
    {
        instance = this;
        Connect();
    }

    // 连接
    public void Connect()
    {
        // 数据库
        string connStr = "Database=game;Data Source=127.0.0.1;";
        connStr += "User Id=root;Password=00000000;port=3306";
        sqlConn = new MySqlConnection(connStr);
        try
        {
            sqlConn.Open();
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]Connect " + e.Message);
            return;
        }
    }

    // 判定安全字符串
    public bool IsSafeStr(string str)
    {
        return !Regex.IsMatch(str, @"[-|;|,|\/|||\}|\{|%|@|\*|!|\']");
    }

    private bool CanRegister(string id)
    {
        if (!IsSafeStr(id)) return false;
        // 查询id是否存在
        string cmdStr = string.Format("select * from user where id='{0}';", id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        try
        {
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            return !hasRows;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]CanRegister fail " + e.Message);
            return false;
        }
    }

    // 注册
    public bool Register(string id, string pw)
    {
        if (!IsSafeStr(id) || !IsSafeStr(pw))
        {
            Console.WriteLine("[DataMgr]Register 使用非法字符");
            return false;
        }
        // 能否注册
        if (!CanRegister(id))
        {
            Console.WriteLine("[DataMgr]Register !CanRegister");
            return false;
        }
        // 写入数据库User表
        string cmdStr = string.Format("insert into user set id = '{0}' , pw = '{1}';", id, pw);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        try
        {
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]Register " + e.Message);
            return false;
        }
    }

    // 创建角色
    public bool CreatePlayer(string id)
    {
        if (!IsSafeStr(id)) return false;
        // 序列化
        IFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        PlayerData playerData = new PlayerData();
        Console.WriteLine("playerData = "+playerData.score);
        try
        {
            formatter.Serialize(stream, playerData);
        }
        catch (Exception e)
        {
            Console.WriteLine("[DataMgr]CreatePlayer 序列化 " + e.Message);
            return false;
        }
        byte[] byteArr = stream.ToArray();
        // 写入数据库
        string cmdStr = string.Format("insert into player set id = '{0}', data = @data;", id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        cmd.Parameters.Add("@data", MySqlDbType.Blob);
        cmd.Parameters[0].Value = byteArr;
        try {
            cmd.ExecuteNonQuery();
            return true;
        } catch (Exception e) {
            Console.WriteLine("[DataMgr]CreatePlayer 写入 " + e.Message);
            return false;
        }
    }
    
    // 检测用户名和密码
    public bool CheckPassWord(string id, string pw)
    {
        if(!IsSafeStr(id) || !IsSafeStr(pw)) return false;
        // 查询
        string cmdStr = string.Format("select * from user where id='{0}' and pw ='{1}';", id, pw);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        try {
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            return hasRows;
        } catch (Exception e) {
            Console.WriteLine("[DataMgr]CheckPassWord " + e.Message);
            return false;
        }
    }

    // 获取玩家数据
    public PlayerData? GetPlayerData(string id) 
    {
        PlayerData? playerData = null;
        if (!IsSafeStr(id)) return playerData;
        // 查询
        string cmdStr = string.Format("select * from player where id = '{0}';", id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        byte[] buffer = new byte[1024];
        
        try {
            MySqlDataReader dataReader = cmd.ExecuteReader();
            Console.WriteLine("dataReader = "+ dataReader + " id = " + id);
            if (!dataReader.HasRows) {
                dataReader.Close();
                return playerData;
            }

            dataReader.Read();

            long len = dataReader.GetBytes(1,0,null,0,0);
            buffer = new byte[len];
            dataReader.GetBytes(1,0,buffer,0,(int)len);

            dataReader.Close();
        } catch(Exception e) {
            Console.WriteLine("[DataMgr]GetPlayerData 查询 " + e.Message);
            return playerData;
        }
        // 反序列化
        MemoryStream stream = new MemoryStream(buffer);
        try {
            BinaryFormatter formatter = new BinaryFormatter();
            playerData = (PlayerData)formatter.Deserialize(stream);
            return playerData;
        } catch (SerializationException e) {
            Console.WriteLine("[DataMgr]GetPlayerData 反序列化 " + e.Message);
            return playerData;
        }
    }

    // 保存角色
    public bool SavePlayer(Player player)
    {
        PlayerData playerData = player.data;
        // 序列化
        IFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        try {
            formatter.Serialize(stream, playerData);
        } catch (Exception e) {
            Console.WriteLine("[DataMgr]SavePlayer 序列化 " + e.Message);
            return false;
        }
        byte[] byteArr = stream.ToArray();
        // 写入数据库
        string formatStr = "update player set data = @data where id = '{0}';";
        string cmdStr = string.Format(formatStr, player.id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        cmd.Parameters.Add("@data", MySqlDbType.Blob);
        cmd.Parameters[0].Value = byteArr;
        try {
            cmd.ExecuteNonQuery();
            return true;
        } catch (Exception e) {
            Console.WriteLine("[DataMsg]SavePlayer 写入 " + e.Message);
            return false;
        }

    }


}
