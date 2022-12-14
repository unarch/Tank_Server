using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


// 处理角色协议
public partial class HandlePlayerMsg
{
    // 获取分数
    // 协议参数:
    // 返回协议: int 分数

    public void MsgGetScore(Player player, ProtocolBase protocolBase)
    {
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("GetScore");
        protocolRet.AddInt(player.data!.score);
        player.Send(protocolRet);
        Console.WriteLine("MsgGetScore " + player.id + player.data.score);
    }

    //  增加分数
    //  协议参数:
    public void MsgAddScore(Player player, ProtocolBase protocolBase)
    {
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protocolBase;
        string protocolName = protocol.GetString(start, ref start);
        // 处理
        player.data!.score += 1;

        Console.WriteLine("MsgAddScore " + player.id + player.data.score);
    }

    // 获取玩家列表
    public void MsgGetList(Player player, ProtocolBase protocolBase)
    {
        Scene.instance!.SendPlayerList(player);
    }

    // 更新信息
    public void MsgUpdateInfo(Player player, ProtocolBase protocolBase)
    {
        // 获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes) protocolBase;
        string protocolName = protocol.GetString(start, ref start);
        float x = protocol.GetFloat(start, ref start);
        float y = protocol.GetFloat(start, ref start);
        float z = protocol.GetFloat(start, ref start);
        int score = player.data!.score;
        Scene.instance!.UpdateInfo(player.id!, x, y, z, score);
        // 广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("UpdateInfo");
        protocolRet.AddString(player.id!);
        protocolRet.AddFloat(x);
        protocolRet.AddFloat(y);
        protocolRet.AddFloat(z);
        protocolRet.AddInt(score);
        ServeNet.instance!.Broadcast(protocolRet);
    }



    // 获取玩家信息
    public void MsgGetAchieve(Player player, ProtocolBase protocolBase)
    {
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("GetAchieve");
        protocolRet.AddInt(player.data!.win);
        protocolRet.AddInt(player.data!.fail);
        player.Send(protocolRet);
        Console.WriteLine("MsgGetScore " + player.id + "win : " + player.data!.win);
    }

}
