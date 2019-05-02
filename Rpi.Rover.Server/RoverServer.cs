using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class RoverServer
{

    TcpClient client;

    public void Listen(Action<string> roverActions)
    {
        TcpListener server = null;
        try
        {
            IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Any, 5050);
            server = new TcpListener(ipLocalEndPoint);
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;

            while (true)
            {
                data = null;

                client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                try
                {
                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        roverActions(data);

                        // TODO - may use reply for some telemetry otherwise will delete

                        data = data.ToUpper();
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                        stream.Write(msg, 0, msg.Length);

                    }
                }
                catch {
                    // to connection broken so stop the motors
                    roverActions("0");
                 }
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            // Stop listening for new clients.
            server.Stop();
        }
    }
}