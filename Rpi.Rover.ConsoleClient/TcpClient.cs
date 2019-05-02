using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Rpi.Rover.ConsoleClient
{
    public class MyTcpClient
    {
        NetworkStream stream;
        TcpClient client;
        Int32 port = 5050;
        string server;

        public MyTcpClient(string server)
        {
            this.server = server;
        }

        public void Open()
        {
            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer 
            // connected to the same address as specified by the server, port
            // combination.
            if (client == null || !client.Connected)
            {
                while (client == null || !client.Connected)
                {
                    try
                    {
                        client = new TcpClient(server, port);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Thread.Sleep(2000);
                    }
                }
                stream = client.GetStream();
            }
        }

        public void Close()
        {
            stream.Close();
            client.Close();
        }

        public void Connect(String message)
        {
            try
            {

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                // NetworkStream stream = client.GetStream();
                Open();
                // try
                // {
                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                // Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                // Int32 bytes = stream.Read(data, 0, data.Length);
                // responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                // Console.WriteLine("Received: {0}", responseData);
                //     }
                //     catch (Exception ex)
                //     {
                //         Console.WriteLine($"Stream exception {ex.Message}");
                //         stream.Close();
                //         stream.Dispose();
                //         if (client.Connected)
                //         {
                //             client.Close();
                //         }
                //         client.Dispose();
                //     }
                // }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException: {0}", ex);
                Console.WriteLine($"Stream exception {ex.Message}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stream exception {ex.Message}");

                stream.Close();
                stream.Dispose();
                stream = null;

                if (client.Connected)
                {
                    client.Close();
                }
                client.Dispose();                
                client = null;                
            }

            // Console.WriteLine("\n Press Enter to continue...");
            // Console.Read();
        }
    }
}