using Godot;
using System.Collections.Generic;

public abstract class Skill
{
  public Unit source = null;
  public string name = null;
  public Type type = Type.none;
  public Category category = Category.none;
  public bool isMelee = true;
  public int basePower = -1;
  public int currentPower { get { return source.SkillStatGetter(basePower, Trigger.OnGetSkillPower, this); } } 
  public int baseRange = 1;
  public int currentRange { get { return source.SkillStatGetter(baseRange, Trigger.OnGetSkillRange, this); } } 
  // TODO overload for cells, and multiple targets
  public abstract void Fire(Unit target);
}

public class DoubleTap : Skill
{
  public DoubleTap(int basePower = 10){
	name = "DoubleTap";
	this.type = Type.Physical;
	this.category = Category.Offensive;
	this.isMelee = false;
	this.basePower = basePower;
	this.baseRange = 5;
  }
  public override void Fire(Unit target){
	source.OnAttacking(new Packet(name, type, Trigger.OnAttacking, currentPower, target, source, new LinkedList<Command>(new[]{new Damage()})));
	source.OnAttacking(new Packet(name, type, Trigger.OnAttacking, currentPower, target, source, new LinkedList<Command>(new[]{new Damage()})));
  }
}

public class BitterMedicine : Skill
{
  public BitterMedicine(int basePower = 10){
	name = "BitterMedicine";
	this.type = Type.Chemical;
	this.category = Category.Supportive;
	this.isMelee = true;
	this.basePower = basePower;
  }
  public override void Fire(Unit target){
	target.OnHealing(new Packet(name, type, Trigger.OnHealing, currentPower, target, source, new LinkedList<Command>(new[]{new Heal()})));
	Poison bitterPoison = new Poison();
	bitterPoison.source = source;
	target.AddUnitEffect(bitterPoison);
  }
}
