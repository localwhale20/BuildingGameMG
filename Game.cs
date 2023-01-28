using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using System.Collections.Generic;
using System.Linq;
using BuildingGame.UI;
using Myra.Graphics2D.UI;
using BuildingGame.Net;
using System.Net;
using BuildingGame.Net.Packets;
using BuildingGame.Util;
using System.Diagnostics;
using Serilog.Core;
using Serilog;
using System.IO;
using System.Reflection;
using System;
using System.Threading.Tasks;

namespace BuildingGame;

public class Game : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;


    public static Texture2D SpriteSheet { get; private set; }
    public static SpriteFont DebugFont { get; private set; }
    public static SpriteFont UserNameFont { get; private set; }

    private OrthographicCamera Camera { get; set; }
    private float Speed { get; set; } = 200;

    public static readonly int TileSize = 16;

    public static List<Tile> Tiles { get; set; }= new();
    private TileType _currentTile = TileType.TileTypes.First();

    private int _previousScrollDelta = 0;
    private int _scrollDelta = 0;

    private DesktopRoot _root;
    private TileSelectMenu _tileSelectMenu;
    private TilePreview _tilePreview;
    private Myra.Graphics2D.UI.Desktop _desktop;
    private MainMenu _mainMenu;

    private Client _client;
    private Server _server;

    public Game()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
        _graphics.ApplyChanges();
        Log.Information("Hello from game!");
        
    }

    protected override void Initialize()
    {
        Window.Title = "BuildingGame " + GameUtils.GetVersion();

        var viewportAdapter = new WindowViewportAdapter(Window, GraphicsDevice);
        Camera = new(viewportAdapter);
        Camera.MinimumZoom = 1;

        _previousScrollDelta = Mouse.GetState().ScrollWheelValue;

        Myra.MyraEnvironment.Game = this;
        _desktop = new();       

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        SpriteSheet = Content.Load<Texture2D>("Textures/SpriteSheet");
        DebugFont = Content.Load<SpriteFont>("Fonts/Debug");
        UserNameFont = Content.Load<SpriteFont>("Fonts/UserNameFont");

        _mainMenu = new();
        var username = _mainMenu.FindChildById<TextBox>("playerName").Text;
        _mainMenu.FindChildById<TextButton>("newLocalGame").Click += (sender, ev) => {
            _desktop.Root = _root;
        };
        _mainMenu.FindChildById<TextButton>("newMultiplayerGame").Click += async (sender, ev) => {
            // client = new();
            // client.Connect("localhost", username);
            var local = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Server.Port);
            _server = Server.Create();
            _client = Client.Create(local, username);
            Log.Debug("Client info:\n\t- Username: " + _client.Username + "\n\t- Server: " + _client.EndPoint);
            _server.Init();
            await _client.ConnectAsync();
            

            _desktop.Root = _root;
        };
        _mainMenu.FindChildById<TextButton>("joinTo").Click += async (sender, ev) => {
            var address = _mainMenu.FindChildById<TextBox>("joinToAddress").Text;

            _client = Client.Create(new IPEndPoint(IPAddress.Parse(address), 1928), username);
            try{
                await _client.ConnectAsync();
            } catch (System.Exception ex) { 
                Dialog.CreateMessageBox(
                    "Conection Trouble", 
                    $"Can't connect to {_client.EndPoint}.\nException: {ex.Message}"
                );
            }
            
            _desktop.Root = _root;
        };

        _tileSelectMenu = new();
        _tileSelectMenu.TileSelected += (sender, ev) => {
            _tilePreview.ChangeTile(ev.SelectedTileType);
            _currentTile = ev.SelectedTileType;
        };
        _tilePreview = new();
        _tilePreview.TouchDown += (sender, ev) => {
            _tileSelectMenu.Visible = true;
        };
        _root = new();
        _root.BuildUI(_tileSelectMenu, _tilePreview);
        _desktop.Root = _mainMenu;
    }

    protected async override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();
        _scrollDelta = System.Math.Clamp(mouse.ScrollWheelValue - _previousScrollDelta, -1, 1);

        if (_client != null) {
            _client.UpdateAsync();
        }
        if (_server != null) {
            _server.UpdateAsync();
        }

        if (IsActive && _desktop.Root != _mainMenu){
            if (keyboard.IsKeyDown(Keys.Escape)) _tileSelectMenu.Visible = false;
            if (keyboard.IsKeyDown(Keys.B)) _tileSelectMenu.Visible = true;

            if (_tileSelectMenu.Visible) return;

            Camera.Move(GetMovementDirection() * gameTime.GetElapsedSeconds() * Speed);

            if (_desktop.IsMouseOverGUI){
                return;
            }

            if (mouse.LeftButton == ButtonState.Pressed){
                var pos = (Camera.ScreenToWorld(mouse.X, mouse.Y) - new Vector2(8, 8)) / TileSize;
                pos.Round();
                pos *= TileSize;

                var tile = new Tile(pos, _currentTile);

                if (Tiles.Find(t => t.Position == tile.Position) == null)
                    Tiles.Add(tile);
                else{
                    var prevTile = Tiles.Find(t => t.Position == pos);
                    if (prevTile.Type.Replaceable) prevTile.Type = tile.Type;
                }

                if (_client.Connected){
                    var packet = PacketMaker.CreateTileStatePacket(pos, tile.Type.GetIndex());
                    await _client.SendToServerAsync(packet);
                }
            }
            if (mouse.RightButton == ButtonState.Pressed){
                var pos = (Camera.ScreenToWorld(mouse.X, mouse.Y) - new Vector2(8, 8)) / TileSize;
                pos.Round();
                pos *= TileSize;

                var tile = Tiles.Find(t => t.Position == pos);

                Tiles.Remove(tile);

                if (_client.Connected) {
                    var packet = PacketMaker.CreateTileStatePacket(pos, -1);
                    await _client.SendToServerAsync(packet);
                }
            }

            if (keyboard.IsKeyDown(Keys.LeftControl)) {
                if (_scrollDelta > 0) Camera.ZoomIn(1);
                else if (_scrollDelta < 0) Camera.ZoomOut(1);
            }
            else{
                var idx = TileType.TileTypes.FindIndex(0, t => t.AtlasPosition == _currentTile.AtlasPosition);
                idx -= _scrollDelta;
                idx = System.Math.Clamp(idx, 0, TileType.TileTypes.Count - 1);

                _currentTile = TileType.TileTypes[idx];

                _tilePreview.ChangeTile(_currentTile);
            }
            
        }

        _previousScrollDelta = mouse.ScrollWheelValue;

        base.Update(gameTime);
    }

    private Vector2 GetMovementDirection(){
        var direction = Vector2.Zero;
        var state = Keyboard.GetState();

        if (state.IsKeyDown(Keys.W)) direction += -Vector2.UnitY;
        if (state.IsKeyDown(Keys.S)) direction += Vector2.UnitY;
        if (state.IsKeyDown(Keys.A)) direction += -Vector2.UnitX;
        if (state.IsKeyDown(Keys.D)) direction += Vector2.UnitX;

        return direction;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        var viewMatrix = Camera.GetViewMatrix();

        // World Section

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp,
                           transformMatrix: viewMatrix);   
        
        foreach (var tile in Tiles){
            tile.Draw(_spriteBatch);
        }

        

        _spriteBatch.End();

        _desktop.Render();

        base.Draw(gameTime);
    }

    
}
