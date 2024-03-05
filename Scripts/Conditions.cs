

using System;
using System.Collections.Generic;
using Godot;

public abstract class Condition
{
  public string name;
  public Player player;
  protected Condition(string name, Player player = null){
    this.name = name;
    this.player = player;
  }
  public abstract bool IsMet(); // Might want to add a parameter like TURN_END or MOVE or SKILL_USE
  public virtual string getExtraData(){return null;}
}

public class EliminateAllEnemiesCondition : Condition
{
  public EliminateAllEnemiesCondition(Player player) : base("EliminateAllEnemiesCondition", player){}
  public override bool IsMet()
  {
    GameMap map = player.game.map;
    foreach(Unit unit in map.unitMap){
      if(unit!=null && unit.player.team != player.team){
        return false;
      }
    }
    return true;
  }
}

public class CharacterStayCondition : Condition
{
  string extraData;
  int tileID;
  public CharacterStayCondition(Player player, string extraData) : base("CharacterStayCondition", player){
      this.extraData = extraData;
      tileID = int.Parse(extraData);
  }
  public override bool IsMet()
  {
    (int x, int y) = player.game.map.IDtoXY(tileID);
    Tile tile = player.game.map.tileMap[x, y];
    Unit unit = tile.GetUnit();
    if(unit!=null && unit.player.team==player.team){
      return true;
    }
    return false;
  }
  public override string getExtraData(){return extraData;}
}

public class DestroyCastleCondition : Condition
{
  string extraData;
  int tileID;
  public DestroyCastleCondition(Player player, string extraData) : base("DestroyCastleCondition", player){
      this.extraData = extraData;
      tileID = int.Parse(extraData);
  }
  public override bool IsMet()
  {
    (int x, int y) = player.game.map.IDtoXY(tileID);
    Tile tile = player.game.map.tileMap[x, y];
    Unit unit = tile.GetUnit();
    if(unit==null || unit.player.team==player.team){
      return true;
    }
    return false;
  }
  public override string getExtraData(){return extraData;}
}

public class AllAlliesEliminatedCondition : Condition
{
  public AllAlliesEliminatedCondition(Player player) : base("AllAlliesEliminatedCondition", player){}
  public override bool IsMet()
  {
    GameMap map = player.game.map;
    foreach(Unit unit in map.unitMap){
      if(unit!=null && unit.player.team == player.team){
        return false;
      }
    }
    return true;
  }
}

public enum ConditionOperator
{
  AND,
  OR,
  //NOT // NOT implemented
}

public class CombinedCondition : Condition
{
  public List<Condition> conditions;
  public ConditionOperator op;

  public CombinedCondition(ConditionOperator op, List<Condition> conditions): base("CombinedCondition"){
    this.op = op;
    this.conditions = conditions;
  }

  public void AddCondition(Condition condition){
    conditions.Add(condition);
  }

  public override bool IsMet(){
    bool result = op == ConditionOperator.AND;

    foreach (Condition condition in conditions){
      bool isMet = condition.IsMet();
      if(condition.player!=null)
        GD.Print("Condition " + condition.name + " for " + condition.player.team + "=" + isMet);
      switch (op)
      {
        case ConditionOperator.AND:
          result = result && isMet;
          break;
        case ConditionOperator.OR:
          result = result || isMet;
          break;
      }
    }

    return result;
  }
  public override string getExtraData(){return ""+op;}
}