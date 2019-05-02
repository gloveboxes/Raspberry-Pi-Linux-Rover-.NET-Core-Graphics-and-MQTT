using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Emmellsoft.IoT.Rpi.SenseHat;
using RichardsTech.Sensors;

namespace Rpi.Rover.Client
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
        static ISenseHat senseHat = SenseHatFactory.GetSenseHat();
        static bool motorRunning = false;  // false = off;

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
                if (!senseHat.Sensors.ImuSensor.Update())
                {
                    continue;
                }

                if (!senseHat.Sensors.Acceleration.HasValue)
                {
                    continue;
                }

                if (senseHat.Joystick.Update())
                {
                    if (senseHat.Joystick.EnterKey == KeyState.Pressing)
                    {
                        QueueMessage(Motor.ShutDown);
                    }
                }

                Image colors = CreateGravityBlobScreen(senseHat.Sensors.Acceleration.Value);

                senseHat.Display.CopyColorsToScreen(colors);

                senseHat.Display.Update();

                SetMotorDirection(senseHat.Sensors.Acceleration.Value);

                Thread.Sleep(50);
            }
        }

        private static void SetMotorDirection(Vector3 vector)
        {


            if (vector.X < -0.55)
            {
                QueueMessage(Motor.SharpLeft);
            }
            else if (vector.X > 0.55)
            {
                QueueMessage(Motor.SharpRight);
            }
            else if (vector.X < -0.25)
            {
                QueueMessage(Motor.RightForward);
            }
            else if (vector.X > 0.25)
            {
                QueueMessage(Motor.LeftForward);
            }
            else if (vector.Y < -0.25)
            {
                QueueMessage(Motor.Forward);
            }
            else if (vector.Y > 0.25)
            {
                messageQueue.Enqueue(((int)Motor.Backward).ToString());
                QueueMessage(Motor.Backward);
            }
            else
            {
                QueueMessage(Motor.Stop);
            }
        }

        static void QueueMessage(Motor cmd)
        {
            if (cmd == lastCommand) { return; }

            lastCommand = cmd;

            messageQueue.Clear();

            messageQueue.Enqueue(((int)cmd).ToString());

            motorRunning = cmd != Motor.Stop;

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

                    // if shutdown msg sense to Rover Server then also shut down this client
                    var cmd = (Motor) Enum.Parse(typeof(Motor), msg);
                    if (cmd == Motor.ShutDown)
                    {
                        ShutDown();
                    }
                }
            }
        }

        private static Image CreateGravityBlobScreen(Vector3 vector)
        {
            double x0 = (vector.X + 1) * 5.5 - 2;
            double y0 = (vector.Y + 1) * 5.5 - 2;
            double distScale = 4;

            var screen = new Image(8, 8);

            bool isUpsideDown = vector.Z < 0;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    double dx = x0 - x;
                    double dy = y0 - y;

                    double dist = Math.Sqrt(dx * dx + dy * dy) / distScale;
                    if (dist > 1)
                    {
                        dist = 1;
                    }

                    int colorIntensity = (int)Math.Round(255 * (1 - dist));
                    if (colorIntensity > 255)
                    {
                        colorIntensity = 255;
                    }

                    screen[x, y] = isUpsideDown
                        ? Color.FromArgb(255, (byte)colorIntensity, 0, 0)
                        : Color.FromArgb(255, 0, (byte)colorIntensity, 0);
                }
            }

            return screen;
        }

        static void ShutDown()
        {
            string result;

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"/bin/bash";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.Arguments = "-c \"sudo halt\"";

            try
            {
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
