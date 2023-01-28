using Myra.Graphics2D.UI;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.TextureAtlases;
using System;
using BuildingGame.Util;

namespace BuildingGame.UI;

public sealed class TileSelectMenu : HorizontalStackPanel {
    public event EventHandler<TileSelectEventArgs> TileSelected;

    public TileSelectMenu(){
        BuildUI();
    }
    private void BuildUI(){
        foreach (var type in TileType.TileTypes){
            var button = new ImageButton();
            var imageScale = GameUtils.CalculatePreviewSize(type) * new Point(32);
            button.Image = new TextureRegion(Game.SpriteSheet, type.GetAtlasBounds());
            button.ImageWidth = imageScale.X;
            button.ImageHeight = imageScale.Y;
            button.Height = 64;
            button.Width = 64;
            button.Border = new SolidBrush(Color.White);
            button.BorderThickness = new Thickness(1);
            button.Margin = new Thickness(5);
            button.TouchDown += (sender, ev) => {
                TileSelected.Invoke(this, new() { SelectedTileType = type });
            };
            Widgets.Add(button);
        }

        Margin = new Thickness(50);
        Background = new SolidBrush(Color.Gray);
        Border = new SolidBrush(Color.White);
        BorderThickness = new Thickness(2);
        Visible = false;
        ZIndex = 10;
    }
    public void SetVisibility(bool visible){
        Visible = visible;
    }
}

public class TileSelectEventArgs : EventArgs {
    public TileType SelectedTileType { get; set; }
}