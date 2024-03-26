

using System.Collections.Generic;
using System.Linq;
using Godot;

public class Player
{
  private static string[] teamColors = new string[] {"Blue", "Red"};
  public Game game;
  public int team;
  public bool isCPU;

  public Condition winCondition;
  public Condition loseCondition;

  public List<Unit> deck;

  public Player(Game game, int team, bool isCPU){
    this.game = game;
    this.team = team;
    this.isCPU = isCPU;
  }

  public string GetTeamColor(){
    return teamColors[team-1];
  }

  // Basic aggressive AI
  public void AIExecute(){
    foreach(Unit unit in game.map.unitMap){
      if(unit!=null && unit.player.team == team){
        
        // Move (or not) and use skill
        bool thereIsEnemyInMyRange = false;
        bool foundTarget = false;
        do{
          foundTarget = false;
          Tile tileToMoveTo = null;
          Tile tileToAttack = null;
          Skill skillToUse = null;

          // 1. Go through all movable-to Tiles (including current one)
          Dictionary<(int, int), float> movableRange = Range.GetAccessibleMovementTiles(unit, game.map);
          movableRange[(unit.x, unit.y)] = 0;
          movableRange = movableRange.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
          foreach((int x, int y) movTile in movableRange.Keys){
            Tile movableTile = game.map.tileMap[movTile.x, movTile.y];
            if(movableTile.GetUnit()!=null && movableTile.GetUnit()!=unit){
              // Tile is occupied.
              continue;
            }
            // 2. If any of offensive skills is in range of enemy from Tile, go to that tile (or stay)
            foreach(Skill skill in unit.allSkills){
              if(skill.category == Category.Offensive && skill.currentCost <= unit.currentStamina){
                Dictionary<(int, int), float> skillRange = Range.GetAccessibleSkillTilesFromTile(skill, movableTile, game.map);
                foreach ((int x, int y) targetableTile in skillRange.Keys) {
                  Tile targetTile = game.map.tileMap[targetableTile.x, targetableTile.y];
                  Unit target = targetTile.GetUnit();
                  if(target!=null && target.player.team != team 
                      && Targeter.IsTileViableTarget(skill, targetTile)){
                    thereIsEnemyInMyRange = true;
                    foundTarget = true;
                    tileToMoveTo = movableTile;
                    tileToAttack = target.GetTile();
                    skillToUse = skill;
                    break;
                  }
                }
                if(foundTarget){
                  break;
                }
              }
            }
            if(foundTarget){
              break;
            }
          }
          if(foundTarget){
            if(tileToMoveTo.x != unit.x || tileToMoveTo.y != unit.y){
              float tileMoveCost = movableRange[(tileToMoveTo.x, tileToMoveTo.y)];
              unit.MoveUnit(tileToMoveTo, tileMoveCost);
            }
            // 3. And use skill as much as possible
            while(skillToUse.currentCost <= unit.currentStamina && Targeter.IsTileViableTarget(skillToUse, tileToAttack)){
              List<Tile> targetList = new();
              for(int i = 0; i<skillToUse.numberOfTargets; i++){
                targetList.Add(tileToAttack);
              }
              skillToUse.UseSkill(targetList);
            }
            if(tileToAttack.GetUnit() == null || tileToAttack.GetUnit().isDead){
              thereIsEnemyInMyRange = false;
            }
          }
            // Then repeat step 1
          GD.Print($"repeat {foundTarget}");
        }while(foundTarget==true);

        if(!thereIsEnemyInMyRange){
          // Move as close as you can
          // Select target enemy - Go through all enemies, and select closest one in straight (manhattan)
          Unit futureTarget = null;
          int approxDist = int.MaxValue;
          foreach(Unit u in game.map.unitMap){
            if(u!=null && u.player.team != team){
              int dis = Range.Manhattan(unit.x, unit.y, u.x, u.y);
              if(approxDist > dis){
                approxDist = dis;
                futureTarget = u;
              }
            }
          }
            // If there are no enemies, just `return`
          if(futureTarget==null){
            return;
          }
          else{
            GD.Print($"My future target is {futureTarget}");
          }
          // Find fastest route to them, and move to the last tile that is within movement range
          List<Tile> path = Range.AStarGetShortestPath(game.map, unit, unit.GetTile(), futureTarget.GetTile());
          path.Reverse();
          Dictionary<(int, int), float> movableRange = Range.GetAccessibleMovementTiles(unit, game.map);
          foreach(Tile t in path){
            if(movableRange.ContainsKey((t.x, t.y)) && t.GetUnit()==null){
              unit.MoveUnit(t, movableRange[(t.x,t.y)]);
              break;
            }
          }
        }
      }
      
    }
  }
}