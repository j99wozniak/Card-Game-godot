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
	public Type type = Type.none;
	public Trigger trigger = Trigger.none;
	public Trigger countdownTrigger = Trigger.none;
	public LinkedList<TileEffect> linkedTileEffects = new();
	public LinkedList<UnitEffect> linkedUnitEffects = new(); 
	public LinkedList<TileEffect> childrenTileEffects = new(); // TODO should be removed when parent effect is dicarded, but not the other way

	public virtual void Execute(Packet packet){}
	public virtual void MovementExecute(ref float movementCost, Tile tile, Unit movingUnit){}
	public virtual void Getter(ref int valueToModify){}
	public void Countdown(){
		foreach(TileEffect e in linkedTileEffects){
			e.CountdownChild();
		}
		foreach(UnitEffect e in linkedUnitEffects){
			e.CountdownChild();
		}
	  if(count <= 100){
			count--;
			if(count<=0){
				RemoveThisEffect();
			}
	  }
	}
	public void CountdownChild(){
		if(count <= 100){
			count--;
			if(count<=0){
				RemoveThisEffect();
			}
	  }
	}
	public void RemoveThisEffect(){
		parentTile.RemoveTileEffect(this);
		foreach(TileEffect e in linkedTileEffects){
			e.linkedTileEffects.Remove(this);
			e.RemoveThisEffect();
		}
		foreach(UnitEffect e in linkedUnitEffects){
			e.linkedTileEffects.Remove(this);
			e.RemoveThisEffect();
		}
		linkedTileEffects = null;
		linkedUnitEffects = null;
	}
}

// For now lowers all damage received by `power` 
public class RockyTerrain : TileEffect
{
  public RockyTerrain(int power = 1){
	name = "RockyTerrain";
	type = Type.Physical;
	trigger = Trigger.OnDamage;
	priority = 1;
	this.power = power;
  }
  public override void Execute(Packet packet)
  {
	// TODO apply only on physical
	GD.Print($"Applying RockyTerrain: {packet.value}-{power}, {packet.trigger}");
	if(packet.trigger == Trigger.OnAttacking || packet.trigger == Trigger.OnDamage){
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
	type = Type.Chemical;
	trigger = Trigger.OnMovingThrough;
	priority = 5;
  }
  public override void MovementExecute(ref float movementCost, Tile tile, Unit movingUnit)
  {
		if(!movingUnit.HasEffect("Skip")){
			movementCost = 10;
		}
  }
}
