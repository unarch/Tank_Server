using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class Player
{
    // id、连接、玩家数据
    public string? id;
    public Conn? conn;
    public PlayerData? data;
    public PlayerTempData? tempData;


    public Player(string id, Conn conn)
    {
        this.id = id;
        this.conn = conn;
        tempData = new PlayerTempData();
    }

    // 发送
    public void Send(ProtocolBase protocol)
    {
        if (conn == null) return;
        ServeNet.instance!.Send(conn, protocol);
    }
    
    // 踢下线
    public static bool KickOff(string id, ProtocolBase protocol)
    {
        Conn[] conns = ServeNet.instance!.conns;
        for (int i = 0; i < conns.Length; i++)
        {
            if (conns[i] == null) continue;
            if (!conns[i].isUse) continue;
            if (conns[i].player == null) continue;
            Player player = conns[i].player!;
            if (player.id != id) continue;
            lock(player!)
            {
                if (protocol != null)
                    player.Send(protocol);
                return player.Logout();
            }
            
        }
        return true;
    }

    // 下线
    public bool Logout()
    {
        // *TODO: 事件处理
        ServeNet.instance!.handlePlayerEvent.OnLogout(this);

        // 保存
        if (!DataMgr.instance!.SavePlayer(this)) return false;

        // 下线
        conn!.player = null;
        conn.Close();
        return true;
    }

}
