using Godot;
using static Godot.CanvasItem;
using System.Collections.Generic;
using System.Linq;

public partial class Unit : Node
{
  public static int currentUnitID = 1;
  public int ID = 0;
  // TODO maybe these triggers should be enums?
  public Dictionary<Trigger, LinkedList<UnitEffect>> unitEffects = new Dictionary<Trigger, LinkedList<UnitEffect>>{
  {Trigger.OnBeginTurn, new LinkedList<UnitEffect>()},
  {Trigger.OnEndTurn, new LinkedList<UnitEffect>()},
  {Trigger.OnDeath, new LinkedList <UnitEffect>()},
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
  {Trigger.OnGetUnitCost, new LinkedList<UnitEffect>()},

  {Trigger.OnGetSkillPower, new LinkedList<UnitEffect>()},
  {Trigger.OnGetSkillCost, new LinkedList<UnitEffect>()},
  {Trigger.OnGetSkillRange, new LinkedList<UnitEffect>()},
  {Trigger.OnGetSkillList, new LinkedList<UnitEffect>()},
  };


  public LinkedList<Skill> skills = new();
  public LinkedList<Skill> allSkills { get { return OnGetSkillList(new LinkedList<Skill>(skills)); } }

  public GameMap map;
  public Node2D parentNode;

  public string unitName;

  public int team;

  public int baseMaxHp;
  public int maxHp { get { return StatGetter(baseMaxHp, Trigger.OnGetMaxHp); } } 
  private int _currentHp;
  public int currentHp { get { return _currentHp; } set { _currentHp = value; UpdateHpLabel(); if(value==0 && !isDead){ OnDeath(); } } }

  public int baseMaxStamina;
  public int maxStamina { get { return StatGetter(baseMaxStamina, Trigger.OnGetMaxStamina); } }
  public int currentStamina;

  public int baseMaxMovement;
  public int maxMovement{ get { return StatGetter(baseMaxMovement, Trigger.OnGetMaxMovement); } }
  public int currentMovement;
   public int baseUnitCost;
  public int unitCost{ get { return StatGetter(baseUnitCost, Trigger.OnGetUnitCost); } }
  public bool isDead;

  public int x;
  public int y;
  public UnitSpriteFrames unitSpriteFrames;
  public AnimatedSprite2D sprite;
  public Label currentHpLabel;

  public Unit SetNewID(){
    this.ID = currentUnitID++;
    return this;
  }

  public void AddUnitToMap(GameMap map){
    this.map = map;
    if(!isDead){
      map.unitMap[x,y] = this;
    }
    else{
      map.graveyard.Add(this);
      return;
    }
    map.AddChild(CreateUnitNodeWithFactoryFrames(this));
  }

  static public Node2D CreateUnitNodeWithFactoryFrames(Unit unit){
    return CreateUnitNode(unit, Factory.GetUnitSpriteFrames(unit.unitSpriteFrames));
  }

  static public Node2D CreateUnitNode(Unit unit, SpriteFrames spriteFrames){
    Node2D unitNode = new Node2D();
    unitNode.Name = unit.unitName + "_node";
    unitNode.Position = unit.GetRealPosition();
    unit.parentNode = unitNode;

    AnimatedSprite2D unitSpriteNode = new AnimatedSprite2D();
    unitSpriteNode.SpriteFrames = spriteFrames;
    unitSpriteNode.Autoplay = "right_idle";
    unitSpriteNode.Name = "animatedSpriteNode";
    unitSpriteNode.TextureFilter = TextureFilterEnum.Nearest;
    unit.sprite = unitSpriteNode;
    unitNode.AddChild(unitSpriteNode);

    Label currentHpLabel = new Label();
    currentHpLabel.ZIndex = 2;
    currentHpLabel.Scale = new Vector2(0.6f, 0.6f);
    currentHpLabel.AddThemeColorOverride("font_outline_color", new Color(0,0,0,1));
    currentHpLabel.AddThemeConstantOverride("outline_size", 15);
    unit.currentHpLabel = currentHpLabel;
    unitNode.AddChild(currentHpLabel);
    unit.UpdateHpLabel();

    return unitNode;
  }

  // TODO add function that updates all GUI visible variables
  public void UpdateHpLabel(){
    if(currentHpLabel!=null){
      currentHpLabel.Text = $"{currentHp}/{maxHp}❤️";
    }
  }

  public Vector2 GetRealPosition(){
    return new Vector2(x*Game.tileSize, y*Game.tileSize);
  }

