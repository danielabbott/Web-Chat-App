using System;
using System.Collections.Generic;

namespace ChatServer
{
    class Program
    {
        public static DB db;
        public static WSServer server;
        static void Main(string[] args)
        {
            db = new DB("chat");

            server = new WSServer(25566);
            server.StartListening();

            while(true)
            {
                Console.Write("Enter command (c/clear, q/quit, l/list, t/talk): ");

                string cmd = Console.ReadLine();
                string s = cmd.ToLower();

                if (s.Equals("c") || s.Equals("clear")) {
                    db.wipe();
                }
                else if (s.Equals("q") || s.Equals("quit"))
                {
                    break;
                }
                else if (s.Equals("l") || s.Equals("list"))
                {
                    List<string> names = server.getAllNicknames();
                    foreach(string n in names)
                    {
                        Console.WriteLine(n);
                    }
                }
                else if (s.StartsWith("t "))
                {
                    string msg = cmd.Substring(2, cmd.Length - 2);
                    if(msg.StartsWith("[") || msg.StartsWith("!"))
                    {
                        msg = ' ' + msg;
                    }
                    server.Broadcast(msg);
                }
                else if(s.StartsWith("talk "))
                {
                    string msg = cmd.Substring(5, cmd.Length - 5);
                    if (msg.StartsWith("[") || msg.StartsWith("!"))
                    {
                        msg = ' ' + msg;
                    }
                    server.Broadcast(msg);
                }
                else
                {
                    Console.WriteLine("Unrecognised command");
                }
            }

            db.close();

        }
    }
}
