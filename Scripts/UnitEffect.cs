using Godot;
using System.Collections.Generic;

public abstract class UnitEffect
{
  public static int currentUnitEffectID = 1;
  public int ID = currentUnitEffectID++;
  public string name = null;
  public Unit parentUnit = null;
  public Unit source = null;
  public int count = 100; // Default (100) means the countdown won't go down
  public int priority = 0; // The higher the priority the sooner it will be executed
  public int power = 0;
  public bool stackable = false;
  public bool removedOnDeath = true;
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
    GD.Print($"Removing {this.name}({this.ID}) on {parentUnit.unitName}");
    parentUnit.RemoveUnitEffect(this);
    foreach(TileEffect e in linkedTileEffects){
      e.linkedUnitEffects.Remove(this);
      e.RemoveThisEffect();
    }
    foreach(UnitEffect e in linkedUnitEffects){
      e.linkedUnitEffects.Remove(this);
      e.RemoveThisEffect();
    }
    linkedTileEffects = null;
    linkedUnitEffects = null;
  }
}

// At the end of turn, directly shoot packet with command Damage of power `power` and type `chemical` to the target
public class Poison : UnitEffect
{
  public Poison(int power = 5){
    name = "Poison";
    type = Type.Chemical;
    trigger = Trigger.OnEndTurn;
    priority = 5;
    this.power = power;
    count = 5;
    stackable = true;
  }
  public override void Execute(Packet packet= null){
    parentUnit.OnDamage(new Packet(name, type, trigger, power, parentUnit, source, new Damage()));
  }
}

// If the packet comes from someone attacking, and target has stamina and packet is larger than one, change that packet power to 1
// TODO should check if the packet has command Damage in it?
public class Dodge : UnitEffect
{
  public Dodge(){
    name = "Dodge";
    type = Type.Physical;
    trigger = Trigger.OnDamage;
    priority = 10;
  }
  public override void Execute(Packet packet)
  {
    // TODO apply only on physical
    if(parentUnit.currentStamina - 5 >= 0 && packet.value > 1 && packet.trigger == Trigger.OnAttacking){
      
      GD.Print($"Applying dodge: currentStamina {parentUnit.currentStamina}, damage to negate {packet.value}, {packet.trigger}");
      packet.value = 1;
      parentUnit.OnConsumeStamina(new Packet(name, Type.Biological, Trigger.OnConsumeStamina, 5, parentUnit, parentUnit, new ConsumeStamina()));
    }
  }
}

// For now lowers all damage received by `power` 
public class Armor : UnitEffect
{
  public Armor(int power = 5){
    name = "Armor";
    type = Type.Physical;
    trigger = Trigger.OnDamage;
    priority = 5;
    this.power = power;
  }
  public override void Execute(Packet packet)
  {
    // TODO apply only on physical
    GD.Print($"Applying armor: {packet.value}-{power}, {packet.trigger}");
    if(packet.trigger == Trigger.OnAttacking || packet.trigger == Trigger.OnDamage){
      packet.value = packet.value - power > 0 ? packet.value - power : 0;
    }
  }
}

public class Counter : UnitEffect
{
  public Counter(int power = 5){
    name = "Counter";
    type = Type.Physical;
    trigger = Trigger.OnDamage;
    priority = 20;
    this.power = power;
  }
  public override void Execute(Packet packet){
    GD.Print($"Applying counter: {packet.value}-{power}, {packet.trigger}");
    // TODO check for team and for infinite counter loop
    // Think if this shouldn't be considered as an attack
    if(packet.source != packet.target && (packet.trigger == Trigger.OnAttacking)){
      int reflectedDamage = packet.value - power >= 0 ? power : packet.value;
      packet.value = packet.value - power >= 0 ? packet.value - power : 0;
      parentUnit.OnDamage(new Packet(name, type, Trigger.OnDamage, reflectedDamage, packet.source, packet.target, new List<Command>(new[]{new Damage()})));
    }
  }
}

// Movement effects
public class Skip : UnitEffect
{
  public Skip(){
    name = "Skip";
    type = Type.Physical;
    trigger = Trigger.OnMoving;
    priority = 5;
  }
  public override void MovementExecute(ref float movementCost, Tile tile, Unit movingUnit){
    if(movementCost > 1 && movementCost < 5){
      movementCost = 1;
    }
  }
}

public class RemoveHealingAura : UnitEffect
{
  public RemoveHealingAura(){
    name = "RemoveHealingAura";
    type = Type.Chemical;
    trigger = Trigger.OnStartMove;
    priority = 5;
  }
  public override void Execute(Packet packet= null){
    for(LinkedListNode<TileEffect> e = linkedTileEffects.First; e != null;){
      e.Value.RemoveThisEffect();
      LinkedListNode<TileEffect> n = e.Next;
      linkedTileEffects.Remove(e);
      e = n;
    }
  }
}

public class ApplyHealingAura : UnitEffect
{
  public ApplyHealingAura(){
    name = "ApplyHealingAura";
    type = Type.Chemical;
    trigger = Trigger.OnEndMove;
    priority = 5;
  }
  public override void Execute(Packet packet= null){
    //TODO might want to add check for Auras range
    Dictionary<(int x, int y), float> toApplyAuraTo = Range.GetRadius(parentUnit.map, parentUnit.x, parentUnit.y, 4);
    foreach (var kvp in toApplyAuraTo) {
      TileEffect HealingAuraTile = new HealingAuraTile();
      HealingAuraTile.source = parentUnit;
      linkedUnitEffects.First.Value.linkedTileEffects.AddLast(HealingAuraTile);
      parentUnit.map.tileMap[kvp.Key.x, kvp.Key.y].AddTileEffect(HealingAuraTile);
    }
  }
}

// Getter effects
public class Eager : UnitEffect
{
  public Eager(){
    name = "Eager";
    type = Type.Physical;
    trigger = Trigger.OnGetMaxMovement;
    priority = 5;
  }
  public override void Getter(ref int movementPoints){
    movementPoints += 10;
  }
}

public class PreciseShots : UnitEffect
{
  public PreciseShots(){
    name = "PreciseShots";
    type = Type.Physical;
    trigger = Trigger.OnGetSkillPower;
    priority = 20;
    this.power = 5;
  }
  public override void SkillGetter(ref int valueToModify, Skill skill){
    GD.Print($"Applying PreciseShots: {valueToModify}+{power}");
    if(skill.category == Category.Offensive && skill.isMelee == false){
      valueToModify += power;
    }
  }
}

public class Sniper : UnitEffect
{
  public Sniper(){
    name = "Sniper";
    type = Type.Physical;
    trigger = Trigger.OnGetSkillRange;
    priority = 5;
  }
  public override void SkillGetter(ref int skillRange, Skill skill)
  {
    skillRange += 5;
  }
}
