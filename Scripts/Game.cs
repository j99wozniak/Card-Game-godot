using Godot;
using System.Diagnostics;

public partial class Game : Node2D
{
  public static int tileSize = 32;
  public Controller controller;
  public int numberOfTeams = 2;
  public int currentTeam = 1;
  public GameMap map;
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

	
	SpriteFrames unitSpriteSheet = (SpriteFrames)GD.Load("res://Sprites/Units/Frames/ArcherFrames.tres");
	Unit u1 = new Unit(map, "u1", 1, 20, 50, 8, 5, 5, UnitSpriteFrames.blueArcher);
	Node2D unit1Node = Unit.createUnitNode(u1);
	map.AddChild(unit1Node);
	map.unitMap[u1.x,u1.y] = u1;
	u1.AddSkill(new DoubleTap());
	u1.AddUnitEffect(new PreciseShots());
	u1.AddSkill(Factory.GetSkill("HealingAura"));
	Unit u2 = new Unit(map, "u2", 2, 20, 50, 8, 3, 3, UnitSpriteFrames.blueArcher);
	Node2D unit2Node = Unit.createUnitNode(u2, unitSpriteSheet);
	map.AddChild(unit2Node);
	u2.sprite.Animation = "front_idle";
	map.unitMap[u2.x,u2.y] = u2;
	u2.AddUnitEffect(Factory.GetUnitEffect("Dodge"));
	u2.AddSkill(Factory.GetSkill("BitterMedicine"));
	u2.AddSkill(Factory.GetSkill("DoubleTap"));

	AddChild(map);
  }

  void test_aura(){

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
