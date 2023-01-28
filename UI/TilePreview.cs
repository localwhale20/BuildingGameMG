using Myra.Graphics2D.UI;
using Myra.Graphics2D.TextureAtlases;
using System.Linq;
using BuildingGame.Util;

namespace BuildingGame.UI;

public sealed class TilePreview : ImageButton {
    private TileType _tile = TileType.TileTypes.First();

    public TilePreview() {
        BuildUI();
    }

    private void BuildUI() {
        Margin = new Myra.Graphics2D.Thickness(12);
        Background = new TextureRegion(Game.SpriteSheet, _tile.GetAtlasBounds());
        var size = new Microsoft.Xna.Framework.Point(96);
        Top = 0;
        Left = 800 - size.X;
        var scale = GameUtils.CalculatePreviewSize(_tile);
        Width = size.X;
        Height = size.Y;
        ImageWidth = size.X;
        ImageHeight = size.Y;
    }

    public void ChangeTile(TileType type){
        if (type == _tile) return;
        
        _tile = type;
        Background = new TextureRegion(Game.SpriteSheet, _tile.GetAtlasBounds());
        var size = new Microsoft.Xna.Framework.Point(96);
        var scale = GameUtils.CalculatePreviewSize(_tile);
        Width = size.X;
        Height = size.Y;
        ImageWidth = size.X;
        ImageHeight = size.Y;
        Left = 800 - size.X;
    }
}