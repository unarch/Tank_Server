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
    static string str = "";

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello xx");
        Thread t1 = new Thread(Add1);
        t1.Start();
        Thread t2 = new Thread(Add2);
        t2.Start();
        Thread.Sleep(1000);
        Console.WriteLine(str);

    }
    // 线程1
    public static void Add1()
    {
        lock(str)
        {
            for (int i = 0; i < 20; i++)
            {
                Thread.Sleep(10);
                str += "A";
            }
        }
       
    }

    public static void Add2()
    {
        lock(str)
        {
            for (int i = 0; i < 20; i++)
            {
                Thread.Sleep(10);
                str += "B";
            }
        }
    }

    
    public static void CreateTimer()
    {

        System.Timers.Timer timer = new System.Timers.Timer();
        timer.AutoReset = true;
        timer.Interval = 1000;
        timer.Elapsed += new ElapsedEventHandler(Tick);
        timer.Start();

        Console.Read();
    }

    public static void Tick(object sender, System.Timers.ElapsedEventArgs e)
    {
        Console.WriteLine("每秒执行一次");
    }


    // 反序列化
    void Deserialize()
    {
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream("data.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
        Player player = (Player)formatter.Deserialize(stream);
        stream.Close();
        // 输出验证
        Console.WriteLine("coin : {0}", player.coin);
        Console.WriteLine("money : {0}", player.money);
        Console.WriteLine("name : {0}", player.name);
    }
    // 序列化
    void Serialize()
    {
        Player player = new Player();
        player.coin = 1;
        player.money = 10;
        player.name = "XiaoMing";

        IFormatter formatter= new BinaryFormatter();
        Stream stream = new FileStream("data.bin", FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, player);
        stream.Close();
    }

    // 开启服务器
    void OpenService()
    {
        Service service = new Service();
        service.Start("127.0.0.1", 1234);
        while (true) {
            string? str = Console.ReadLine();
            switch(str!) {
                case "quit": return;
            }
        }
    }
}

