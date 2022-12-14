using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public partial class HandlePlayerMsg
{
    // 开始战斗
    public void MsgStartFight(Player player, ProtocolBase protocolBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("StartFight");

        // 条件判断
        if (player.tempData!.status != PlayerTempData.Status.Room)
        {
            Console.WriteLine("MsgStartFight status error " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }

        if (!player.tempData.isOwner)
        {
            Console.WriteLine("MsgStartFight owner error " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }

        Room room = player.tempData!.room;
        if (!room.CanStart())
        {
            Console.WriteLine("MsgStartFight CanStart error " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        // 开启战斗
        protocol.AddInt(0);
        player.Send(protocol);
        room.StartFight();
    }    

    // 同步坦克单元
	public void MsgUpdateUnitInfo(Player player, ProtocolBase protocolBase)
	{
		//获取数值
		int start = 0;
		ProtocolBytes protocol = (ProtocolBytes)protocolBase;
		string protocolName = protocol.GetString (start, ref start);
		float posX = protocol.GetFloat (start, ref start);
		float posY = protocol.GetFloat (start, ref start);
		float posZ = protocol.GetFloat (start, ref start);
		float rotX = protocol.GetFloat (start, ref start);
		float rotY = protocol.GetFloat (start, ref start);
		float rotZ = protocol.GetFloat (start, ref start);
		float gunRot = protocol.GetFloat (start, ref start);
		float gunRoll = protocol.GetFloat (start, ref start);
		//获取房间
		if (player.tempData!.status != PlayerTempData.Status.Fight)
			return;
		Room room = player.tempData.room;
		//作弊校验 略
		player.tempData.posX = posX;
		player.tempData.posY = posY;
		player.tempData.posZ = posZ;
		player.tempData.lastUpdateTime = Sys.GetTimeStamp ();
		//广播
		ProtocolBytes protocolRet = new ProtocolBytes();
		protocolRet.AddString ("UpdateUnitInfo");
		protocolRet.AddString (player.id!);
		protocolRet.AddFloat (posX);
		protocolRet.AddFloat (posY);
		protocolRet.AddFloat (posZ);
		protocolRet.AddFloat (rotX);
		protocolRet.AddFloat (rotY);
		protocolRet.AddFloat (rotZ);
		protocolRet.AddFloat (gunRot);
		protocolRet.AddFloat (gunRoll);
		room.BroadCast (protocolRet);
	}

	public void MsgShooting(Player player, ProtocolBase protocolBase)
	{
		//获取数值
		int start = 0;
		ProtocolBytes protocol = (ProtocolBytes)protocolBase;
		string protocolName = protocol.GetString (start, ref start);
		float posX = protocol.GetFloat (start, ref start);
		float posY = protocol.GetFloat (start, ref start);
		float posZ = protocol.GetFloat (start, ref start);
		float rotX = protocol.GetFloat (start, ref start);
		float rotY = protocol.GetFloat (start, ref start);
		float rotZ = protocol.GetFloat (start, ref start);
		//获取房间
		if (player.tempData!.status != PlayerTempData.Status.Fight)
			return;
		Room room = player.tempData.room;
		//广播
		ProtocolBytes protocolRet = new ProtocolBytes();
		protocolRet.AddString ("Shooting");
		protocolRet.AddString (player.id!);
		protocolRet.AddFloat (posX);
		protocolRet.AddFloat (posY);
		protocolRet.AddFloat (posZ);
		protocolRet.AddFloat (rotX);
		protocolRet.AddFloat (rotY);
		protocolRet.AddFloat (rotZ);
		room.BroadCast (protocolRet);
	}

	//伤害处理
	public void MsgHit(Player player, ProtocolBase protocolBase)
	{
		//解析协议
		int start = 0;
		ProtocolBytes protocol = (ProtocolBytes)protocolBase;
		string protocolName = protocol.GetString (start, ref start);
		string enemyName = protocol.GetString (start, ref start);
		float damage = protocol.GetFloat (start, ref start);
		//作弊校验
		long lastShootTime = player.tempData!.lastShootTime;
		if (Sys.GetTimeStamp () - lastShootTime < 1) 
		{
			Console.WriteLine ("MsgHit开炮作弊 " + player.id);
			return;
		}
		player.tempData.lastShootTime = Sys.GetTimeStamp();
		//更多作弊校验 略
		//获取房间
		if (player.tempData.status != PlayerTempData.Status.Fight)
			return;
		Room room = player.tempData.room;
		//扣除生命值
		if (!room.list.ContainsKey (enemyName))
		{
			Console.WriteLine ("MsgHit not Contains enemy " + enemyName);
			return;
		}
		Player enemy = room.list[enemyName];
		if (enemy == null)
			return;
		if (enemy.tempData!.hp <= 0)
			return;
		enemy.tempData.hp -= damage;
		Console.WriteLine("MsgHit " + enemyName + "  hp:" + enemy.tempData.hp + " damage:" + damage);
		//广播
		ProtocolBytes protocolRet = new ProtocolBytes();
		protocolRet.AddString ("Hit");
		protocolRet.AddString (player.id!);
		protocolRet.AddString (enemy.id!);
		protocolRet.AddFloat (damage);
		room.BroadCast (protocolRet);
		//胜负判断
		room.UpdateWin ();   //下一节实现
	}



}
