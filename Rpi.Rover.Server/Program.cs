using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Net;
using System.Text;
using Glovebox.Graphics;
using Glovebox.Graphics.Components;
using Glovebox.Graphics.Drivers;
using Glovebox.Graphics.LedType;
using Glovebox.Graphics.Font;
using System.Drawing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;


namespace Rpi.listen
{
    class Program
    {
        enum MotorMap : byte
        {
            TwoPlus = 21,
            TwoMinus = 26,
            OnePlus = 19,
            OneMinus = 20
        }

        enum MotorControl
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

        public enum Symbols : ulong
        {
            Heart = 0x00081C3E7F7F3600,
            HappyFace = 0x3C4299A581A5423C,
            SadFace = 0x3C42A59981A5423C,
            Block = 0xffffffffffffffff,
            HourGlass = 0xFF7E3C18183C7EFF,
            UpArrow = 0x18181818FF7E3C18,
            DownArrow = 0x183C7EFF18181818,
            RightArrow = 0x103070FFFF703010,
            LeftArrow = 0x080C0EFFFF0E0C08,
        }

        static RoverServer tcp = new RoverServer();
        static GpioController controller = new GpioController();

        static Motor left = new Motor(controller, (int)MotorMap.TwoPlus, (int)MotorMap.TwoMinus);
        static Motor right = new Motor(controller, (int)MotorMap.OnePlus, (int)MotorMap.OneMinus);

        static MqttClient client = new MqttClient("localhost");

        static Ht16K33BiColor driver = new Ht16K33BiColor(new byte[] { 0x71 });
        static LED8x8Matrix matrix = new LED8x8Matrix(driver, Fonts.CP437);

        private static readonly string userId = $"User {new Random().Next(1, 99)}";
        private static ServiceUtils serviceUtils;
        private static readonly string hubName = "Rover";
        private static HubConnection hubConnection;


        static async Task  Main(string[] args)
        {
            matrix.Brightness = 1;
            matrix.Blink = LedDriver.BlinkRate.Slow;
            driver.Write(new ulong[] { 0 }, new ulong[] { (ulong)Symbols.Heart });

            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddUserSecrets<Program>()
               .Build();

            serviceUtils = new ServiceUtils(configuration["Azure:SignalR:ConnectionString"]);

            var url = $"{serviceUtils.Endpoint}:5001/client/?hub={hubName}";

            hubConnection = new HubConnectionBuilder()
                .WithUrl(url, option =>
                {
                    option.AccessTokenProvider = () =>
                    {
                        return Task.FromResult(serviceUtils.GenerateAccessToken(url, userId));
                    };
                }).Build();

            hubConnection.On<string, string>("SendMessage",
                (string server, string message) =>
                {
                    Console.WriteLine($"Message from server {server}: {message}");
                    roverActions(message);
                });
            
            await hubConnection.StartAsync();

            try
            {
                client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                string clientId = Guid.NewGuid().ToString();
                client.Connect(clientId);
                client.Subscribe(new string[] { "/rover/motor" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
            catch
            {
                Console.WriteLine("Check Mosquitto installed on Raspberry Pi. sudo apt install mosquitto");
            }

            tcp.Listen(roverActions);
        }

        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string action = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
            roverActions(action);
        }

        static void roverActions(string action)
        {
            int cmd;

            matrix.Blink = LedDriver.BlinkRate.Off;

            // Console.WriteLine(action);
            if (int.TryParse(action, out cmd))
            {
                switch ((MotorControl)cmd)
                {
                    case MotorControl.Stop: // stop
                        left.Stop();
                        right.Stop();
                        matrix.Blink = LedDriver.BlinkRate.Slow;
                        driver.Write(new ulong[] { 0 }, new ulong[] { (ulong)Symbols.Heart });
                        break;
                    case MotorControl.Forward: // forward
                        left.Forward();
                        right.Forward();
                        driver.Write(new ulong[] { (ulong)Symbols.DownArrow }, new ulong[] { 0 });
                        break;
                    case MotorControl.LeftForward: // left
                        left.Stop();
                        right.Forward();
                        driver.Write(new ulong[] { (ulong)Symbols.LeftArrow }, new ulong[] { (ulong)Symbols.LeftArrow });
                        break;
                    case MotorControl.RightForward: // right
                        left.Forward();
                        right.Stop();
                        driver.Write(new ulong[] { (ulong)Symbols.RightArrow }, new ulong[] { (ulong)Symbols.RightArrow });
                        break;
                    case MotorControl.LeftBackward: // leftbackward
                        left.Stop();
                        right.Backward();
                        break;
                    case MotorControl.RightBackward: // right backward
                        left.Backward();
                        right.Stop();
                        break;
                    case MotorControl.Backward:
                        left.Backward();
                        right.Backward();
                        driver.Write(new ulong[] { 0 }, new ulong[] { (ulong)Symbols.UpArrow });
                        break;
                    case MotorControl.SharpLeft: // sharpleft
                        left.Forward();
                        right.Backward();
                        driver.Write(new ulong[] { 0 }, new ulong[] { (ulong)Symbols.RightArrow });
                        break;
                    case MotorControl.SharpRight: //sharpright
                        left.Backward();
                        right.Forward();
                        driver.Write(new ulong[] { 0 }, new ulong[] { (ulong)Symbols.LeftArrow });
                        break;
                    case MotorControl.ShutDown:
                        ShutDown();
                        break;
                }
            }
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
