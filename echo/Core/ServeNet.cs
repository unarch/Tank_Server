using System.Reflection;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Net;


public class ServeNet
{
    // 监听套接字
    public Socket? listenFd;
    // 客户端连接
    public Conn[] conns;
    // 最大连接数
    public int maxConn = 50;
    // 单例
    public static ServeNet? instance;
        // 数据库
    MySqlConnection? sqlConn;

    // 数据库名字
    public static string databaseName = "game";


    // 协议
    public ProtocolBase protocol;

    // 打印信息
    public void Print()
    {
        Console.WriteLine("===服务器登录信息===");
        for (int i = 0; i < conns.Length; i++)
        {
            if (conns[i] == null) continue;
            if (!conns[i].isUse) continue;
            string str = "连接[" + conns[i].GetAddress() + "] ";
            if (conns[i].player != null)
                str += "玩家id " + conns[i].player!.id;
            Console.WriteLine(str);
        }
    }
    
    
    public ServeNet()
    {
        instance = this;
        conns = new Conn[]{};
        protocol = new ProtocolBytes();
    }

    // 获取连接池索引， 返回负数表示获取失败
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
        // 定时器
        timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
        timer.AutoReset = false;
        timer.Enabled = true;

        // 数据库
        string connStr = "Database=" + databaseName + ";Data Source=127.0.0.1;";
        connStr += "User Id=root;Password=00000000;port=3306";
        sqlConn = new MySqlConnection(connStr);
        try {
            sqlConn.Open();
        } catch (Exception e) {
            Console.WriteLine("[数据库] 连接失败 " + e.Message);
        }
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

    // Accept回调
    private void AcceptCb(IAsyncResult ar)
    {
        try {
            Socket socket = listenFd!.EndAccept(ar);
            int index = NewIndex();

            if (index < 0)
            {
                socket.Close();
                Console.WriteLine("[警告]连接已满");
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
        lock (conn) 
        {
            try 
            {
                int count = conn.socket!.EndReceive(ar);
                // 关闭信号
                if (count <= 0) {
                    Console.WriteLine("收到[" + conn.GetAddress() + "] 断开连接");
                    conn.Close();
                    return;
                }
                conn.buffCount += count;
                ProcessData(conn);
                // 继续接收
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
            } catch (Exception e) 
            {
                Console.WriteLine("收到[" + conn.GetAddress() + "] 断开连接 信息:"+e.Message );
                conn.Close();
            }
        }
    }

    // 处理数据
    private void ProcessData(Conn conn)
    {
        // 小于长度字节
        if (conn.buffCount < sizeof(Int32)) return;

        // 消息长度
        Array.Copy(conn.readBuff, conn.lenBytes, sizeof(Int32));
        conn.msgLength = BitConverter.ToInt32(conn.lenBytes, 0);
        // 如果消息不足一组消息
        if (conn.buffCount < conn.msgLength + sizeof(Int32)) return;
        // 处理消息
        ProtocolBase newProtocol = protocol.Decode(conn.readBuff, sizeof(Int32), conn.msgLength);
        HandleMsg(conn, newProtocol);
        

        // 清除已发送的消息
        int count = conn.buffCount - conn.msgLength - sizeof(Int32);
        Array.Copy(conn.readBuff, sizeof(Int32) + conn.msgLength, conn.readBuff, 0, count);
        conn.buffCount = count;
        if (conn.buffCount > 0)
        {
            ProcessData(conn);
        }

    }

    private void HandleMsg(Conn conn, ProtocolBase protocolBase)
    {
        string name = protocolBase.GetName();
        Console.WriteLine("[收到协议]" + name);
        string methodName = "Msg" + name;

        // 连接协议分发
        if (conn.player == null || name == "HeatBeat" || name == "Logout")
        {
            MethodInfo? methodInfo = handleConnMsg.GetType().GetMethod(methodName);
            if (methodInfo == null) {
                string str = "[警告⚠️]HandleMsg 没有处理连接方法 ";
                Console.WriteLine(str + methodName);
                return;
            }
            Object[] obj = new object[]{conn, protocolBase};
            Console.WriteLine("[处理连接消息]" + conn.GetAddress() + " : " + name);
            methodInfo.Invoke(handleConnMsg, obj);
        }
        // 角色协议分发
        else 
        {
            MethodInfo? methodInfo = handlePlayerMsg.GetType().GetMethod(methodName);
            if (methodInfo == null) {
                string str = "[警告⚠️]HandleMsg 没有处理玩家方法 ";
                Console.WriteLine(str + methodName);
                return;
            }
            Object[] obj = new Object[]{conn.player, protocolBase};
            Console.WriteLine("[处理玩家消息]" + conn.player.id + " : " + name);
            methodInfo.Invoke(handlePlayerMsg, obj);
        }
    }

    // 发送
    public void Send(Conn conn, ProtocolBase protocol) 
    {
        byte[] bytes = protocol.Encode();
        byte[] length = BitConverter.GetBytes(bytes.Length);
        byte[] sendBuff = length.Concat(bytes).ToArray();
        try 
        {
            conn.socket!.BeginSend(sendBuff, 0, sendBuff.Length, SocketFlags.None, null, null);
        } catch (Exception e) {
            Console.WriteLine("[发送消息] " + conn.GetAddress() + " : " + e.Message);
        }
    }

    // 广播📢
    public void Broadcast(ProtocolBase protocol)
    {
        for (int i = 0; i < conns.Length; i++) 
        {
            if(!conns[i].isUse) continue;
            if (conns[i].player == null) return;
            Send(conns[i], protocol);
        }
    }
    // 关闭
    public void Close()
    {
        for (int i = 0; i < conns.Length; i++)
        {
            Conn conn = conns[i];
            if (conn == null) continue;
            if (!conn.isUse) continue;
            lock(conn)
            {
                conn.Close();
            }
        }
    }

    /// -------------------- 心跳检测 -------------------------
    // 主定时器
    System.Timers.Timer timer = new System.Timers.Timer(1000);
    // 心跳时间
    public long heartBeatTime = 180;

    // 主定时器
    public void HandleMainTimer(object? sender, System.Timers.ElapsedEventArgs e) 
    {
        // 处理心跳
        HeartBeat();
        timer.Start();
    }

    // 心跳
    public void HeartBeat()
    {
        // Console.WriteLine("[主定时器执行]");
        long timeNow = Sys.GetTimeStamp();
        for (int i = 0; i < conns.Length; i++)
        {
            Conn conn = conns[i];
            if (conn == null) continue;
            if (!conn.isUse) continue;
            if (conn.lastTickTime < timeNow - heartBeatTime)
            {
                Console.WriteLine("[心跳引起断开连接] " + conn.GetAddress());
                lock(conn)
                    conn.Close();
            }
        }
    }

    // 消息分发
    public HandleConnMsg handleConnMsg = new HandleConnMsg();
    public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();
    public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent();




    
    
}
