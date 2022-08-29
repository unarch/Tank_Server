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

        Service service = new Service();
        service.Start("127.0.0.1", 1234);
        while (true) {
            string str = Console.ReadLine();
            switch(str) {
                case "quit": return;
            }
        }
    }
}