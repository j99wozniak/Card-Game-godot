using Godot;
using System.Collections.Generic;

public abstract class Skill
{
  public Unit source;
  public string name = null;
  public Type type;
  public int power = 0;
  // TODO overload for cells, and multiple targets
  public abstract void Fire(Unit target);
}

public class DoubleTap : Skill
{
  public DoubleTap(int power = 10){
	name = "DoubleTap";
	this.power = power;
	this.type = Type.Physical;
  }
  public override void Fire(Unit target){
	source.OnAttacking(new Packet(name, type, Trigger.OnAttacking, power, target, source, new LinkedList<Command>(new[]{new Damage()})));
	source.OnAttacking(new Packet(name, type, Trigger.OnAttacking, power, target, source, new LinkedList<Command>(new[]{new Damage()})));
  }
}

public class BitterMedicine : Skill
{
  public BitterMedicine(int power = 10){
	name = "BitterMedicine";
	this.power = power;
	this.type = Type.Chemical;
  }
  public override void Fire(Unit target){
	target.OnHealing(new Packet(name, type, Trigger.OnHealing, power, target, source, new LinkedList<Command>(new[]{new Heal()})));
	Poison bitterPoison = new Poison(power / 10);
	bitterPoison.source = source;
	bitterPoison.target = target;
	target.AddUnitEffect(bitterPoison);
  }
}
