using System.IO;
using Microsoft.Xna.Framework;

namespace BuildingGame;

public static class Extensions {
    public static void Write(this BinaryWriter bw, Vector2 pos){
        bw.Write(pos.X);
        bw.Write(pos.Y);
    }
    public static void Write(this BinaryWriter bw, TileType tileType){
        bw.Write(TileType.TileTypes.IndexOf(tileType));
    }
}