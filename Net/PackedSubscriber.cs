using System.Collections.Generic;
using System;
using System.Linq;
using BuildingGame.Net.Packets;
using System.Diagnostics;
using Serilog;

namespace BuildingGame.Net;

public class PacketSubscriber {
    public delegate void PacketDelegate(Packet packet);
    private Dictionary<PacketType, PacketDelegate> packetHandlers = new();

    protected bool CheckIfPayloadRegistered(Packet packet) => 
        packetHandlers.Keys.ToList().Contains(packet.Type);
    
    protected void InvokePacketHandler(Packet packet){
        Log.Information("Packet " + packet.Type + " received");
        
        if (CheckIfPayloadRegistered(packet)){
            var type = packet.Type;
            packetHandlers[packetHandlers.Keys.ToList().Find(p => p == type)].Invoke(packet);
        }
    }

    public void SubscribePacket(PacketType type, PacketDelegate payloadDelegate){
        Log.Information("Subscribed packet of type " + type);
        packetHandlers.Add(type, payloadDelegate);
    }
}