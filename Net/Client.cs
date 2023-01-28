// wenomechinsama

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using BuildingGame.Net.Packets;
using System;
using Microsoft.Xna.Framework;
using BuildingGame.Util;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using System.IO;

namespace BuildingGame.Net;

public class Client : PacketSubscriber, IDisposable{
    public static Client Empty => new Client();
    public static short Port => Server.Port;
    public string Username { get; private set; }
    private Socket listener;
    public IPEndPoint EndPoint { get; private set; }
    public bool Connected { get; set; }

    ~Client(){
        Dispose();
    }

    private void SubscribePayloads(){
        SubscribePacket(PacketType.TileAction, packet => {
            using MemoryStream ms = new(packet.Payload);
            using BinaryReader br = new(ms);

            var pos = new Vector2(br.ReadSingle(), br.ReadSingle());
            var type = br.ReadInt32();

            if (type == -1) Game.Tiles.Remove(Game.Tiles.Find(t => t.Position == pos));
            else Game.Tiles.Add(new(pos, type));
        });
    }

    private Client() {}

    private Client(Socket listener, IPEndPoint endPoint, string username){
        this.listener = listener;
        this.EndPoint = endPoint;
        this.Username = username;
        SubscribePayloads();
    }

    

    public static Client Create(IPEndPoint endPoint, string username){
        var client = new Client(
            new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp),
            endPoint,
            username
        );
        
        Log.Debug("Client ready to use");
        
        return client;
    }

    public async Task ConnectAsync(){
        listener.Connect(EndPoint);
        var packet = PacketMaker.CreatePlayerStatePacket(listener.LocalEndPoint.ToString(), true);
        await SendToServerAsync(packet);
        Log.Debug("Trying connect to " + EndPoint + "...");
        Connected = true;
    }

    public async void UpdateAsync(){
        var buffer = new byte[Packet.PacketSize];
        await listener.ReceiveAsync(buffer, SocketFlags.None);

        var packet = Packet.Deserialize(buffer);
        InvokePacketHandler(packet);
    }

    public async Task SendToServerAsync(Packet packet){
        await listener.SendAsync(Packet.Serialize(packet), SocketFlags.None);
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Connected = false;
        listener.Close();
        Log.Debug("Client is shutting down...");
    }
}