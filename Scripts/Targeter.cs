using Godot;
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
    switch (t){
      case Target.Unit:
        if(targetTile.GetUnit()!=null)
          targetedTiles.Add(targetTile);
        break;
      case Target.EnemyUnit:
        if(targetTile.GetUnit()!=null && targetTile.GetUnit().team!=targetingSkill.source.team)
          targetedTiles.Add(targetTile);
        break;
      case Target.AllyUnit:
        if(targetTile.GetUnit()!=null && targetTile.GetUnit().team==targetingSkill.source.team)
          targetedTiles.Add(targetTile);
        break;
      case Target.EmptyTile:
        if(targetTile.GetUnit()==null)
          targetedTiles.Add(targetTile);
        break;
      case Target.Any:
        targetedTiles.Add(targetTile);
        break;
      case Target.Self:
        targetedTiles.Add(targetingSkill.source.GetTile());
        break;
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
}