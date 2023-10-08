using Godot;
using System.Collections;
using System.Collections.Generic;

public abstract class UnitEffect
{
	public string name = null;
	public Unit parentUnit = null;
	public Unit target = null;
	public Unit source = null;
	public int count = 100; // Default (100) means the countdown won't go down
	public int priority = 0; // The higher the priority the sooner it will be executed
	public int power = 0;
	public string type = null;
	public string trigger = null;
	public string countDownTrigger = null; // TODO like onDamage or onMoving
	public LinkedList<TileEffect> linkedTileEffects; // TODO should remove related effects when it's removed
	public LinkedList<UnitEffect> linkedUnitEffects;

	public virtual void Execute(Packet packet){}
	public virtual void MovementExecute(ref float movementCost, Tile tile, Unit movingUnit){}
	public void CountDown(){ //
	  if(count <= 100){
		count--;
		if(count<=0){
			target.RemoveUnitEffect(this);
		}
	  }
	}
}

// At the end of turn, directly shoot packet with command Damage of power `power` and type `chemical` to the target
public class Poison : UnitEffect
{
	public Poison(int power = 5){
	  name = "Poison";
	  type = "Chemical";
	  trigger = "OnEndTurn";
	  priority = 5;
	  this.power = power;
	}
	public override void Execute(Packet packet = null){
	  target.OnDamage(new Packet(name, type, trigger, power, target, source, new LinkedList<Command>(new[]{new Damage()})));
	}
}

// If the packet comes from someone attacking, and target has stamina and packet is larger than one, change that packet power to 1
// TODO should check if the packet has command Damage in it?
public class Dodge : UnitEffect
{
  public Dodge(){
	name = "Dodge";
	type = "Physical";
	trigger = "OnDamage";
	priority = 10;
  }
  public override void Execute(Packet packet)
  {
	// TODO apply only on physical
	GD.Print($"Applying dodge: currentStamina {target.currentStamina}, damage to negate {packet.value}, {packet.trigger}");
	if(target.currentStamina - 5 >= 0 && packet.value > 1 && packet.trigger == "OnAttacking"){
		packet.value = 1;
		target.currentStamina = target.currentStamina - 5;
	}
  }
}

// For now lowers all damage received by `power` 
public class Armor : UnitEffect
{
  public Armor(int power = 5){
	name = "Armor";
	type = "Physical";
	trigger = "OnDamage";
	priority = 5;
	this.power = power;
  }
  public override void Execute(Packet packet)
  {
	// TODO apply only on physical
	GD.Print($"Applying armor: {packet.value}-{power}, {packet.trigger}");
	if(packet.trigger == "OnAttacking" || packet.trigger == "OnDamage"){
	  packet.value = packet.value - power > 0 ? packet.value - power : 0;
	}
  }
}

public class Counter : UnitEffect
{
  public Counter(int power = 5){
	name = "Counter";
	type = "Physical";
	trigger = "OnDamage";
	priority = 20;
	this.power = power;
  }
  public override void Execute(Packet packet)
  {
	GD.Print($"Applying counter: {packet.value}-{power}, {packet.trigger}");
	  // TODO check for team and for infinite counter loop
	  // Think if this shouldn't be considered as an attack
	if(packet.source != packet.target && (packet.trigger == "OnAttacking")){
	  int reflectedDamage = packet.value - power >= 0 ? power : packet.value;
	  packet.value = packet.value - power >= 0 ? packet.value - power : 0;
	  target.OnDamage(new Packet(name, type, "OnDamage", reflectedDamage, packet.source, packet.target, new LinkedList<Command>(new[]{new Damage()})));
	}
  }
}

public class PreciseShots : UnitEffect
{
  public PreciseShots(int power = 5){
	name = "PreciseShots";
	type = "Physical";
	trigger = "OnAttacking";
	priority = 20;
	this.power = power;
  }
  public override void Execute(Packet packet)
  {
	GD.Print($"Applying PreciseShots: {packet.value}+{power}");
	packet.value += power;
  }
}

// Movement effects
public class Skip : UnitEffect
{
  public Skip(){
	name = "Skip";
	type = "Physical";
	trigger = "OnMoving";
	priority = 5;
  }
  public override void MovementExecute(ref float movementCost, Tile tile, Unit movingUnit)
  {
		if(movementCost > 1 && movementCost < 5){
			movementCost = 1;
		}
  }
}
