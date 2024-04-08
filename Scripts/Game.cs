using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class Game : Node2D
{
  public int loadVar;
  public static int tileSize = 32;
  public Controller controller;
  public int numberOfTeams = 2;
  public int currentTeam = 1;
  public GameMap map;
  public Player [] players;
  
  public HudController hudController;
  Label resourcesLabel;
  int resources;
  string saveJson;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    Camera2D camera = new Camera2D();
    camera.Enabled = true;
    AddChild(camera);
    
    PackedScene scene = (PackedScene)GD.Load("res://hud.tscn"); 
    CanvasLayer hudInst = (CanvasLayer)scene.Instantiate();
    hudController = new HudController(hudInst, this);
    AddChild(hudInst);
    
    controller = new Controller();
    controller.parentGame = this;
    AddChild(controller);
    GD.Print($"{this.Owner}");
    
    if(!FileAccess.FileExists("user://Decks/deck1.json"))
      saveInitialDeck1();
    if(!FileAccess.FileExists("user://Decks/deck2.json"))
        saveInitialDeck2();

    switch(loadVar){
      case 1:
        test1();
        break;
      case 2:
        CheckIfFileExistsElseCreate("Levels/Editable/", "map1.png");
        CheckIfFileExistsElseCreate("Levels/Editable/", "mapUnits1.png");
        editableLevel();
        break;
      case 3:
        LoadGame();
        break;
    }
  }

  void saveInitialDeck1(){
    Unit strongArcher = new("Strong Archer", null, 30, 15, 6, 4, 0, 0, UnitSpriteFrames.blueArcher, "portrait_Archer_Blue1");
    strongArcher.AddSkill(new Shoot());
    strongArcher.AddUnitEffect(new StrongShots());
    Unit quickArcher = new("Quick Archer", null, 20, 15, 10, 5, 0, 0, UnitSpriteFrames.blueArcher, "portrait_Archer_Blue1");
    quickArcher.AddSkill(new DoubleTap());
    Unit preciseArcher = new("Precise Archer", null, 20, 14, 8, 4, 0, 0, UnitSpriteFrames.blueArcher, "portrait_Archer_Blue1");
    preciseArcher.AddSkill(new Shoot());
    preciseArcher.AddUnitEffect(new Sniper());
    preciseArcher.AddUnitEffect(new Lucky());

    SaveUtil.SaveDeck(new List<Unit>(){strongArcher, quickArcher, preciseArcher}, 1);
  }
  void saveInitialDeck2(){
    Unit u1 = new Unit("Healer", null, 20, 50, 8, 2, 5, 5, UnitSpriteFrames.redArcher, "portrait_Archer_Red1").SetNewID();
    u1.AddSkill(new DoubleTap());
    u1.AddSkill(Factory.GetSkill("BitterMedicine"));
    u1.AddSkill(Factory.GetSkill("HealingAura"));
    Unit u2 = new Unit("Sniper", null, 20, 50, 8, 2, 3, 3, UnitSpriteFrames.redArcher, "portrait_Archer_Red1").SetNewID();
    u2.AddUnitEffect(new PreciseShots());
    u2.AddUnitEffect(Factory.GetUnitEffect("Dodge"));
    u2.AddSkill(Factory.GetSkill("DoubleTap"));
    Unit u3 = new Unit("Summoner", null, 20, 50, 8, 2, 6, 6, UnitSpriteFrames.redArcher, "portrait_Archer_Red1").SetNewID();
    u3.AddSkill(new DoubleTap());
    u3.AddUnitEffect(new SummonSkillsFromDeck());

    SaveUtil.SaveDeck(new List<Unit>(){u1, u2, u3}, 2);
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
    map.tileMap[0,10].tileName = "sands";
    map.tileMap[0,10].tilePreset = TilePreset.Sands;
    map.tileMap[0,10].tileTexture = TileTexture.Sands;
    map.tileMap[0,10].tileSprite.Texture = Factory.GetTileTexture(TileTexture.Sands);

    for (int i = 8; i < 12; i++){
      for (int j = 0; j < 3; j++){
        map.tileMap[i,j].tileName = "rocky";
        map.tileMap[i,j].tilePreset = TilePreset.Rocky;
        map.tileMap[i,j].tileTexture = TileTexture.Rocky;
        map.tileMap[i,j].tileSprite.Texture = Factory.GetTileTexture(TileTexture.Rocky);
        TileEffect rocky = Factory.GetTileEffect("RockyTerrain");
        rocky.preset = true;
        map.tileMap[i,j].AddTileEffect(rocky);
      } 
    }

    // SETUP
    map.tileMap[3,3].AddTileEffect(Factory.GetTileEffect("RockyTerrain"));
    map.tileMap[3,3].AddTileEffect(Factory.GetTileEffect("Flame"));

    map.tileMap[7,1].AddTileEffect(Factory.GetTileEffect("JumpPad"));

    Player player1 = new Player(this, 1, false);
    Player player2 = new Player(this, 2, false);
    
    Condition wincon11_p1 = Factory.GetCondition("CharacterStayCondition", player1, "10");
    Condition wincon12_p1 = Factory.GetCondition("DestroyCastleCondition", player1, "246");
    Condition wincon21_p1 = Factory.GetCondition("EliminateAllEnemiesCondition", player1);
    Condition wincon_p2 = Factory.GetCondition("EliminateAllEnemiesCondition", player2);
    Condition losecond_p1 = Factory.GetCondition("AllAlliesEliminatedCondition", player1);
    Condition losecond_p2 = Factory.GetCondition("AllAlliesEliminatedCondition", player2);

    CombinedCondition stay_and_destr = new CombinedCondition(ConditionOperator.AND, new List<Condition>(){wincon11_p1, wincon12_p1});
    CombinedCondition finalWinCon = new CombinedCondition(ConditionOperator.OR, new List<Condition>(){stay_and_destr, wincon21_p1});
    player1.winCondition = finalWinCon;
    player2.winCondition = wincon_p2;

    player1.loseCondition = losecond_p1;
    player2.loseCondition = losecond_p2;

    player1.deck = SaveUtil.LoadDeck(1);
    player2.deck = SaveUtil.LoadDeck(2);

    players = new Player[numberOfTeams];
    players[0] = player1;
    players[1] = player2;

    Unit u1 = new Unit("Healer", player1, 20, 50, 8, 2, 5, 5, UnitSpriteFrames.blueArcher, "portrait_Archer_Blue1").SetNewID();
    u1.AddUnitToMap(map);
    u1.AddSkill(new DoubleTap());
    u1.AddSkill(Factory.GetSkill("BitterMedicine"));
    u1.AddSkill(Factory.GetSkill("HealingAura"));
    Unit u2 = new Unit("Sniper", player1, 20, 50, 8, 2, 3, 3, UnitSpriteFrames.blueArcher, "portrait_Archer_Blue1").SetNewID();
    u2.AddUnitToMap(map);
    u2.sprite.Animation = "front_idle";
    u2.AddUnitEffect(new PreciseShots());
    u2.AddUnitEffect(Factory.GetUnitEffect("Dodge"));
    u2.AddSkill(Factory.GetSkill("DoubleTap"));
    Unit u21 = new Unit("Summoner", player1, 20, 50, 8, 2, 4, 5, UnitSpriteFrames.blueArcher, "portrait_Archer_Blue1").SetNewID();
    u21.AddUnitToMap(map);
    u21.AddSkill(new DoubleTap());
    u21.AddUnitEffect(new SummonSkillsFromDeck());
    Unit u3 = new Unit("Summoner", player2, 20, 50, 8, 2, 6, 6, UnitSpriteFrames.redArcher, "portrait_Archer_Red1").SetNewID();
    u3.AddUnitToMap(map);
    u3.AddSkill(new DoubleTap());
    u3.AddUnitEffect(new SummonSkillsFromDeck());


    AddChild(map);
    InitializeTimeline();
    BeginTurn();
  }

  void editableLevel(){
    /*
    R=132, G=126, B=135 //rock = (0,5176471, 0,49411765, 0,5294118, 1)
    R=251, G=242, B=54 //sand = (0,9843137, 0,9490196, 0,21176471, 1)
    R=106, G=190, B=48 //plains = (0,41568628, 0,74509805, 0,1882353, 1)
    */
    Dictionary<Color, (TilePreset, TileTexture)> terrainDict = new(){
      {new Color(0.5176471f, 0.49411765f, 0.5294118f, 1), (TilePreset.Rocky, TileTexture.Rocky)},
      {new Color(0.9843137f, 0.9490196f, 0.21176471f, 1), (TilePreset.Sands, TileTexture.Sands)},
      {new Color(0.41568628f, 0.74509805f, 0.1882353f, 1), (TilePreset.Plains, TileTexture.Plains)},
    };
    CreateTerrainFromFile(terrainDict, "user://Levels/Editable/map1.png");

    // Create players
    players = new Player[numberOfTeams];
    Player player1 = new Player(this, 1, false);
    players[0] = player1;
    Player player2 = new Player(this, 2, true);
    players[1] = player2;
    Condition wincon1_p1 = Factory.GetCondition("CharacterStayCondition", player1, "310");
    Condition wincon2_p1 = Factory.GetCondition("EliminateAllEnemiesCondition", player1);
    CombinedCondition stay_or_el = new CombinedCondition(ConditionOperator.OR, new List<Condition>(){wincon1_p1, wincon2_p1});
    Condition wincon_p2 = Factory.GetCondition("EliminateAllEnemiesCondition", player2);
    Condition losecond_p1 = Factory.GetCondition("AllAlliesEliminatedCondition", player1);
    Condition losecond_p2 = Factory.GetCondition("AllAlliesEliminatedCondition", player2);
    player1.winCondition = stay_or_el;
    player1.loseCondition = losecond_p1;
    player2.winCondition = wincon_p2;
    player2.loseCondition = losecond_p2;

    /*
      Strong Archer (Has double the power of his shots)
      Quick Archer (Has doubletap)
      Far Archer (Has double range)
    */

    player1.deck = SaveUtil.LoadDeck(1);
    player2.deck = SaveUtil.LoadDeck(2);

    Dictionary<Color, ((string, Player, int hp, int st, int mv, int cost,  UnitSpriteFrames, string), List<string>, List<string>)> unitDict = new(){
      {new Color(0, 0, 1, 1), (("Summoner", player1, 20, 50, 8, 2, UnitSpriteFrames.blueArcher, "portrait_Archer_Blue1"), new List<string>(){"Shoot"}, new List<string>(){"SummonSkillsFromDeck"})},
      {new Color(1, 0, 0, 1), (("Archer", player2, 20, 7, 3, 2, UnitSpriteFrames.redArcher, "portrait_Archer_Red1"), new List<string>(){"Shoot"}, new List<string>(){})}
    };
    CreateUnitsFromFile(unitDict, "user://Levels/Editable/mapUnits1.png");

    AddChild(map);
    InitializeTimeline();
    BeginTurn();
  }

  public void CreateTerrainFromFile(Dictionary<Color, (TilePreset, TileTexture)> dict, string path){
    Image image = Image.LoadFromFile(path);
    map = new GameMap(image.GetWidth(), image.GetHeight());
    for (int x = 0; x < image.GetWidth(); x++){
      for (int y = 0; y < image.GetHeight(); y++){
        TilePreset tp = TilePreset.none;
        TileTexture tt = TileTexture.none;
        Color color = image.GetPixel(x, y);
        foreach(var (dictColor, (tilePreset, tileTexture)) in dict){
          if(color == dictColor){
            tp = tilePreset;
            tt = tileTexture;
            break;
          }
        }
        Tile tile = Factory.GetPresetTile(tp, x,y, map);
        Node2D tileNode = Tile.createTileNode(tile, Factory.GetTileTexture(tt));
        map.AddChild(tileNode);
      }
    }
  }

  
  public void CreateUnitsFromFile(Dictionary<Color, ((string, Player, int, int, int, int, UnitSpriteFrames, string), List<string>, List<string>)> dict, string path){
    Image image = Image.LoadFromFile(path);
    for (int x = 0; x < image.GetWidth(); x++){
      for (int y = 0; y < image.GetHeight(); y++){
        Color color = image.GetPixel(x, y);
        foreach(var (dictColor, ((unitName, player, baseMaxHp, baseMaxStamina, baseMaxMovement, baseCost,
                                  frames, portraitName), skillNames, effectNames)) in dict){
          if(color == dictColor){
            Unit u = new Unit(unitName, player, baseMaxHp, baseMaxStamina, baseMaxMovement, baseCost, x, y, frames, portraitName).SetNewID();
            foreach(string skillName in skillNames){
              u.AddSkill(Factory.GetSkill(skillName));
            }
            foreach(string effectName in effectNames){
              u.AddUnitEffect(Factory.GetUnitEffect(effectName));
            }
            u.AddUnitToMap(map);
            break;
          }
        }
      }
    }
  }

  private void CheckIfFileExistsElseCreate(string dir, string file){
    if (FileAccess.FileExists("user://"+dir+file)){
      GD.Print("File exists.");
    }
    else{
      GD.Print("File does not exist.");
      DirAccess.MakeDirRecursiveAbsolute("user://"+dir);
      Image readFile = ((Texture2D)GD.Load("res://"+dir+file)).GetImage();
      readFile.SavePng("user://"+dir+file);
    }
    
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
    SaveUtil.LoadSave(this);
    controller.Reset();
    hudController.ColorConditions();
    GD.Print("Load SUCCESS!!");

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
    if(hudController.conditionsClosedScrollContainer.Visible == false){
      hudController.ShowConditions();
    }
    if(players[currentTeam-1].isCPU){
      players[currentTeam-1].AIExecute();
      NextTurn();
    }
  }

  private void nextTeam(){
    currentTeam += 1;
    if(currentTeam>numberOfTeams){
      currentTeam = 1;
    }
  }

  public void EndTurn(){
    foreach(Unit unit in map.unitMap){
      if(unit!=null && unit.player.team == currentTeam){
        unit.OnEndTurn();
      }
    }
    foreach(Tile tile in map.tileMap){
      foreach(TileEffect tileEffect in tile.tileEffects){
        if(tileEffect.countdownTrigger == Trigger.OnEndTurn){
          if(tileEffect.source == null || tileEffect.source.player.team == currentTeam){
            tileEffect.Countdown();
          }
        }
      }
    }
    controller.Reset();
  }

  public void BeginTurn(){
    foreach(Unit unit in map.unitMap){
      if(unit!=null && unit.player.team == currentTeam){
        unit.OnBeginTurn();
      }
    }
    foreach(Tile tile in map.tileMap){
      foreach(TileEffect tileEffect in tile.tileEffects){
        if(tileEffect.countdownTrigger == Trigger.OnBeginTurn){
          if(tileEffect.source == null || tileEffect.source.player.team == currentTeam){
          tileEffect.Countdown();
          }
        }
      }
    }
    CheckConditions();
  }
  
  HashSet<Player> winners = new();
  HashSet<Player> losers = new();
  public void CheckConditions(){
    hudController.ColorConditions();
    foreach(Player player in players){
      if(player.winCondition.IsMet()){
        GD.Print("Player " + player.team + " wins!!");
        winners.Add(player);
      }
      if(player.loseCondition.IsMet()){
        GD.Print("Player " + player.team + " loses!!");
        losers.Add(player);
        // Could add Disable, so that we skip this players turn
      }
    }

    if(winners.Count == 1){
      GameOver($"Player {winners.Single().team} wins!");
      return;
    }
    if(winners.Count > 1){
      string winningPlayersString = "";
      foreach(Player p in winners){
        if(winningPlayersString.Length>1){
          winningPlayersString += " and ";
        }
        winningPlayersString += $"Player {p.team}";
      }
      GameOver("Draw! " + winningPlayersString + " win!");
      return;
    }
    if(losers.Count == numberOfTeams){
      string loserPlayersString = "";
      foreach(Player p in winners){
        if(loserPlayersString.Length>1){
          loserPlayersString += " and ";
        }
        loserPlayersString += $"Player {p.team}";
      }
      GameOver("Draw! " + loserPlayersString + " lose!");
      return;
    }
    if(losers.Count == numberOfTeams-1){
      foreach(Player player in players){
        if(!losers.Contains(player)){
          GameOver($"Player {player.team} wins!");
          return;
        }
      }
    }
    AddToTimeline();
  }

  public void GameOver(string message){
    PackedScene scene = (PackedScene)GD.Load("res://game_over_screen.tscn"); 
    CanvasLayer gameOverInst = (CanvasLayer)scene.Instantiate();
    Label gameOverMessage = (Label)gameOverInst.FindChild("GameOverMessage");
    gameOverMessage.Text = message;

    Button gameOverQuitButton = (Button)gameOverInst.FindChild("GameOverQuitButton");
    gameOverQuitButton.Connect("pressed", Callable.From(() => GetTree().Quit()));
    GetTree().Root.AddChild(gameOverInst);
  }

  public SaveNode oldestSave;
  public SaveNode currentSave;
  public int numberOfSaves = 0;
  public int saveChange = 0;
  public int maxNumberOfSaves = 30;

  public void InitializeTimeline(){
    SaveNode firstSave = new(SaveUtil.CreateSave(this));
    oldestSave = firstSave;
    currentSave = firstSave;
    numberOfSaves = 1;
  }

  public void AddToTimeline(){
    if(players[currentTeam-1].isCPU)
      return;
    SaveNode newSave = new(SaveUtil.CreateSave(this));
    newSave.previousNode = currentSave;
    currentSave.nextNode = newSave;
    currentSave = newSave;
    numberOfSaves += saveChange;
    saveChange = 0;
    numberOfSaves += 1;
    if(numberOfSaves>maxNumberOfSaves){
      oldestSave = oldestSave.nextNode;
      oldestSave.previousNode = null;
      numberOfSaves -= 1;
      GD.Print("Removing Tail");
    }
  }

  public void BackInTime(){
    if(currentSave.previousNode!=null){
      currentSave = currentSave.previousNode;
      SaveUtil.CreateGame(this, currentSave.save);
      saveChange -= 1;
    }
    else{
      GD.Print("Can't go any older");
    }
  }

  public void ForwardInTime(){
    if(currentSave.nextNode!=null){
      currentSave = currentSave.nextNode;
      SaveUtil.CreateGame(this, currentSave.save);
      saveChange += 1;
    }
    else{
      GD.Print("Can't go any newer");
    }
  }

}

public class SaveNode{
  public Save save;
  public SaveNode nextNode = null;
  public SaveNode previousNode = null;
  public SaveNode(Save save){
    this.save = save;
  }
}

