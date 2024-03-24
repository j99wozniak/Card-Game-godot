using Godot;
using System;

public partial class MenuController : Control
{
  Button loadTestButton;
  Button loadEditorButton;
  Button quitButton;
  public override void _Ready(){
    loadTestButton = (Button)FindChild("LoadTestButton");
    loadTestButton.Connect("pressed", Callable.From(() => RunTheGame(1)));

    loadEditorButton = (Button)FindChild("LoadEditorButton");
    loadEditorButton.Connect("pressed", Callable.From(() => RunTheGame(2)));

    quitButton = (Button)FindChild("QuitButton");
    quitButton.Connect("pressed", Callable.From(() => GetTree().Quit()));
  }

  private void RunTheGame(int choice){
    GD.Print(choice);
    PackedScene scene = (PackedScene)GD.Load("res://game.tscn"); 
    Game gameInst = (Game)scene.Instantiate();
    gameInst.loadVar = choice;
    GetTree().Root.AddChild(gameInst);
    this.Hide();
  }

}
