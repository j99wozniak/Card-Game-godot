using Godot;
using System;

public partial class Controller : Node
{
  private Camera2D camera;
  private Viewport viewport;
  public Game parentGame;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    GD.Print($"# entered here");
    viewport = GetViewport();
    camera = viewport.GetCamera2D();
    GD.Print($"# {camera}");
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
  }

  public override void _Input(InputEvent @event)
  {
    if(@event is InputEventKey eventKey){
      if (eventKey.Pressed && eventKey.Keycode == Key.W){
        camera.Position = new Vector2(camera.Position.X, camera.Position.Y-20);
      }
      if (eventKey.Pressed && eventKey.Keycode == Key.A){
        camera.Position = new Vector2(camera.Position.X-20, camera.Position.Y);
      }
      if (eventKey.Pressed && eventKey.Keycode == Key.S){
        camera.Position = new Vector2(camera.Position.X, camera.Position.Y+20);
      }
      if (eventKey.Pressed && eventKey.Keycode == Key.D){
        camera.Position = new Vector2(camera.Position.X+20, camera.Position.Y);
      }
    }
    else if (@event is InputEventMouseMotion eventMouseMotion){
      GD.Print(GetTree().Root.GetNode<Node2D>("Node2D").GetGlobalMousePosition());
      int xTile = (int)Math.Floor((GetTree().Root.GetNode<Node2D>("Node2D").GetGlobalMousePosition().X + Game.TileSize/2) / Game.TileSize);
      int yTile = (int)Math.Floor((GetTree().Root.GetNode<Node2D>("Node2D").GetGlobalMousePosition().Y + Game.TileSize/2) / Game.TileSize);
      if(xTile>=0 && yTile>=0 && xTile < parentGame.map.sizeX && yTile < parentGame.map.sizeY){
        parentGame.map.tileMap[xTile, yTile].SelectTile();
      }
    }
      //GD.Print(GetTree().Root.GetNode("Node2D").GetNode<Node2D>("Node2D").GetGlobalMousePosition());
  }
}
