using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class NetworkManager
{
    private TcpClient client;
    private TcpListener server;
    private NetworkStream stream;
    private byte[] buffer;

    public bool IsServer { get; private set; }
    public bool IsConnected { get; private set; }

    public async Task StartServer(int port)
    {
        server = new TcpListener(IPAddress.Any, port);
        server.Start();
        IsServer = true;

        Console.WriteLine("Server started. Waiting for connection...");
        client = await server.AcceptTcpClientAsync();
        stream = client.GetStream();
        buffer = new byte[1024];
        IsConnected = true;

        Console.WriteLine("Client connected.");
        _ = Task.Run(() => ReceiveData());
    }

    public async Task ConnectToServer(string ip, int port)
    {
        client = new TcpClient();
        await client.ConnectAsync(ip, port);
        stream = client.GetStream();
        buffer = new byte[1024];
        IsConnected = true;

        Console.WriteLine("Connected to server.");
        _ = Task.Run(() => ReceiveData());
    }

    public async Task SendData(string message)
    {
        if (IsConnected)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);
        }
    }

    private async Task ReceiveData()
    {
        while (IsConnected)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received: " + message);
                // Process the received data
            }
        }
    }

    public void Disconnect()
    {
        if (IsConnected)
        {
            stream.Close();
            client.Close();
            IsConnected = false;

            if (IsServer)
            {
                server.Stop();
                IsServer = false;
            }

            Console.WriteLine("Disconnected.");
        }
    }
}