using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

public class Conn
{
    // 常量
    public const int BUFFER_SIZE = 1024;
    // socket
    public Socket? socket;
    // 是否使用
    public bool isUse = false;
    // Buff
    public byte[] readBuff = new byte[BUFFER_SIZE];
    public int buffCount = 0;
    // 构造函数
    public Conn()
    {
        readBuff = new byte[BUFFER_SIZE];
    }

    // 初始化
    public void Init(Socket socket)
    {
        this.socket = socket;
        isUse = true;
        buffCount = 0;
    }
    // 缓冲区剩余的字节数
    public int BuffRemain()
    {
        return BUFFER_SIZE - buffCount;
    }
    // 获取客户端地址
    public string GetAddress()
    {
        if (!isUse) return "无法获取地址";
        return socket!.RemoteEndPoint.ToString();
    }

    // 关闭
    public void Close()
    {
        if (!isUse) return;
        Console.WriteLine("[断开连接]" + GetAddress());
        socket!.Close();
        isUse = false;
    }

}
