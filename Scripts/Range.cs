using Godot;
using System;
using System.Collections.Generic;

public static class Range{
  // TODO after movement is done, countdown effect affecting movement.
  public static Dictionary<(int, int), float> GetAccessibleTiles(Unit unit, GameMap map){
    float maxRange = unit.currentMovement;
    return CalculateAccessibleTiles(unit, map, unit.x, unit.y, maxRange, getMovementCostForTile);
  }
  public static Dictionary<(int, int), float> GetAccessibleTiles(Skill skill, GameMap map){
    float maxRange = skill.currentRange;
    return CalculateAccessibleTiles(skill.source, map, skill.source.x, skill.source.y, maxRange, getSkillRangeCostForTile);
  }
  public static Dictionary<(int, int), float> GetRadius(GameMap map, int x, int y, int r){
    return CalculateAccessibleTiles(null, map, x, y, r, getCostOne);
  }
  // TODO can be overloaded for radious.

  private static Dictionary<(int, int), float> CalculateAccessibleTiles(Unit unit, GameMap map, int startX, int startY, float maxRange, Func<Unit, Tile, float> getCostFunc){ 
    Dictionary<(int x, int y), float> accessibleTiles = new Dictionary<(int, int), float>();

    Stack<(int x, int y, float cost)> possibilities = new Stack<(int, int, float)>();
    possibilities.Push((startX, startY, 0));

    List<(int modX, int modY)> directions = new List<(int, int)>{(-1, 0), (1, 0), (0, -1), (0, 1)};

    while(possibilities.Count != 0){
      (int x, int y, float cost) = possibilities.Pop();
      foreach( (int x, int y) mod in directions){
        int modedX = x+mod.x;
        int modedY = y+mod.y;
        //GD.Print("Checking out " + modedX + " " + modedY);
        // Check if within map borders
        if(modedX >= 0 && modedX < map.sizeX && modedY >= 0 && modedY < map.sizeY){
          //GD.Print("got in borders");
          // Check cost for this tile
          float costWithNextTile = cost + getCostFunc(unit, map.tileMap[modedX, modedY]);
          if(costWithNextTile <= maxRange){
            //GD.Print("cost good");
            // Check if in Dictionary already
            // Check if cost in dictionary is lower than cost now
            // If any of these has an answer "NO" then add/change the record in dictionary, and push it to the stack
            //GD.Print("cost from dict = " + accessibleTiles.GetValueOrDefault((modedX, modedY), 0) );
            if(!accessibleTiles.ContainsKey((modedX, modedY)) || accessibleTiles.GetValueOrDefault((modedX, modedY), 0) > costWithNextTile){
              //GD.Print("pushed");
              accessibleTiles[(modedX, modedY)] = costWithNextTile;
              possibilities.Push((modedX, modedY, costWithNextTile));
            }
          }
        }
      }
    }
    return accessibleTiles;
  }

  // Move this to Unit?
  static float getMovementCostForTile(Unit unit, Tile tile){
    float movementCost = tile.cost;
    unit.OnMoving(ref movementCost, tile);
    return movementCost;
  }

  // TODO Can be expanded if needed.
  static float getSkillRangeCostForTile(Unit unit, Tile tile){
    return 1;
  }

  static float getCostOne(Unit unit=null, Tile tile=null){
    return 1;
  }

}
