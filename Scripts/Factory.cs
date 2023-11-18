using Godot;
using System;
using System.Linq;
using System.Reflection;

// TODO if we need optimization we can convert switches to operate on enums
public static class Factory
{
  // TODO might want to copy this solution to the rest of factories
  public static Skill GetSkill(string skillName){
    // Find the type with the specified class name that is derived from Skill
    System.Type targetType = Assembly.GetExecutingAssembly().GetTypes()
        .FirstOrDefault(t => t.IsSubclassOf(typeof(Skill)) && t.Name == skillName);
    if (targetType != null){
      return (Skill)Activator.CreateInstance(targetType);
    }
    return null;
  }
  public static UnitEffect GetUnitEffect(string unitEffectName){
    switch(unitEffectName) 
    {
      case "Poison":
        return new Poison();
      case "Dodge":
        return new Dodge();
      case "Armor":
        return new Armor();
      case "Counter":
        return new Counter();
      case "Skip":
        return new Skip();
      case "Eager":
        return new Eager();
      case "PreciseShots":
        return new PreciseShots();
      case "Sniper":
        return new Sniper();
      default:
        return null;
    }
  }
  public static TileEffect GetTileEffect(string tileEffectName){
    switch(tileEffectName) 
    {
      case "RockyTerrain":
        return new RockyTerrain();
      case "Flame":
        return new Flame();
      case "Glue":
        return new Glue();
      default:
        return null;
    }
  }
}
