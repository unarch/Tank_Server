using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class Service
{
    // 监听套接字
    public Socket? listenFd;
    // 客户端连接
    public Conn[]? conns;
    // 最大连接数
    public int maxConn = 50;

    // 获取连接池索引，返回负数表示获取失败
    public int NewIndex()
    {
        if (conns == null) return -1;
        for (int i = 0; i < conns.Length; i++)
        {
            if (conns[i] == null) {
                conns[i] = new Conn();
                return i;
            }
            else if (conns[i].isUse == false) {
                return i;
            }
        }
        return -1;
    }
    
    // 开启服务器
    public void Start(string host, int port) 
    {
        // 初始化连接池
        conns = new Conn[maxConn];
        for (int i = 0; i < maxConn; i++) conns[i] = new Conn();

        //Socket
        listenFd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Bind
        IPAddress ipAdr = IPAddress.Parse(host);
        IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
        listenFd.Bind(ipEp);
        // Listen
        listenFd.Listen(maxConn);
        // Accept
        listenFd.BeginAccept(AcceptCb, null);
        Console.WriteLine("[服务器]启动成功");
    }

    //Accept回调
    private void AcceptCb(IAsyncResult ar)
    {
        try {
            Socket socket = listenFd!.EndAccept(ar);
            int index = NewIndex();

            if (index < 0)
            {
                socket.Close();
                Console.WriteLine("[警告]链接已满");
            }
            else 
            {
                Conn conn = conns![index];
                conn.Init(socket);
                string adr = conn.GetAddress();
                Console.WriteLine("客户端连接 [" + adr + "] conn池ID : " + index);
                conn.socket!.BeginReceive(conn.readBuff,conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                listenFd.BeginAccept(AcceptCb, null);
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("AcceptCb 失败 :" + e.Message);
        }
    }

    // 接收指令
    private void ReceiveCb(IAsyncResult ar)
    {
        Conn conn = (Conn)ar.AsyncState!;
        // 关闭信号
        try {
            int count = conn.socket!.EndReceive(ar);
            // 关闭信号
            if (count <= 0) {
                Console.WriteLine("收到[" + conn.GetAddress() + "] 断开连接");
                conn.Close();
                return;
            }
            // 数据处理
            string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, 0, count);
            Console.WriteLine("收到[" + conn.GetAddress() + "]数据:" + str);
            str = conn.GetAddress() + ":" + str;
            byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
            // 广播
            for (int i = 0; i < conns!.Length; i++) {
                if (conns[i] == null) continue;
                if (!conns[i].isUse) continue;
                Console.WriteLine("将消息转播给 " + conns[i].GetAddress());
                conns[i].socket!.Send(bytes);
            }
            // 继续接收
            conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);

        } catch(Exception e) {
            Console.WriteLine("收到[" + conn.GetAddress() + "] 断开连接 信息:"+e.Message );
            conn.Close();
        }
        
    }
}
