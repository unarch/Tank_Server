using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


// 处理角色协议
public class HandlePlayerMsg
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
}
