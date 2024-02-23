using Godot;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Game : Node2D
{
  public static int tileSize = 32;
  public Controller controller;
  public int numberOfTeams = 2;
  public int currentTeam = 1;
  public GameMap map;
  List<Unit> deck;
  Label resourcesLabel;
  int resources;
  string saveJson;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    Camera2D camera = new Camera2D();
    camera.Enabled = true;
    AddChild(camera);
    controller = new Controller();
    controller.parentGame = this;
    AddChild(controller);
    GD.Print($"{this.Owner}");

    
    var button = new Button();
    button.Text = "Next Turn";
    button.ZIndex = 2;
    button.Pressed += NextTurn;
    AddChild(button);
    
    var button2 = new Button();
    button2.Text = "SaveGame";
    button2.ZIndex = 2;
    button2.Position = new Vector2(button2.Position.X+32, button2.Position.Y+32);
    button2.Pressed += SaveGame;
    AddChild(button2);

    var button3 = new Button();
    button3.Text = "LoadGame";
    button3.ZIndex = 2;
    button3.Position = new Vector2(button2.Position.X+64, button2.Position.Y+32);
    button3.Pressed += LoadGame;
    AddChild(button3);

    resourcesLabel = new Label();
    resourcesLabel.ZIndex = 3;
    resourcesLabel.Scale = new Vector2(1.5f, 1.5f);
    resourcesLabel.AddThemeColorOverride("font_outline_color", new Color(0,0,0,1));
    resourcesLabel.AddThemeConstantOverride("outline_size", 15);
    AddChild(resourcesLabel);
    resourcesLabel.Text = $"{resources}💰";

    test1();
  }

  void test1(){
    map = new GameMap(40, 40);
    for (int i = 0; i < map.sizeX; i++){
      for (int j = 0; j < map.sizeY; j++){
      Tile tile = Factory.GetPresetTile(TilePreset.Plains, i,j, map);
      Node2D tileNode = Tile.createTileNode(tile, Factory.GetTileTexture(TileTexture.Plains));
      map.AddChild(tileNode);
      } 
    }
    // SETUP
    map.tileMap[3,3].AddTileEffect(Factory.GetTileEffect("RockyTerrain"));
    map.tileMap[3,3].AddTileEffect(Factory.GetTileEffect("Flame"));

    Unit u1 = new Unit("Healer", 2, 20, 50, 8, 2, 5, 5, UnitSpriteFrames.blueArcher).SetNewID();
    u1.AddUnitToMap(map);
    map.unitMap[u1.x,u1.y] = u1;
    u1.AddSkill(new DoubleTap());
    u1.AddSkill(Factory.GetSkill("BitterMedicine"));
    u1.AddSkill(Factory.GetSkill("HealingAura"));
    Unit u2 = new Unit("Sniper", 2, 20, 50, 8, 2, 3, 3, UnitSpriteFrames.blueArcher).SetNewID();
    u2.AddUnitToMap(map);
    u2.sprite.Animation = "front_idle";
    map.unitMap[u2.x,u2.y] = u2;
    u2.AddUnitEffect(new PreciseShots());
    u2.AddUnitEffect(Factory.GetUnitEffect("Dodge"));
    u2.AddSkill(Factory.GetSkill("DoubleTap"));
    Unit u3 = new Unit("Summoner", 2, 20, 50, 8, 2, 6, 6, UnitSpriteFrames.blueArcher).SetNewID();
    u3.AddUnitToMap(map);
    map.unitMap[u1.x,u1.y] = u1;
    u3.AddSkill(new DoubleTap());
    u3.AddUnitEffect(new SummonSkillsFromDeck());
    deck = SaveUtil.LoadDeck(2);
    map.decks[2] = deck;
    
    map.decks[1] = SaveUtil.LoadDeck(1);

    //List<Unit> newDeck = new List<Unit>(){u1, u2, u3};
    //SaveUtil.SaveDeck(newDeck, 2);
    // TODO make the SummonSkillsFromList effect compatibile with save


    AddChild(map);
    }

    void testDeck(){
    map = new GameMap(40, 40);
    for (int i = 0; i < map.sizeX; i++){
      for (int j = 0; j < map.sizeY; j++){
        Tile tile = Factory.GetPresetTile(TilePreset.Plains, i,j, map);
        Node2D tileNode = Tile.createTileNode(tile, Factory.GetTileTexture(TileTexture.Plains));
        map.AddChild(tileNode);
      } 
    }
    /*
    Unit u1 = new Unit("Healer", 1, 25, 50, 8, 3, 5, 5, UnitSpriteFrames.blueArcher);
    u1.AddSkill(Factory.GetSkill("HealingAura"));
    u1.AddSkill(new BitterMedicine());
    u1.AddSkill(new DoubleTap());
    Unit u2 = new Unit("Sharpshooter", 2, 20, 50, 8, 4, 3, 3, UnitSpriteFrames.blueArcher);
    u2.AddSkill(Factory.GetSkill("DoubleTap"));
    u2.AddUnitEffect(Factory.GetUnitEffect("Dodge"));
    u2.AddUnitEffect(new PreciseShots());
    */
    AddChild(map);

    //SaveUtil.SaveDeck(new List<Unit>(){u1, u2});

    deck = SaveUtil.LoadDeck(1);

  }

  public void SaveGame(){
    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start();

    saveJson = SaveUtil.SaveGame(this);
    
    stopwatch.Stop();
    long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
    GD.Print($"Save Elapsed Time: {elapsedMilliseconds} ms");
  }

  public void LoadGame(){
    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start();

    Save loadedSave = SaveUtil.LoadSave();
    SaveUtil.CreateGame(this, loadedSave);
    for(int i = 1; i<=numberOfTeams; i++){
      map.decks[i] = SaveUtil.LoadSaveDeck(i);
    }
    controller.Reset();
    GD.Print("Load SUCCESS!!");


    stopwatch.Stop();
    long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
    GD.Print($"Load Elapsed Time: {elapsedMilliseconds} ms");

    GD.Print("Dead units:");
    foreach(Unit u in map.graveyard){
      GD.Print($"{u.unitName}");
    }
  }

  public void NextTurn(){
    GD.Print($"---- End turn of team {currentTeam}");
    EndTurn();
    nextTeam();
    GD.Print($"---- Begin turn of team {currentTeam}");
    BeginTurn();
  }

  private void nextTeam(){
    currentTeam += 1;
    if(currentTeam>numberOfTeams){
      currentTeam = 1;
    }
  }

  public void EndTurn(){
    foreach(Unit unit in map.unitMap){
      if(unit!=null && unit.team == currentTeam){
        unit.OnEndTurn();
      }
    }
    foreach(Tile tile in map.tileMap){
      foreach(TileEffect tileEffect in tile.tileEffects){
        if(tileEffect.countdownTrigger == Trigger.OnEndTurn){
          if(tileEffect.source == null || tileEffect.source.team == currentTeam){
            tileEffect.Countdown();
          }
        }
      }
    }
  }

  public void BeginTurn(){
  foreach(Unit unit in map.unitMap){
    if(unit!=null && unit.team == currentTeam){
    unit.OnBeginTurn();
    }
  }
  foreach(Tile tile in map.tileMap){
    foreach(TileEffect tileEffect in tile.tileEffects){
    if(tileEffect.countdownTrigger == Trigger.OnBeginTurn){
      if(tileEffect.source == null || tileEffect.source.team == currentTeam){
      tileEffect.Countdown();
      }
    }
    }
  }
  }

}
