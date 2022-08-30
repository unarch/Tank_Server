using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.IO;
using System.Net.Sockets;
using System;
using System.Net;
using System.Timers;


class MainClass
{


    public static void Main(string[] args)
    {
        // MemoryStream stream = new MemoryStream();
        // PlayerData playerData = new PlayerData();
        // IFormatter formatter = new BinaryFormatter();
        // formatter.Serialize(stream, playerData);
        // Console.WriteLine("playerData = "+playerData.score);
        // byte[] byteArr = stream.ToArray();
        // Console.WriteLine("playerData = "+stream.Length);
        // IFormatter formatter2 = new BinaryFormatter();
        // PlayerData player2 = (PlayerData)formatter2.Deserialize(stream);
        // Console.WriteLine("byteArr = "+ player2.score);
    
        DataMgr dataMgr = new DataMgr();
        // 注册
        bool ret = dataMgr.Register("LJY","123");
        if (ret) Console.WriteLine("注册成功");
        else Console.WriteLine("注册失败");

        // 创建玩家
        ret = dataMgr.CreatePlayer("LJY");
        if (ret) Console.WriteLine("创建玩家成功");
        else Console.WriteLine("创建玩家失败");

        // 获取玩家数据
        PlayerData? pd = dataMgr.GetPlayerData("LJY");
        if (pd != null) Console.WriteLine("获取玩家成功 分数是 " + pd.score);
        else Console.WriteLine("获取玩家数据失败");

        if (pd != null) pd!.score += 10;
        // 更改玩家数据
        

        // 保持数据
        Player p = new Player();
        p.id = "LJY";
        p.data = pd;
        dataMgr.SavePlayer(p);
        // 重新读取
        pd = dataMgr.GetPlayerData("LJY");
        if (pd != null) Console.WriteLine("获取玩家成功 分数是 " + pd.score);
        else Console.WriteLine("重新获取玩家数据失败");

    }

}