  public Unit(string unitName, int team, int baseMaxHp, int baseMaxStamina, int baseMaxMovement, 
              int baseUnitCost, int x, int y, UnitSpriteFrames unitSpriteFrames, bool isDead = false){
    this.unitName = unitName;
    this.team = team;
    this.baseMaxHp = baseMaxHp;
    currentHp = baseMaxHp;
    this.baseMaxStamina = baseMaxStamina;
    currentStamina = baseMaxStamina;
    this.baseMaxMovement = baseMaxMovement;
    currentMovement = baseMaxMovement;
    this.baseUnitCost = baseUnitCost;
    this.x = x;
    this.y = y;
    this.unitSpriteFrames = unitSpriteFrames;
    this.isDead = isDead;
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

  public void MoveUnit(Tile targetTile){
    OnStartMove();
    map.unitMap[x, y] = null;
    x = targetTile.x;
    y = targetTile.y;
    map.unitMap[x, y] = this;
    parentNode.Position = GetRealPosition();
    OnEndMove();
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
          UpdateHpLabel();
          return;
        }
        e = e.Next;
      }
      unitEffects[effect.trigger].AddLast(effect);
      }
    }
    else if(existingEffect.stackable){
      existingEffect.power += effect.power;
      effect.power = existingEffect.power;
    }
    else if(existingEffect.count < effect.count || existingEffect.power < effect.power){
      RemoveUnitEffect(existingEffect);
      AddUnitEffect(effect);
    }
    UpdateHpLabel();
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
    foreach(Skill s in allSkills){
      if(s.name == skillName){
      return s;
      }
    }
    return null;
  }

  public void UseSkill(string skillName, List<Tile> targetList){
    foreach(Skill s in allSkills){
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

  public void OnDeath(){
    ExecuteEffects(Trigger.OnDeath);
    CountdownUnitEffects(Trigger.OnDeath);
    if(currentHp == 0){
      map.unitMap[x, y] = null;
      isDead = true;
      map.graveyard.Add(this);
      foreach(KeyValuePair<Trigger, LinkedList<UnitEffect>> list in unitEffects){
        LinkedListNode<UnitEffect> e = list.Value.First;
        while(e != null){
          if(e.Value.removedOnDeath){
            e.Value.RemoveThisEffect();
          }
          e = e.Next;
        }
      }
      PlayAnimation("right_death");
    }
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
    GD.Print($"{this.unitName} Geting healed with: {heal.name}-{heal.value}");
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
/*
  public void PlayAnimation(string anim){
    GD.Print("actioning");
    sprite.Animation = anim;
    if(!sprite.IsConnected("animation_finished", Callable.From(new Action(GoBackIdle)))){
      sprite.AnimationFinished += GoBackIdle;
    }
    sprite.Play();
  }
*/
  Queue<string> animationQueue = new Queue<string>();
  bool queueEmpty = true;
  public void PlayAnimation(string anim){
    if(IsInstanceValid(parentNode)){
      if(queueEmpty){
        queueEmpty = false;
        sprite.Animation = anim;
        sprite.Play();
        sprite.AnimationFinished += FinishAnimation;
      }
      else{
        if(currentHp != 0 || anim.Contains("death")){
          animationQueue.Enqueue(anim);
        }
      }
    }
    else{
      GD.Print($"Tried to play animation {anim} but {unitName} doesn't have physical node2d");
    }
  }

  public void FinishAnimation(){
    if(IsInstanceValid(parentNode)){
      if(animationQueue.Count == 0){
        if(sprite.Animation.ToString().Contains("death")){
          parentNode.QueueFree();
        }
        else{
          sprite.Animation = "right_idle";
          sprite.Play();
          sprite.AnimationFinished -= FinishAnimation;
          queueEmpty = true;
        }
      }
      else{
        sprite.Animation = animationQueue.Dequeue();
        sprite.Play();
      }
    }
    else{
      GD.Print($"Tried to finish animation but {unitName} already doesn't have physical node2d");
    }
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
    ExecuteEffects(Trigger.OnStartMove);
    CountdownUnitEffects(Trigger.OnStartMove);
  }

  public void OnEndMove(){
    ExecuteEffects(Trigger.OnEndMove);
    CountdownUnitEffects(Trigger.OnEndMove);
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

  public LinkedList<Skill> OnGetSkillList(LinkedList<Skill> skillList){
    foreach(UnitEffect e in unitEffects[Trigger.OnGetSkillList]){
      e.ModifySkillList(skillList);
    }
    return skillList;
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

