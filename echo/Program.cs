using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.IO;
using System.Net.Sockets;
using System;
using System.Net;
using System.Timers;


class MainClass
{


    public static void Main(string[] args)
    {
        ServeNet serveNet = new ServeNet();
        serveNet.Start("127.0.0.1", 1234);
        Console.ReadLine();
    }

}

