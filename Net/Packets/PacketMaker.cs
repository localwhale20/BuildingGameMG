using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using BuildingGame;
using BuildingGame.Util;
using Microsoft.Xna.Framework;

namespace BuildingGame.Net.Packets;

public static class PacketMaker {
    public static unsafe Packet CreatePlayerStatePacket(string ip, bool state) {
        var packet = new Packet(PacketType.PlayerConnect);

        using MemoryStream ms = new(packet.Payload);
        using BinaryWriter bw = new(ms);

        bw.Write(ip);
        bw.Write(state);

        return packet;
    }
    
    public static Packet CreateTileStatePacket(Vector2 pos, int type){
        var packet = new Packet(PacketType.TileAction);

        using MemoryStream ms = new(packet.Payload);
        using BinaryWriter bw = new(ms);

        bw.Write(pos.X);
        bw.Write(pos.Y);
        bw.Write(type);

        return packet;
    }
}