using Godot;
using System;
using System.Collections.Generic;

public static class Movement{
	// TODO after movement is done, countdown effect affectingg movement.
	public static Dictionary<(int, int), float> GetAccessibleTiles(Unit unit, GameMap map){
		Dictionary<(int x, int y), float> accessibleTiles = new Dictionary<(int, int), float>();
		float maxMovement = unit.currentMovement;

		Stack<(int x, int y, float cost)> possibilities = new Stack<(int, int, float)>();
		possibilities.Push((unit.x, unit.y, 0));

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
					float costWithNextTile = cost + getMovementCostForTile(unit, map.tileMap[modedX, modedY]);
					if(costWithNextTile <= maxMovement){
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

}
