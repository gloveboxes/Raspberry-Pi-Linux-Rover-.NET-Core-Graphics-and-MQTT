using System;
using System.Collections.Generic;
using System.Threading;

namespace Rpi.Rover.ConsoleClient
{
    class Program
    {
        static Queue<string> messageQueue = new Queue<string>();
        static Motor lastCommand = Motor.Unknown;

        private static AutoResetEvent sync = new AutoResetEvent(false);

        enum Motor
        {
            Stop,
            Forward,
            LeftForward,
            RightForward,
            LeftBackward,
            RightBackward,
            Backward,
            SharpLeft,
            SharpRight,
            ShutDown,
            Unknown
        }

        static MyTcpClient client = new MyTcpClient("rpirover.local");

        static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                client.Close();
            };

            client.Open();

            ThreadStart threadDelegate = new ThreadStart(ProcessMessages);
            Thread newThread = new Thread(threadDelegate);
            newThread.Start();

            while (true)
            {

                var consoleKey = Console.ReadKey(true);

                switch (consoleKey.Key)
                {
                    case ConsoleKey.DownArrow:
                        Console.WriteLine("Backwards");
                        QueueMessage(Motor.Backward);
                        break;
                    case ConsoleKey.UpArrow:
                        Console.WriteLine("Forwards");
                        QueueMessage(Motor.Forward);
                        break;
                    case ConsoleKey.LeftArrow:
                        if (consoleKey.Modifiers == ConsoleModifiers.Control)
                        {
                            Console.WriteLine("Sharp Left");
                            QueueMessage(Motor.SharpLeft);
                        }
                        else
                        {
                            Console.WriteLine("Left");
                            QueueMessage(Motor.RightForward);
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (consoleKey.Modifiers == ConsoleModifiers.Control)
                        {
                            Console.WriteLine("Sharp Right");
                            QueueMessage(Motor.SharpRight);
                        }
                        else
                        {
                            Console.WriteLine("Right");
                            QueueMessage(Motor.LeftForward);
                        }
                        break;
                    default:
                        switch (consoleKey.KeyChar)
                        {
                            case ' ':
                                Console.WriteLine("Stop");
                                QueueMessage(Motor.Stop);
                                break;

                            case 's':
                                Console.WriteLine("Shutdown Raspberry Pi");
                                QueueMessage(Motor.ShutDown);
                                Environment.Exit(0);
                                break;
                        }
                        break;
                }
            }
        }

        static void QueueMessage(Motor cmd)
        {
            if (cmd == lastCommand) { return; }

            lastCommand = cmd;

            messageQueue.Clear();

            messageQueue.Enqueue(((int)cmd).ToString());

            sync.Set();
        }

        static void ProcessMessages()
        {
            while (true)
            {
                sync.WaitOne();
                string msg;

                if (messageQueue.TryDequeue(out msg))
                {
                    client.Connect(msg);
                }
            }
        }
    }
}
