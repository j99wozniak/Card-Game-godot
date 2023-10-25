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
  public int baseCost = 1;
  public int currentCost { get { return source.SkillStatGetter(baseCost, Trigger.OnGetSkillCost, this); } } 
  public int baseRange = 1;
  public int currentRange { get { return source.SkillStatGetter(baseRange, Trigger.OnGetSkillRange, this); } } 
  // TODO make targeting schemes - single target/multiple selected targets/radius and self/targeted
  public void UseSkill(List<Tile> targetList){
	source.OnConsumeStamina(new Packet(name, Type.Biological, Trigger.OnConsumeStamina, currentCost, source, source, new List<Command>(new[]{new ConsumeStamina()})));
	Fire(targetList);
  }
  public void Fire(List<Tile> targetList){
  foreach(Tile targetTile in targetList){
	FireEffect(targetTile);
  }
  }
  public abstract void FireEffect(Tile targetTile);
}

public class DoubleTap : Skill
{
  public DoubleTap(int basePower = 10){
	name = "DoubleTap";
	this.type = Type.Physical;
	this.category = Category.Offensive;
	this.isMelee = false;
	this.basePower = basePower;
	this.baseCost = 5;
	this.baseRange = 5;
  }
  public override void FireEffect(Tile targetTile){
	Unit target = targetTile.GetUnit();
	source.OnAttacking(new Packet(name, type, Trigger.OnAttacking, currentPower, target, source, new List<Command>(new[]{new Damage()})));
	source.OnAttacking(new Packet(name, type, Trigger.OnAttacking, currentPower, target, source, new List<Command>(new[]{new Damage()})));
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
	this.baseCost = 2;
	this.baseRange = 2;
  }
  public override void FireEffect(Tile targetTile){
	Unit target = targetTile.GetUnit();
	target.OnHealing(new Packet(name, type, Trigger.OnHealing, currentPower, target, source, new List<Command>(new[]{new Heal()})));
	Poison bitterPoison = new Poison();
	bitterPoison.source = source;
	target.AddUnitEffect(bitterPoison);
  }
}
