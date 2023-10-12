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
		// TODO add Unit source (that can be null)
		TileEffect existingEffect = GetTileEffectByName(effect.name, effect.trigger);
		if(existingEffect == null){
			effect.parentTile = this;
			if(!tileEffects.Any()){
				tileEffects.AddLast(effect);
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
			return;
		}
		if(existingEffect.stackable){
			existingEffect.power += effect.power;
			effect.power = existingEffect.power;
		}
		if(existingEffect.count < effect.count){
			LinkedListNode<TileEffect> e = tileEffects.First;
			while(e != null){
				if(e.Value == existingEffect){
					e.Value = effect;
					break;
				}
				e = e.Next;
			}
		}
	}

	public TileEffect GetTileEffectByName(string effectName, Trigger effectTrigger = Trigger.none){
		if(effectTrigger != Trigger.none){
			foreach(TileEffect e in tileEffects){
				if(e.name == effectName && e.trigger == effectTrigger){
					return e;
				}
			}
			return null;
		}
		foreach(TileEffect e in tileEffects){
				if(e.name == effectName){
					return e;
				}
			}
		return null;
	}

	public void RemoveTileEffect(TileEffect effect){
		tileEffects.Remove(effect);
	}

	public void CountdownTileEffects(Trigger countdownTrigger){
		LinkedListNode<TileEffect> e = tileEffects.First;
		while(e != null){
			if(e.Value.countdownTrigger == countdownTrigger){
				e.Value.Countdown();
			}
			e = e.Next;
		}
	}
}
