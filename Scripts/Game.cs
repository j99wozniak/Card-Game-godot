using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Game : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Tests to make: 
		// ## U1 uses skill Double Tap, while having passive Precise Shots, on U2 that has passive Dodge, but only enough stamina to dodge once
		// ## U2 uses skill Bitter Medicine on self, which gives it poison
		// ## Dodge shouldn't trigger on poison - it still triggers, but doesn't do anything
		// U1 uses skill Take Aim on U2, which increses damage on target from the source (U1) (should it make it impossible to dodge?)
		//  - Should it be a buff on source or debuff on target? - should be buff
		// U2 uses Bitter Medicine again, which will heal but also increase poison potency
		//  - Add to UnitEffect 'stackable' property. If it is stackable, it should implement overriden 'stack' method
		//  - Add max hp, so that one can't go over it
		// U1 uses skill One Tap on U2 dealing massive damage
		
		//test1();

		//test2();

		test3();

		// U1 has passive Defend
		// Effect onMove to remove this effect from nerby units, and to apply it when the move is done
		// When the ally unit receives this effect, it receives two effects 
		// - onMove - that will remove this effect if the end move is outside the range of the source
		// - onDamage - that will switch the target of the attack to the source of the passive
		// ----- this will not work if the ally unit comes into the range of the passive on their turn
		// Maybe have a broadcast function? that will broadcast to all units that the position of the unit changed
		// then other units could have checkers, to check if their passive auras do something to the unit that broadcasted position change
		// -- 
		// Second option, make the targets be cells themselves
		// Add another category of effects, that come from the cells they're standing on.
		// Make them be added when unit ends their turn on them, and then remove them when they leave it?
		// SOLUTION:
		// - add to the unit two linked effects: 
		//     - OnStartMove - remove X from nearby tiles (or from tiles from LinkedList<TileEffects>)
		//     - OnEndMove - add X to nearby tiles (and add them to LinkedList<TileEffects>?)
		// - X = TileEffect OnDamage that will do the defending of target (which is a Unit that is standing on the tile)

	}

	void test1(){
		GameMap map = new GameMap(40, 40);
		Texture2D plainsTexture = (Texture2D)GD.Load("res://Sprites/Tiles/plains.png");

		for (int i = 0; i < map.sizeX; i++){
			for (int j = 0; j < map.sizeY; j++){
				Node2D tileNode = new Node2D();
				Tile tile = new Tile();
				tileNode.AddChild(tile);
				tile.tileName = "Plains";
				tile.map = map;
				tile.x = i;
				tile.y = j;
				Sprite2D plainsSprite = new Sprite2D(); // Create a new Sprite2D.
				plainsSprite.Texture = plainsTexture;
				tile.tileSprite = plainsSprite;
				tileNode.AddChild(plainsSprite);
				tileNode.Position = tile.GetRealPosition();
				AddChild(tileNode);
				map.tileMap[i,j] = tile;
			} 
		}
		TileEffect rockyTerrain = new RockyTerrain(1);
		map.tileMap[3,3].AddTileEffect(rockyTerrain);
		
		Unit u1 = new Unit
		{
			unitName = "u1",
			map = map,
			baseMaxHp = 20,
			currentHp = 20,
			baseMaxStamina = 10,
			currentStamina = 10,
			x = 1,
			y = 1
		};

		map.unitMap[u1.x,u1.y] = u1;

		u1.AddSkill(new DoubleTap(10));
		u1.AddUnitEffect(new PreciseShots(5));

		Unit u2 = new Unit();
		u2.unitName = "u2";
		u2.map = map;
		u2.baseMaxHp = 20;
		u2.currentHp = 20;
		u2.baseMaxStamina = 10;
		u2.currentStamina = 6;
		u2.x = 3;
		u2.y = 3;
		map.unitMap[u2.x,u2.y] = u2;

		UnitEffect dodge1 = new Dodge();
		dodge1.source = u2;
		dodge1.count = 100;
		u2.AddUnitEffect(dodge1);

		u2.AddSkill(new BitterMedicine(10));

		u1.FireSkill("DoubleTap", u2);
		u2.FireSkill("BitterMedicine", u2);

		u1.OnEndTurn();
		u2.OnEndTurn();
		GD.Print(u2.currentHp);
	}

	void test2(){
		GameMap map = new GameMap(40, 40);
		Texture2D plainsTexture = (Texture2D)GD.Load("res://Sprites/Tiles/plains.png");
		Texture2D sandsTexture = (Texture2D)GD.Load("res://Sprites/Tiles/sands.png");

		for (int i = 0; i < map.sizeX; i++){
			for (int j = 0; j < map.sizeY; j++){
				Node2D tileNode = new Node2D();
				Tile tile = new Tile();
				tileNode.AddChild(tile);
				tile.tileName = "Plains";
				tile.map = map;
				tile.x = i;
				tile.y = j;
				tile.cost = 1;
				Sprite2D plainsSprite = new Sprite2D(); // Create a new Sprite2D.
				plainsSprite.Texture = plainsTexture;
				tile.tileSprite = plainsSprite;
				plainsSprite.Name = "spriteNode";
				tileNode.AddChild(plainsSprite);
				tileNode.Position = tile.GetRealPosition();
				AddChild(tileNode);
				map.tileMap[i,j] = tile;
			} 
		}
		TileEffect rockyTerrain = new RockyTerrain(1);
		map.tileMap[3,3].AddTileEffect(rockyTerrain);

		map.tileMap[16,15].AddTileEffect(new Glue());
		
		Unit u1 = new Unit
		{
			unitName = "u1",
			map = map,
			baseMaxHp = 20,
			currentHp = 20,
			baseMaxStamina = 10,
			currentStamina = 10,
			x = 15,
			y = 15,
			baseMaxMovement = 10,
			currentMovement = 10
		};

		map.unitMap[u1.x,u1.y] = u1;

		u1.AddUnitEffect(new Skip());
		u1.AddUnitEffect(new Eager());
		u1.currentMovement = u1.maxMovement;


		// Create a Stopwatch instance
		Stopwatch stopwatch = new Stopwatch();

		// Start the stopwatch
		stopwatch.Start();

		// Call the function you want to benchmark
		
		Dictionary<(int x, int y), float> dict = Movement.GetAccessibleTiles(u1, map);

		foreach (var kvp in dict) {
			GD.Print($"Key = {kvp.Key}, Value = {kvp.Value}");
			//Sprite2D sandsSprite = new Sprite2D(); // Create a new Sprite2D.
			//sandsSprite.Texture = sandsTexture;
			Tile tile = map.tileMap[kvp.Key.x,kvp.Key.y];
			Node2D tileNode = (Node2D)tile.GetParent();
			//tile.tileSprite = sandsSprite;

			Sprite2D mySprite = tileNode.GetNode<Sprite2D>("spriteNode");
			mySprite.Texture = sandsTexture;
			//tileNode.AddChild(plainsSprite);
		}

		// Stop the stopwatch
		stopwatch.Stop();

		// Get the elapsed time in milliseconds
		long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

		// Print the elapsed time
		GD.Print($"Elapsed Time: {elapsedMilliseconds} ms");

	}

	void test3(){
		GameMap map = new GameMap(40, 40);
		Texture2D plainsTexture = (Texture2D)GD.Load("res://Sprites/Tiles/plains.png");
		Texture2D sandsTexture = (Texture2D)GD.Load("res://Sprites/Tiles/sands.png");
		for (int i = 0; i < map.sizeX; i++){
			for (int j = 0; j < map.sizeY; j++){
				Node2D tileNode = new Node2D();
				Tile tile = new Tile();
				tileNode.AddChild(tile);
				tile.tileName = "Plains";
				tile.map = map;
				tile.x = i;
				tile.y = j;
				tile.cost = 1;
				Sprite2D plainsSprite = new Sprite2D(); // Create a new Sprite2D.
				plainsSprite.Texture = plainsTexture;
				tile.tileSprite = plainsSprite;
				plainsSprite.Name = "spriteNode";
				tileNode.AddChild(plainsSprite);
				tileNode.Position = tile.GetRealPosition();
				AddChild(tileNode);
				map.tileMap[i,j] = tile;
			} 
		}
		
		Unit u1 = new Unit
		{
			unitName = "u1",
			map = map,
			baseMaxHp = 20,
			currentHp = 20,
			baseMaxStamina = 10,
			currentStamina = 10,
			x = 15,
			y = 15,
			baseMaxMovement = 10,
			currentMovement = 10
		};

		GD.Print($"Before {u1.GetUnitEffectByName("Poison")} ");
		map.unitMap[u1.x,u1.y] = u1;
		UnitEffect poison = new Poison();
		UnitEffect poison2 = new Poison();
		UnitEffect eager = new Eager();

		poison.count = 2;
		poison.linkedUnitEffects.AddLast(eager);
		eager.linkedUnitEffects.AddLast(poison);

		u1.AddUnitEffect(poison);
		u1.AddUnitEffect(poison2);
		u1.AddUnitEffect(eager);


		TileEffect rockyTerrain = new RockyTerrain(1);
		map.tileMap[3,3].AddTileEffect(rockyTerrain);

		map.tileMap[16,15].AddTileEffect(new Glue());
		map.tileMap[16,15].CountdownTileEffects(Trigger.OnEndTurn);
		map.tileMap[16,15].AddTileEffect(new Glue());

		//u1.OnEndTurn();
		//u1.OnEndTurn();

		GD.Print($"Does u1 have Poison (count): {u1.GetUnitEffectByName("Poison").count} ");
		GD.Print($"Does u1 have Poison (power): {u1.GetUnitEffectByName("Poison").power} ");
		GD.Print($"Does u1 have Eager: {u1.GetUnitEffectByName("Eager")} ");
		GD.Print($"Does map.tileMap[16,15] have Glue (count): {map.tileMap[16,15].GetTileEffectByName("Glue").count} ");
		GD.Print($"Does map.tileMap[16,15] have Glue (count): {map.tileMap[16,15].GetTileEffectByName("Glue").power} ");
	}

}
