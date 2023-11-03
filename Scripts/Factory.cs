using Godot;
using System;

// TODO if we need optimization we can convert switches to operate on enums
public static class Factory
{
  public static Skill GetSkill(string skillName){
    switch(skillName) 
    {
      case "DoubleTap":
        return new DoubleTap();
      case "BitterMedicine":
        return new BitterMedicine();
      default:
        return null;
    }
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
