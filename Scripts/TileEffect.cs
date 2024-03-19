using Godot;
using System.Collections.Generic;

public class TileEffect
{
  public static int currentTileEffectID = 1;
  public int ID = currentTileEffectID++;
  public string name = null;
  public Tile parentTile = null;
  public Unit source;
  public int count = 100; // Default (100) means the countdown won't go down
  public int priority = 0; // The higher the priority the sooner it will be executed
  public int power = 0;
  public bool stackable = false;
  public bool preset = false;
  public Type type = Type.none;
  public Trigger trigger = Trigger.none;
  public Trigger countdownTrigger = Trigger.none;
  public LinkedList<TileEffect> linkedTileEffects = new();
  public LinkedList<UnitEffect> linkedUnitEffects = new(); 

  public virtual void Execute(Packet packet){}
  public virtual void MovementExecute(ref float movementCost, Tile tile, Unit movingUnit){} // Executed when calculating cost of a tile when attempting to move through it
  public virtual void Getter(ref int valueToModify){}
  public virtual void SkillGetter(ref int valueToModify, Skill skill){}
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
public class ShieldTrees : TileEffect
{
  public ShieldTrees(){
    name = "ShieldTrees";
    type = Type.Physical;
    trigger = Trigger.OnDamage;
    priority = 1;
    this.power = 1;
  }
  public override void Execute(Packet packet){
    // TODO apply only on physical
    if(packet.trigger == Trigger.OnAttacking || packet.trigger == Trigger.OnDamage){
      GD.Print($"Applying ShieldTrees: {packet.value} - {power}, {packet.name}, for {parentTile.x}x:{parentTile.y}y");
      packet.value = packet.value - power > 0 ? packet.value - power : 0;
    }
  }
}


public class Flame : TileEffect
{
  public Flame(){
    name = "Flame";
    type = Type.Elemental;
    trigger = Trigger.OnEndTurn;
    priority = 5;
    this.power = 3;
  }
  public override void Execute(Packet packet){
    // TODO apply only on physical
    GD.Print($"Applying Flame: {power}, for {parentTile.GetUnit().unitName}");
    parentTile.GetUnit().OnDamage(new Packet(name, type, trigger, power, parentTile.GetUnit(), source, new Damage()));;
  }
}

public class HealingAuraTile : TileEffect
{
  public HealingAuraTile(){
    name = "HealingAuraTile";
    type = Type.Chemical;
    trigger = Trigger.OnEndTurn;
    priority = 5;
    this.power = 5;
  }
  public override void Execute(Packet packet = null){
    Unit target = parentTile.GetUnit();
    if(target != null){
      source.OnHealing(new Packet("HealingAuraTile", type, Trigger.OnHealing, power, target, source, new Heal()));
    }
  }
}

// MOVEMENT EFFECTS

public class JumpPad : TileEffect
{
  public JumpPad(){
    name = "JumpPad";
    type = Type.Physical;
    trigger = Trigger.OnMoving;
    priority = 5;
  }
  public override void MovementExecute(ref float movementCost, Tile tile, Unit movingUnit){
    if(movementCost >= 5 && movementCost < 10){
      movementCost -= 4;
    }
  }
}

// Changes the cost of the tile to 10
public class Glue : TileEffect
{
  public Glue(){
    name = "Glue";
    type = Type.Chemical;
    trigger = Trigger.OnMovingThrough;
    countdownTrigger = Trigger.OnEndTurn;
    power = 10;
    stackable = true;
    count = 2;
    priority = 5;
  }
  public override void MovementExecute(ref float movementCost, Tile tile, Unit movingUnit){
    if(movingUnit.GetUnitEffectByName("Skip") == null){
      movementCost = 10;
    }
  }
}


public class RockyTerrain : TileEffect
{
  public RockyTerrain(){
    name = "RockyTerrain";
    type = Type.Physical;
    trigger = Trigger.OnMovingThrough;
    priority = 1;
    this.power = 1;
  }
  public override void MovementExecute(ref float movementCost, Tile tile, Unit movingUnit){
    if(movingUnit.GetUnitEffectByName("Skip") == null && movingUnit.GetUnitEffectByName("Fly") == null){
      movementCost = 5;
    }
  }
}
