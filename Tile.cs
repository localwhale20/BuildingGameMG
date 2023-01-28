using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildingGame;

public class Tile {
    public TileType Type { get; set; }
    public Vector2 Position { get; set; }
    

    public Tile(Vector2 pos, TileType type){
        Position = pos;
        Type = type;
    }

    public void Draw(SpriteBatch spriteBatch) =>
        spriteBatch.Draw(
            Game.SpriteSheet, 
            new Rectangle(Position.ToPoint(), Type.SpriteSize), 
            Type.GetAtlasBounds(), 
            Color.White
        );
}
public class TileType {
    public static List<TileType> TileTypes { get => new List<TileType>() {
        new (0, 0),
        new (1, 0),
        new (2, 0),
        new (3, 0),
        new (4, 0),
        new (5, 0),
        new (6, 0),
        new (7, 0),
        new (8, 0),
        new (9, 0, 16, 32),
    }; }

    public Point AtlasPosition { get; set; }
    public Point SpriteSize { get; set; }
    public bool Replaceable { get; set; }

    public TileType(Point atlasPosition, bool replaceable = true) {
        AtlasPosition = atlasPosition;
        Replaceable = replaceable;
        SpriteSize = new Point(Game.TileSize, Game.TileSize);
    }
    public TileType(Point atlasPosition, Point spriteSize, bool replaceable = true) {
        AtlasPosition = atlasPosition;
        Replaceable = replaceable;
        SpriteSize = spriteSize;
    }
    public TileType(int atlasX, int atlasY, bool replaceable = true) 
        : this(new Point(atlasX, atlasY), replaceable) {}
    public TileType(int atlasX, int atlasY, int atlasWidth, int atlasHeight, bool replaceable = true) 
        : this(new Point(atlasX, atlasY), new Point(atlasWidth, atlasHeight), replaceable) {}

    public Rectangle GetAtlasBounds() =>
        new Rectangle(new Point(AtlasPosition.X * Game.TileSize, AtlasPosition.Y * Game.TileSize), SpriteSize);
    
    public int GetIndex() =>
        TileTypes.FindIndex(t => t.AtlasPosition == this.AtlasPosition);
    
    public static TileType FromInteger(int integer) =>
        TileTypes[MathHelper.Clamp(integer, 0, TileTypes.Count - 1)];
    
    public static implicit operator TileType(int integer){
        return FromInteger(integer);
    }
}