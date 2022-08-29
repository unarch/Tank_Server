using System.Security.AccessControl;
using System.IO;
using System.Net.Sockets;
using System;
using System.Net;
class MainClass
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello xx");
        // Socket
        Socket listenFd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // Bind
        IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
        IPEndPoint ipEp = new IPEndPoint(ipAdr, 1234);
        listenFd.Bind(ipEp);
        // Listen
        listenFd.Listen(0);
        Console.WriteLine("[服务器]启动成功");
        while (true) 
        {
            // Accept
            Socket connFd = listenFd.Accept();
            Console.WriteLine("[服务器]Accept");
            // Recv
            byte[] readBuff = new byte[1024];
            int count = connFd.Receive(readBuff);
            // string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
            string str = System.DateTime.Now.ToString();
            Console.WriteLine("[服务器接收]" + str);
            // Send
            byte[] bytes = System.Text.Encoding.Default.GetBytes("service echo " + str);
            connFd.Send(bytes);
        }
    }
}