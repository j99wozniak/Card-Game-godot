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
    source.OnConsumeStamina(new Packet(this, new ConsumeStamina()));
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
  public DoubleTap(){
    name = "DoubleTap";
    this.type = Type.Physical;
    this.category = Category.Offensive;
    this.isMelee = false;
    this.basePower = 10;
    this.baseCost = 5;
    this.baseRange = 5;
  }
  public override void FireEffect(Tile targetTile){
    Unit target = targetTile.GetUnit();
    source.PlayAnimation("skill_right");
    source.OnAttacking(new Packet(this, target, currentPower, new Damage()));
    source.PlayAnimation("skill_right");
    source.OnAttacking(new Packet(this, target, currentPower, new Damage()));
  }
}

public class BitterMedicine : Skill
{
  public BitterMedicine(){
    name = "BitterMedicine";
    this.type = Type.Chemical;
    this.category = Category.Supportive;
    this.isMelee = true;
    this.basePower = 10;
    this.baseCost = 2;
    this.baseRange = 2;
  }
  public override void FireEffect(Tile targetTile){
    Unit target = targetTile.GetUnit();
    target.OnHealing(new Packet(this, target, currentPower, new Heal()));
    Poison bitterPoison = new Poison();
    bitterPoison.source = source;
    target.AddUnitEffect(bitterPoison);
  }
}

public class HealingAura : Skill
{
  public HealingAura(){
    name = "HealingAura";
    this.type = Type.Chemical;
    this.category = Category.Supportive;
    this.isMelee = true;
    this.basePower = 10;
    this.baseCost = 2;
    this.baseRange = 4;
  }
    public override void FireEffect(Tile targetTile){
      Unit target = targetTile.GetUnit();
      UnitEffect removeHealingAura = new RemoveHealingAura();
      target.AddUnitEffect(removeHealingAura);
      UnitEffect applyHealingAura = new ApplyHealingAura();
      target.AddUnitEffect(applyHealingAura);
      removeHealingAura.linkedUnitEffects.AddFirst(applyHealingAura);
      applyHealingAura.linkedUnitEffects.AddFirst(removeHealingAura);
      applyHealingAura.Execute(null);
    }
}

public class Teleport : Skill
{
  public Teleport(){
    name = "Teleport";
    this.type = Type.Energy;
    this.category = Category.Utility;
    this.isMelee = true;
    this.basePower = 10;
    this.baseCost = 2;
    this.baseRange = 4;
  }
    public override void FireEffect(Tile targetTile){
      Unit target = targetTile.GetUnit();
      GD.Print($"{target.GetUnitEffectByName("RemoveHealingAura", Trigger.OnStartMove)}, {target.GetUnitEffectByName("ApplyHealingAura", Trigger.OnEndMove)}");
      target.OnStartMove();
      target.map.unitMap[target.x, target.y] = null;
      target.x = 10;
      target.y = 10;
      target.map.unitMap[target.x, target.y] = target;
      target.parentNode.Position = target.GetRealPosition();
      target.OnEndMove();
    }
}