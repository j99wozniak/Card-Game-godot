using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Tile : Node
{
	public string tileName;
	public GameMap map;
	public int x;
	public int y;
	public float cost;
	public LinkedList<TileEffect> tileEffects = new();
	public Sprite2D tileSprite;
	
	// TODO create constructor

	public Vector2 GetRealPosition(){
		return new Vector2(x*32, y*32);
	}

	public Unit GetUnit(){
		return map.unitMap[x,y];
	}

	public void AddTileEffect(TileEffect effect){
		// TODO that's similar logic to Unit add... Maybe unify somehow? 
		//      But then would either have to change dictionary to just linked list, or this linked list to dictionary
		effect.parentTile = this;
		if(!tileEffects.Any()){
			tileEffects.AddLast(effect);
			return;
		}
		else{
			for(LinkedListNode<TileEffect> eIterator = tileEffects.First; eIterator != null; ){
				if(eIterator.Value.priority < effect.priority){
					tileEffects.AddBefore(eIterator, effect);
					return;
				}
				eIterator = eIterator.Next;
			}
			tileEffects.AddLast(effect);
		}
	}
}
