using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SimpleGameServer
{
    public class Server
    {
        private EventBasedNetListener listener;
        private NetManager server;
        private Dictionary<int, Vector2> playerPositions;

        public Server()
        {
            listener = new EventBasedNetListener();
            server = new NetManager(listener);
            playerPositions = new Dictionary<int, Vector2>();
        }

        public void Start()
        {
            server.Start(9050);

            listener.ConnectionRequestEvent += request =>
            {
                request.AcceptIfKey("game_key");
            };

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine($"Player connected: {peer.EndPoint}");
                playerPositions[peer.Id] = new Vector2(100, 100); // Initial position for new players
            };

            listener.NetworkReceiveEvent += (fromPeer, reader) =>
            {
                var messageType = (MessageType)reader.GetByte();

                if (messageType == MessageType.PlayerMovement)
                {
                    var x = reader.GetFloat();
                    var y = reader.GetFloat();
                    playerPositions[fromPeer.Id] = new Vector2(x, y);
                }

                BroadcastGameState();
            };

            listener.PeerDisconnectedEvent += (peer, info) =>
            {
                Console.WriteLine($"Player disconnected: {peer.RemoteEndPoint}");
                playerPositions.Remove(peer.Id);
            };
        }

        private void BroadcastGameState()
        {
            foreach (var peer in server.ConnectedPeerList)
            {
                var writer = new NetDataWriter();
                writer.Put((byte)MessageType.GameState);

                writer.Put(playerPositions.Count);
                foreach (var player in playerPositions)
                {
                    writer.Put(player.Key);
                    writer.Put(player.Value.X);
                    writer.Put(player.Value.Y);
                }

                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }

        public void Run()
        {
            while (!Console.KeyAvailable)
            {
                server.PollEvents();
                System.Threading.Thread.Sleep(15);
            }
        }

        static void Main(string[] args)
        {
            var server = new Server();
            server.Start();
            server.Run();
        }
    }

    public enum MessageType
    {
        PlayerMovement,
        GameState
    }
}