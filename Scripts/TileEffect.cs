using Godot;
using System.Collections.Generic;

public class TileEffect
{
	public string name = null;
	public Tile parentTile = null;
	public Unit source;
	public int count = 100; // Default (100) means the countdown won't go down
	public int priority = 0; // The higher the priority the sooner it will be executed
	public int power = 0;
	public string type = null;
	public string trigger = null;
	public string countDownTrigger = null; // TODO like onDamage or onMoving
	public LinkedList<TileEffect> linkedTileEffects; // TODO should remove related effects when it's removed
	public LinkedList<UnitEffect> linkedUnitEffects; 
	public LinkedList<TileEffect> childrenTileEffects; // TODO should be removed when parent effect is dicarded, but not the other way
	public LinkedList<UnitEffect> childrenUnitEffects; 

	public virtual void Execute(Packet packet){}
	public virtual void MovementExecute(ref float movementCost, Tile tile, Unit movingUnit){}
	public virtual void Getter(ref int valueToModify){}
	public void CountDown(){
	  // TODO
	}
}

// For now lowers all damage received by `power` 
public class RockyTerrain : TileEffect
{
  public RockyTerrain(int power = 1){
	name = "RockyTerrain";
	type = "Physical";
	trigger = "OnDamage";
	priority = 1;
	this.power = power;
  }
  public override void Execute(Packet packet)
  {
	// TODO apply only on physical
	GD.Print($"Applying RockyTerrain: {packet.value}-{power}, {packet.trigger}");
	if(packet.trigger == "OnAttacking" || packet.trigger == "OnDamage"){
	  packet.value = packet.value - power > 0 ? packet.value - power : 0;
	}
	GD.Print("Also, the guy that's benefiting from this is called " + parentTile.GetUnit().unitName);
  }
}


// MOVEMENT EFFECTS

// Changes the cost of the tile to 10
public class Glue : TileEffect
{
  public Glue(){
	name = "Glue";
	type = "Chemical";
	trigger = "OnMovingThrough";
	priority = 5;
  }
  public override void MovementExecute(ref float movementCost, Tile tile, Unit movingUnit)
  {
		if(!movingUnit.HasEffect("Skip")){
			movementCost = 10;
		}
  }
}
