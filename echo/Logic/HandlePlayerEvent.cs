using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class HandlePlayerEvent
{
    // 上线
    public void OnLogin(Player player) 
    {
        Scene.instance!.AddPlayer(player.id!);
    }

    // 下线
    public void OnLogout(Player player)
    {
        if (player.tempData!.status == PlayerTempData.Status.Room)
        {
            Room room = player.tempData!.room;
            RoomMgr.instance!.LeaveRoom(player);
            if (room != null)
                room.BroadCast(room.GetRoomInfo());
        }

        // 战斗中退出
        if (player.tempData!.status == PlayerTempData.Status.Fight)
        {
            Room room = player.tempData!.room;
            if (room != null)
                room.ExitFight(player);
            RoomMgr.instance!.LeaveRoom(player);
        }
        
        Scene.instance!.DelPlayer(player.id!);
    }
}

