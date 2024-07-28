using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Serilog;

public class NetworkManager
{
    private UdpClient client;
    private IPEndPoint serverEndpoint;
    private byte[] buffer;

    public event Action<string> OnMessageReceived;

    public async Task StartClient(string ip, int port)
    {
        client = new UdpClient();
        serverEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        buffer = new byte[1024];
        Log.Information("Client started.");
        _ = Task.Run(() => ReceiveData());
    }

    public async Task SendData(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        await client.SendAsync(data, data.Length, serverEndpoint);
        Log.Information("Sent to server: {Message}", message);
    }

  private async Task ReceiveData()
{
    while (true)
    {
        try
        {
            var result = await client.ReceiveAsync();
            string message = Encoding.ASCII.GetString(result.Buffer);
            Log.Information("Received: {Message}", message);
            OnMessageReceived?.Invoke(message);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in ReceiveData");
        }
    }
}

    public void Disconnect()
    {
        if (client != null)
        {
            client.Close();
            client = null;
            Log.Information("Client disconnected.");
        }
    }
}