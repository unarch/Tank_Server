using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 房间
public class Room
{
    // 状态
    public enum Status
    {
        Prepare = 1,
        Fight = 2,
    }
    public Status status = Status.Prepare;
    // 玩家
    public int maxPlayers = 6;
    public Dictionary<string, Player> list = new Dictionary<string, Player>();


    // 添加玩家
    public bool AddPlayer(Player player)
    {
        lock(list)
        {
            if (list.Count >= maxPlayers) return false;
            PlayerTempData tempData = player.tempData!;
            tempData.room = this;
            tempData.team = SwitchTeam();
            tempData.status = PlayerTempData.Status.Room;

            if (list.Count == 0) tempData.isOwner = true;
            string id = player.id!;
            list.Add(id, player);
        }
        return true;
    }

    // 分配队伍
    public int SwitchTeam()
    {
        int count1 = 0;
        int count2 = 0;
        foreach(Player player in list.Values)
        {
            if (player.tempData!.team == 1) count1 ++;
            if (player.tempData!.team == 2) count2 ++;
        }
        if (count1 <= count2) return 1;
        return 2;
    }

    // 删除玩家
    public void DelPlayer(string id) 
    {
        lock(list)
        {
            if (!list.ContainsKey(id)) return;
            bool isOwner = list[id].tempData!.isOwner;
            list[id].tempData!.status = PlayerTempData.Status.None;
            list.Remove(id);
            if (isOwner) UpdateOwner();
        }
    }

    // 更换房主
    public void UpdateOwner()
    {
        lock (list)
        {
            if (list.Count <= 0) return;
            foreach(Player player in list.Values)
            {
                player.tempData!.isOwner = false;
            }
            Player p = list.Values.First();
            p.tempData!.isOwner = true;
        }
    }

    // 广播
    public void BroadCast(ProtocolBase protocol)
    {
        foreach(Player player in list.Values)
        {
            player.Send(protocol);
        }
    }

    // 房间信息
    public ProtocolBytes GetRoomInfo()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomInfo");
        // 房间信息
        protocol.AddInt(list.Count);
        // 每个玩家的信息
        foreach(Player p in list.Values)
        {
            protocol.AddString(p.id!);
            protocol.AddInt(p.tempData!.team);
            protocol.AddInt(p.data!.win);
            protocol.AddInt(p.data!.fail);
            int isOwner = p.tempData.isOwner ? 1 : 0;
            protocol.AddInt(isOwner);
        }
        return protocol;
    }

    // 房间是否可以开战
    public bool CanStart()
    {
        if (status != Status.Prepare) return false;
        int count1 = 0;
        int count2 = 0;

        foreach(Player player in list.Values)
        {
            if (player.tempData!.team == 1) count1 ++;
            if (player.tempData!.team == 2) count2 ++;
        }
        if (count1 < 1 || count2 < 1) return false;
        return true;
    }

    public void StartFight()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Fight");
        status = Status.Fight;
        int teamPos1 = 1;
        int teamPos2 = 1;
        lock (list)
        {
            protocol.AddInt(list.Count);
            foreach (Player p in list.Values)
            {
                p.tempData!.hp = 200;
                protocol.AddString(p.id!);
                protocol.AddInt(p.tempData!.team);
                if (p.tempData.team == 1) protocol.AddInt(teamPos1++);
                else protocol.AddInt(teamPos2++);
                p.tempData!.status = PlayerTempData.Status.Fight;
            }
            BroadCast(protocol);
        }
    }

    private int IsWin()
    {
        if (status != Status.Fight) return 0;
        int count1 = 0;
        int count2 = 0;
        foreach (Player player in list.Values)
        {
            PlayerTempData pt = player.tempData!;
            if (pt.team == 1 && pt.hp > 0) count1 ++;
            if( pt.team == 2 && pt.hp > 0) count2 ++;
        }
        if (count1 <= 0) return 2;
        if (count2 <= 0) return 1;
        return 0;

    }

    public void UpdateWin()
    {
        int isWin = IsWin();
        if (isWin == 0) return;
        // 改变状态
        lock(list)
        {
            status = Status.Prepare;

            foreach (Player player in list.Values)
            {
                player.tempData!.status = PlayerTempData.Status.Room;
                if (player.tempData.team == isWin)
                    player.data!.win ++;
                else
                    player.data!.fail ++;
            }
        }
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Result");
        protocol.AddInt(isWin);
        BroadCast(protocol);
    }

    // 中途退出游戏
    public void ExitFight(Player player)
    {
        // 摧毁坦克
        if (list[player.id!] != null)
            list[player.id!].tempData!.hp = -1;
        // 广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("Hit");
        protocolRet.AddString(player.id!);
        protocolRet.AddString(player.id!);
        protocolRet.AddFloat(999);
        if (IsWin() == 0) player.data!.fail ++;
        UpdateWin();

    }

}


