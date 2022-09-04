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
        DataMgr dataMgr = new DataMgr();
        ServeNet serveNet = new ServeNet();
        Scene scene = new Scene();
        serveNet.Start("127.0.0.1", 1234);
        // Console.ReadLine();
        while(true) {
            string? str = Console.ReadLine();
            switch(str)
            {
                case "quit":
                    serveNet.Close();
                    return;
                case "print":
                    serveNet.Print();
                    break;
            }
        }
    }

}

