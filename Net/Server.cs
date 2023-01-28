using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BuildingGame.Net.Packets;
using BuildingGame.Util;
using Serilog;

namespace BuildingGame.Net;

public class Server : PacketSubscriber, IDisposable
{
    public static Server Empty => new Server();
    public static short Port => 1928;
    public bool Started { get; private set; }
    private Socket listener;
    private IPEndPoint endPoint;
    private List<Socket> players = new();

    ~Server() {
        Dispose();
    }

    private void SubscribePayloads(){
        // SubscribePacket(PacketType.PlayerConnect, async packet => {
        //     using MemoryStream ms = new(packet.Payload);
        //     using BinaryReader br = new(ms);

        //     var ip       = IPAddress.Parse(br.ReadString());
        //     var state    = br.ReadBoolean();
        //     var player = players.Find(s => ((IPEndPoint) s.LocalEndPoint).Address == ip);

        //     if (state && player != null){
        //         Log.Debug("Player " + player.LocalEndPoint + " has been conncted");
        //         Log.Debug("Sending tiles to player...");
        //         foreach (var tile in Game.Tiles){
        //             await player.SendAsync(Packet.Serialize(PacketMaker.CreateTileStatePacket(tile.Position, tile.Type.GetIndex())), SocketFlags.None);
        //         }
        //     }
        //     else if (player != null){
        //         Log.Debug("Player " + player.LocalEndPoint + " has been disconnected");
        //     }
        // });
    }

    private Server() { }

    private Server(Socket listener, IPEndPoint endPoint){
        this.listener = listener;
        this.endPoint = endPoint;
        SubscribePayloads();
    }

    public static Server Create(){
        var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);
        var server = new Server(
            new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp),
            endPoint
        );
        Log.Debug("Server will be hosted at " + endPoint);

        return server;
    }

    public void Init(){
        listener.Bind(endPoint);
        listener.Listen(100);
        Started = true;
        Log.Debug("Server listening at " + endPoint);
    }
    
    public async void UpdateAsync(){
        var connect = await listener.AcceptAsync();
        players.Add(connect);
        foreach (var tile in Game.Tiles){
            await connect.SendAsync(Packet.Serialize(PacketMaker.CreateTileStatePacket(tile.Position, tile.Type.GetIndex())), SocketFlags.None);
        }
        
        foreach (var player in players){
            var buffer = new byte[Packet.PacketSize];
            await player.ReceiveAsync(buffer, SocketFlags.None);
            var packet = Packet.Deserialize(buffer);

            Log.Debug(packet.Type.ToString());

            InvokePacketHandler(packet);

            foreach (var player1 in players){
                Log.Debug("Resending " + packet.Type);
                await player.SendAsync(Packet.Serialize(packet), SocketFlags.None);
            }
        }
    }

    public void Dispose()
    {
        Started = false;
        Log.Debug("Stopping server...");
    }
}