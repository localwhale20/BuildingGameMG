using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using System.Linq;

namespace BuildingGame.Util;

public static class GameUtils {
    public static string GetVersion(){
        var ver = Assembly.GetExecutingAssembly().GetName().Version;
        return $"{ver.Major}.{ver.Minor}";
    }

    public static Rectangle GetSpriteBounds(int x, int y) {
        return new Rectangle(x * Game.TileSize, y * Game.TileSize, Game.TileSize, Game.TileSize);
    }

    public static Point CalculatePreviewSize(TileType tile) {
        var scale = new Point(tile.SpriteSize.X / Game.TileSize, tile.SpriteSize.Y / Game.TileSize);
        return scale;
    }

    public static Color GetRandomColor(){
        var rnd = new Random();
        return new Color(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
    }

    public static bool IsNumericType(this object o)
    {   
        switch (Type.GetTypeCode(o.GetType()))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }
}