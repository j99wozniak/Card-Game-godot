using Godot;

public partial class MenuController : Control
{
  Button loadTestButton;
  Button loadEditorButton;
  Button loadSave;
  Label noSaveInfo;
  Button quitButton;
  public override void _Ready(){
    loadTestButton = (Button)FindChild("LoadTestButton");
    loadTestButton.Connect("pressed", Callable.From(() => RunTheGame(1)));

    loadEditorButton = (Button)FindChild("LoadEditorButton");
    loadEditorButton.Connect("pressed", Callable.From(() => RunTheGame(2)));

    loadSave = (Button)FindChild("LoadSave");
    loadSave.Connect("pressed", Callable.From(() => RunTheGame(3)));
    noSaveInfo = (Label)FindChild("NoSaveInfo");

    quitButton = (Button)FindChild("QuitButton");
    quitButton.Connect("pressed", Callable.From(() => GetTree().Quit()));
  }

  private void RunTheGame(int choice){
    if(choice == 3){
      if(!FileAccess.FileExists("user://Saves/save_game.json")){
        noSaveInfo.Visible = true;
        noSaveInfo.Modulate = new Color(1f, 1f, 1f, 1f);
        return;
      }
    }
    PackedScene scene = (PackedScene)GD.Load("res://game.tscn"); 
    Game gameInst = (Game)scene.Instantiate();
    gameInst.loadVar = choice;
    GetTree().Root.AddChild(gameInst);
    Hide();
  }

  public override void _Process(double delta){
    if(noSaveInfo.Visible){
      float a = noSaveInfo.Modulate.A*0.95f;
      if(a<0.1){
        a = 1f;
        noSaveInfo.Visible = false;
      }
      noSaveInfo.Modulate = new Color(1f, 1f, 1f, a);
    }
  }

}
