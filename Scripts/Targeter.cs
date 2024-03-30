using System.Collections.Generic;

public class Targeter
{
  
  public const bool AllTargetsSelected = true;
  public const bool WaitingForTargets = false;
  
  public const bool RemovedLast = true;
  public const bool AlreadyEmpty = false;
  public Skill targetingSkill = null;
  public List<Tile> targetedTiles = null;
  public Targeter(Skill targetingSkill){
    this.targetingSkill = targetingSkill;
     targetedTiles = new List<Tile>();
  }

  // TODO After moving mouse (changing hovered tile) 
  // get list of tiles that would be affected.
  // If there is more than one numbers of targets
  // get also splash zones from previous targets.
  // If the target qualifier is Self, then it should always be-
  // only self (+splash zone)
  public List<Tile> GetAffectedTiles(Tile hoveredTargetTile){
    return null;
  }
  // TODO add check for canTargetSameTileAgain
  public bool AddTileToTargeting(Tile targetTile){
    Target t = targetingSkill.targetQualifier;
    if(t == Target.Self){
      targetedTiles.Add(targetingSkill.source.GetTile());
    }
    else if(IsTileViableTarget(targetingSkill, targetTile)){
      targetedTiles.Add(targetTile);
    }
    if(targetingSkill.numberOfTargets == targetedTiles.Count)
      return AllTargetsSelected;
    return WaitingForTargets;
  }
  public bool RemoveLastTileFromTargets(){
    if(targetedTiles.Count == 0)
      return AlreadyEmpty;
    targetedTiles.RemoveAt(targetedTiles.Count-1);
    return RemovedLast;
  }

  public static bool IsTileViableTarget(Skill targetingSkill, Tile targetTile){
    switch (targetingSkill.targetQualifier){
      case Target.Unit:
        if(targetTile.GetUnit()!=null)
         return true;
        break;
      case Target.EnemyUnit:
        if(targetTile.GetUnit()!=null && targetTile.GetUnit().player.team!=targetingSkill.source.player.team)
          return true;
        break;
      case Target.AllyUnit:
        if(targetTile.GetUnit()!=null && targetTile.GetUnit().player.team==targetingSkill.source.player.team)
          return true;
        break;
      case Target.EmptyTile:
        if(targetTile.GetUnit()==null)
          return true;
        break;
      case Target.Any:
        return true;
      case Target.Self:
        if(targetingSkill.source != null && !targetingSkill.source.isDead)
          return true;
        break;
    }
    return false;
  }
}