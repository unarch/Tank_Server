using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 临时数据 - > 不存储到数据库中的数据
public class PlayerTempData
{
    public PlayerTempData()
    {
        status = Status.None;
    }

    // 状态
    public enum Status {
        None,
        Room,
        Fight,
    }
    public Status status;
    // room 状态
    public Room room;
    public int team = 1;
    public bool isOwner = false;
}
