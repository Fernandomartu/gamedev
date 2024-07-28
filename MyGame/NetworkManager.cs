using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class NetworkManager
{
    private UdpClient udpClient;
    private IPEndPoint serverEndPoint;
    public event Action<string> OnMessageReceived;

    public Task ConnectToServer(string ip, int port)
    {
        udpClient = new UdpClient();
        serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        _ = ListenForMessages();  // No need for await here
        return Task.CompletedTask;
    }

    public Task SendData(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        return udpClient.SendAsync(data, data.Length, serverEndPoint);  // Directly return the Task
    }

    private async Task ListenForMessages()
    {
        while (true)
        {
            var result = await udpClient.ReceiveAsync();
            var message = Encoding.ASCII.GetString(result.Buffer);
            OnMessageReceived?.Invoke(message);
        }
    }

    public void Disconnect()
    {
        udpClient.Close();
    }
}