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
        Scene.instance!.DelPlayer(player.id!);
    }
}

