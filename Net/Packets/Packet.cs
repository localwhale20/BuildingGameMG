using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BuildingGame.Net.Packets;

public class Packet {
    public static ushort PacketSize => 1_024 * 4;
    public PacketType Type { get; set; }
    public ushort Length { get; set; }
    public byte[] Payload { get; set; }

    public Packet(PacketType type = PacketType.Empty){
        Type = type;
        Length = PacketSize;
        Payload = new byte[1_024];
    }

    public static byte[] Serialize(Packet packet){
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);

        bw.Write((byte) packet.Type);
        foreach (var @byte in packet.Payload) bw.Write(@byte);

        return ms.ToArray();
    }

    public static Packet Deserialize(byte[] buffer){
        using MemoryStream ms = new(buffer);
        using BinaryReader br = new(ms);

        Packet packet = new();
        packet.Type = (PacketType) br.ReadByte();
        packet.Payload = br.ReadBytes(PacketSize);

        return packet;
    }
}