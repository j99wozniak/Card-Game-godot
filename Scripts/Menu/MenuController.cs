using Godot;
using System;

public partial class MenuController : Control
{
  Button loadButton;
  Button quitButton;
  public override void _Ready(){
    loadButton = (Button)FindChild("LoadButton");
    loadButton.Connect("pressed", Callable.From(RunTheGame));

    quitButton = (Button)FindChild("QuitButton");
    quitButton.Connect("pressed", Callable.From(Quit));
  }

  private void RunTheGame(){
    PackedScene scene = (PackedScene)GD.Load("res://game.tscn"); 
    Node2D gameInst = (Node2D)scene.Instantiate();
    GetTree().Root.AddChild(gameInst);
    this.Hide();
  }

  private void Quit(){
    GetTree().Quit();
  }

}
