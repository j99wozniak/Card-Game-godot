using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Unit : Node
{
  // TODO maybe these triggers should be enums?
  public Dictionary<Trigger, LinkedList<UnitEffect>> unitEffects = new Dictionary<Trigger, LinkedList<UnitEffect>>{
  {Trigger.OnBeginTurn, new LinkedList<UnitEffect>()},
  {Trigger.OnEndTurn, new LinkedList<UnitEffect>()},
  {Trigger.OnDamage, new LinkedList<UnitEffect>()},
  {Trigger.OnHeal, new LinkedList<UnitEffect>()},
  {Trigger.OnAttacking, new LinkedList<UnitEffect>()},
  {Trigger.OnConsumeStamina, new LinkedList<UnitEffect>()},
  {Trigger.OnReplenishStamina, new LinkedList<UnitEffect>()},
  {Trigger.OnHealing, new LinkedList<UnitEffect>()},
  {Trigger.OnStartMove, new LinkedList<UnitEffect>()},
  {Trigger.OnEndMove, new LinkedList<UnitEffect>()},
  {Trigger.OnMoving, new LinkedList <UnitEffect>()},

  {Trigger.OnGetMaxHp, new LinkedList<UnitEffect>()},
  {Trigger.OnGetMaxStamina, new LinkedList<UnitEffect>()},
  {Trigger.OnGetMaxMovement, new LinkedList<UnitEffect>()},

  {Trigger.OnGetSkillPower, new LinkedList<UnitEffect>()},
  {Trigger.OnGetSkillCost, new LinkedList<UnitEffect>()},
  {Trigger.OnGetSkillRange, new LinkedList<UnitEffect>()}
  };

  public LinkedList<Skill> skills = new();

  public GameMap map;

  public string unitName;

  public int team;

  public int baseMaxHp;
  public int maxHp { get { return StatGetter(baseMaxHp, Trigger.OnGetMaxHp); } } 
  public int currentHp;

  public int baseMaxStamina;
  public int maxStamina { get { return StatGetter(baseMaxStamina, Trigger.OnGetMaxStamina); } }
  public int currentStamina;

  public int baseMaxMovement;
  public int maxMovement{ get { return StatGetter(baseMaxMovement, Trigger.OnGetMaxMovement); } }
  public int currentMovement;

  public int x;
  public int y;

  public Unit(GameMap map, string unitName, int team, int baseMaxHp, int baseMaxStamina, int baseMaxMovement, int x, int y){
    this.map = map;
    this.unitName = unitName;
    this.team = team;
    this.baseMaxHp = baseMaxHp;
    currentHp = baseMaxHp;
    this.baseMaxStamina = baseMaxStamina;
    currentStamina = baseMaxStamina;
    this.baseMaxMovement = baseMaxMovement;
    currentMovement = baseMaxMovement;
    this.x = x;
    this.y = y;
    map.unitMap[x,y] = this;
  }

  // TODO Could probably make this smarter.
  // like setting an ID for each sequence of effects, 
  // and then checking if the sequence is the same
  public int StatGetter(int baseStat, Trigger trigger){
    int fetchedStat = baseStat;
    foreach(TileEffect e in GetTile().tileEffects){
      if(e.trigger == trigger){
      e.Getter(ref fetchedStat);
      }
    }
    foreach(UnitEffect e in unitEffects[trigger]){
      e.Getter(ref fetchedStat);
    }
    return fetchedStat;
  }

  public int SkillStatGetter(int baseStat, Trigger trigger, Skill skill){
    int fetchedStat = baseStat;
    foreach(TileEffect e in GetTile().tileEffects){
      if(e.trigger == trigger){
      e.SkillGetter(ref fetchedStat, skill);
      }
    }
    foreach(UnitEffect e in unitEffects[trigger]){
      e.SkillGetter(ref fetchedStat, skill);
    }
    return fetchedStat;
  }

  public Tile GetTile(){
    return map.tileMap[x,y];
  }

  public void AddUnitEffect(UnitEffect effect){
    UnitEffect existingEffect = GetUnitEffectByName(effect.name, effect.trigger);
    if(existingEffect == null){
      effect.parentUnit = this;
      if(!unitEffects[effect.trigger].Any()){
      unitEffects[effect.trigger].AddLast(effect);
      }
      else{
      for(LinkedListNode<UnitEffect> e = unitEffects[effect.trigger].First; e != null; ){
        if(e.Value.priority < effect.priority){
        unitEffects[effect.trigger].AddBefore(e, effect);
        return;
        }
        e = e.Next;
      }
      unitEffects[effect.trigger].AddLast(effect);
      }
      return;
    }
    if(existingEffect.stackable){
      existingEffect.power += effect.power;
      effect.power = existingEffect.power;
    }
    if(existingEffect.count < effect.count){
      LinkedListNode<UnitEffect> e = unitEffects[existingEffect.trigger].First;
      while(e != null){
      if(e.Value == existingEffect){
        e.Value = effect;
        break;
      }
      e = e.Next;
      }
    }
  }
  
  public UnitEffect GetUnitEffectByName(string effectName, Trigger effectTrigger = Trigger.none){
    if(effectTrigger != Trigger.none){
      foreach(UnitEffect e in unitEffects[effectTrigger]){
      if(e.name == effectName){
        return e;
      }
      }
      return null;
    }
    foreach(KeyValuePair<Trigger, LinkedList<UnitEffect>> list in unitEffects){
      foreach(UnitEffect e in list.Value){
      if(e.name == effectName){
        return e;
      }
      }
    }
    return null;
  }

  // TODO remove UnitEffect by name (and source?)
  public void RemoveUnitEffect(UnitEffect effect){
    unitEffects[effect.trigger].Remove(effect);
  }

  // TODO check for duplicates. Perserve only strongest
  public void AddSkill(Skill skill){
    skill.source = this;
    skills.AddLast(skill);
  }

  public void RemoveSkill(Skill skill){
    skills.Remove(skill);
  }

  public Skill GetSkillByName(string skillName){
    foreach(Skill s in skills){
      if(s.name == skillName){
      return s;
      }
    }
    return null;
  }

  // TODO create a function for first selecting skill, and then targetting 
  // Maybe then we no longer will have skillName, but just reference to picked skill?
  public void UseSkill(string skillName, List<Tile> targetList){
    foreach(Skill s in skills){
      if(s.name == skillName){
      s.UseSkill(targetList);
      return;
      }
    }
  }

  public void OnBeginTurn(){
    ExecuteEffects(Trigger.OnBeginTurn);
    CountdownUnitEffects(Trigger.OnBeginTurn);
  }

  public void OnEndTurn(){
    ExecuteEffects(Trigger.OnEndTurn);
    CountdownUnitEffects(Trigger.OnEndTurn);
  }

  // Receive a packet
  public void OnDamage(Packet damage){
    GD.Print($"{this.unitName} Geting damaged with: {damage.name}-{damage.value}");
    ExecuteEffects(Trigger.OnDamage, damage);
    GD.Print($"{this.unitName} Final value of damage {damage.name}-{damage.value}");
    ApplyCommands(damage);
    CountdownUnitEffects(Trigger.OnDamage);
  }

  public void OnHeal(Packet heal){
    ExecuteEffects(Trigger.OnHeal, heal);
    ApplyCommands(heal);
    CountdownUnitEffects(Trigger.OnHeal);
  }

  public void OnConsumeStamina(Packet consumeStamina){
    ExecuteEffects(Trigger.OnConsumeStamina, consumeStamina);
    ApplyCommands(consumeStamina);
    CountdownUnitEffects(Trigger.OnConsumeStamina);
  }

  public void OnReplenishStamina(Packet replenishStamina){
    ExecuteEffects(Trigger.OnReplenishStamina, replenishStamina);
    ApplyCommands(replenishStamina);
    CountdownUnitEffects(Trigger.OnReplenishStamina);
  }

  // Fire a packet
  public void OnAttacking(Packet attack){
    GD.Print($"{this.unitName} Attacking with: {attack.name}-{attack.value}->{attack.target.unitName}");
    ExecuteEffects(Trigger.OnAttacking , attack);
    GD.Print($"{this.unitName} Final attack value: {attack.name}-{attack.value}->{attack.target.unitName}");
    attack.target.OnDamage(attack);
    CountdownUnitEffects(Trigger.OnAttacking);
  }

  public void OnHealing(Packet healing){
    ExecuteEffects(Trigger.OnHealing, healing);
    GD.Print($"{this.unitName} Final Healing with: {healing.name}-{healing.value}->{healing.target.unitName}");
    healing.target.OnHeal(healing);
    CountdownUnitEffects(Trigger.OnHealing);
  }

  public void OnStartMove(){

  }

  public void OnMoving(ref float movementCost, Tile tile){
    foreach(TileEffect e in tile.tileEffects){
      if(e.trigger == Trigger.OnMovingThrough){
      e.MovementExecute(ref movementCost, tile, this);
      }
    }
    foreach(TileEffect e in GetTile().tileEffects){
      if(e.trigger == Trigger.OnMoving){
      e.MovementExecute(ref movementCost, tile, this);
      }
    }
    foreach(UnitEffect e in unitEffects[Trigger.OnMoving]){
      e.MovementExecute(ref movementCost, tile, this);
    }
  }

  void CountdownUnitEffects(Trigger countdownTrigger){
    LinkedListNode<UnitEffect> e = unitEffects[countdownTrigger].First;
    while(e != null){
      e.Value.Countdown();
      e = e.Next;
    }
  }

  void ExecuteEffects(Trigger trigger, Packet packet = null){
    foreach(TileEffect e in GetTile().tileEffects){
      if(e.trigger == trigger){
        e.Execute(packet);
      }
    }
    foreach(UnitEffect e in unitEffects[trigger]){
      e.Execute(packet);
    }
  }

  void ApplyCommands(Packet packet){
    foreach(Command e in packet.commands){
      e.Apply(packet);
    }
  }
}

