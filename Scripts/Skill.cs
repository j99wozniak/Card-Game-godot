using System.Collections.Generic;

public abstract class Skill
{
  public Unit source = null;
  public string name = null;
  public Type type = Type.none;
  public Category category = Category.none;
  public int numberOfTargets = 0;
  public Target targetQualifier = Target.none;
  public bool canTargetSameTileAgain = true;
  public int splashZoneRange = 0;
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
    source.player.game.CheckConditions();
  }
  public void Fire(List<Tile> targetList){
    foreach(Tile targetTile in targetList){
      if(Targeter.IsTileViableTarget(this, targetTile)){
        FireEffect(targetTile);
      }
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
    this.numberOfTargets = 2;
    this.targetQualifier = Target.EnemyUnit;
    this.splashZoneRange = 0;
    this.isMelee = false;
    this.basePower = 10;
    this.baseCost = 7;
    this.baseRange = 5;
  }
  public override void FireEffect(Tile targetTile){
    Unit target = targetTile.GetUnit();
    Directions flip = Directions.none;
    if(Tile.GetDirection(source.GetTile(), targetTile).Contains(Directions.LEFT))
      flip = Directions.LEFT;
    if(Tile.GetDirection(source.GetTile(), targetTile).Contains(Directions.RIGHT))
      flip = Directions.RIGHT;
    source.PlayAnimation("right_skill", flip);
    source.OnAttacking(new Packet(this, target, currentPower, new Damage()));
  }
}

public class Shot : Skill
{
  public Shot(){
    name = "Shot";
    this.type = Type.Physical;
    this.category = Category.Offensive;
    this.numberOfTargets = 1;
    this.targetQualifier = Target.EnemyUnit;
    this.splashZoneRange = 0;
    this.isMelee = false;
    this.basePower = 10;
    this.baseCost = 5;
    this.baseRange = 5;
  }
  public override void FireEffect(Tile targetTile){
    Unit target = targetTile.GetUnit();
    Directions flip = Directions.none;
    if(Tile.GetDirection(source.GetTile(), targetTile).Contains(Directions.LEFT))
      flip = Directions.LEFT;
    if(Tile.GetDirection(source.GetTile(), targetTile).Contains(Directions.RIGHT))
      flip = Directions.RIGHT;
    source.PlayAnimation("right_skill", flip);
    source.OnAttacking(new Packet(this, target, currentPower, new Damage()));
  }
}

public class BitterMedicine : Skill
{
  public BitterMedicine(){
    name = "BitterMedicine";
    this.type = Type.Chemical;
    this.category = Category.Supportive;
    this.numberOfTargets = 1;
    this.targetQualifier = Target.AllyUnit;
    this.splashZoneRange = 0;
    this.isMelee = true;
    this.basePower = 10;
    this.baseCost = 2;
    this.baseRange = 2;
  }
  public override void FireEffect(Tile targetTile){
    Unit target = targetTile.GetUnit();
    target.OnHealing(new Packet(this, target, currentPower, new Heal()));
    Poison bitterPoison = new Poison(){source = source};;
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
    this.numberOfTargets = 1;
    this.targetQualifier = Target.AllyUnit;
    this.splashZoneRange = 0;
    this.isMelee = true;
    this.basePower = 10;
    this.baseCost = 2;
    this.baseRange = 4;
  }
  public override void FireEffect(Tile targetTile){
    Unit target = targetTile.GetUnit();
    target.GetUnitEffectByName("ApplyHealingAura")?.RemoveThisEffect();
    UnitEffect removeHealingAura = new RemoveHealingAura(){source = source};
    target.AddUnitEffect(removeHealingAura);
    UnitEffect applyHealingAura = new ApplyHealingAura(){source = source};
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
    this.numberOfTargets = 1;
    this.targetQualifier = Target.Unit;
    this.splashZoneRange = 0;
    this.isMelee = true;
    this.basePower = 10;
    this.baseCost = 2;
    this.baseRange = 4;
  }
  public override void FireEffect(Tile targetTile){
    Unit target = targetTile.GetUnit();
    target.OnStartMove();
    target.map.unitMap[target.x, target.y] = null;
    target.x = 10;
    target.y = 10;
    target.map.unitMap[target.x, target.y] = target;
    target.parentNode.Position = target.GetRealPosition();
    target.OnEndMove();
  }
}

public class Summon : Skill
{
  Unit unit = null;
  List<Unit> deckToRemoveFrom = null;
  public Summon(Unit unit, List<Unit> deckToRemoveFrom = null){
    name = "Summon " + unit.unitName;
    this.type = Type.Physical;
    this.category = Category.Utility;
    this.numberOfTargets = 1;
    this.targetQualifier = Target.EmptyTile;
    this.splashZoneRange = 0;
    this.isMelee = false;
    this.basePower = 0;
    this.baseCost = 0;
    this.baseRange = 4;
    this.unit = unit;
    this.deckToRemoveFrom = deckToRemoveFrom;
  }
  public override void FireEffect(Tile targetTile){
    unit.player = source.player;
    unit.x = targetTile.x;
    unit.y = targetTile.y;
    unit.SetNewID();
    unit.AddUnitToMap(targetTile.map);
    if(deckToRemoveFrom!=null){
      deckToRemoveFrom.Remove(unit);
    }
  }
}

public class GranadeSack : Skill
{
  public GranadeSack(){
    name = "GranadeSack";
    this.type = Type.Physical;
    this.category = Category.Offensive;
    this.numberOfTargets = 3;
    this.targetQualifier = Target.Any;
    this.splashZoneRange = 2;
    this.isMelee = false;
    this.basePower = 5;
    this.baseCost = 20;
    this.baseRange = 4;
  }
  public override void FireEffect(Tile targetTile){
    
  }
}

