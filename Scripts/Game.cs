using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public partial class Game : Node2D
{
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
    controller = new Controller();
    controller.parentGame = this;
    AddChild(controller);
    GD.Print($"{this.Owner}");

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
    Player player2 = new Player(this, 2, true);
    
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
    map.unitMap[u1.x,u1.y] = u1;
    u1.AddSkill(new DoubleTap());
    u1.AddSkill(Factory.GetSkill("BitterMedicine"));
    u1.AddSkill(Factory.GetSkill("HealingAura"));
    Unit u2 = new Unit("Sniper", player1, 20, 50, 8, 2, 3, 3, UnitSpriteFrames.blueArcher).SetNewID();
    u2.AddUnitToMap(map);
    u2.sprite.Animation = "front_idle";
    map.unitMap[u2.x,u2.y] = u2;
    u2.AddUnitEffect(new PreciseShots());
    u2.AddUnitEffect(Factory.GetUnitEffect("Dodge"));
    u2.AddSkill(Factory.GetSkill("DoubleTap"));
    Unit u3 = new Unit("Summoner", player2, 20, 50, 8, 2, 6, 6, UnitSpriteFrames.redArcher, "portrait_Archer_Red1").SetNewID();
    u3.AddUnitToMap(map);
    map.unitMap[u1.x,u1.y] = u1;
    u3.AddSkill(new DoubleTap());
    u3.AddUnitEffect(new SummonSkillsFromDeck());

    PackedScene scene = (PackedScene)GD.Load("res://hud.tscn"); 
    CanvasLayer hudInst = (CanvasLayer)scene.Instantiate();
    hudController = new HudController(hudInst, this);
    AddChild(hudInst);

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
    Save loadedSave = SaveUtil.LoadSave();
    SaveUtil.CreateGame(this, loadedSave);
    controller.Reset();
    
    GD.Print("loseCondition1"+players[0].loseCondition.IsMet());
    GD.Print("loseCondition2"+players[1].loseCondition.IsMet());
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
  }
  
  HashSet<Player> winners = new();
  HashSet<Player> losers = new();
  public void CheckConditions(){
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

}
