using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class Unit : Node
{
	// TODO maybe these triggers should be enums?
	public Dictionary<string, LinkedList<UnitEffect>> unitEffects = new Dictionary<string, LinkedList<UnitEffect>>{
		{"OnBeginTurn", new LinkedList<UnitEffect>()},
		{"OnEndTurn", new LinkedList<UnitEffect>()},
		{"OnDamage", new LinkedList<UnitEffect>()},
		{"OnHeal", new LinkedList<UnitEffect>()},
		{"OnAttacking", new LinkedList<UnitEffect>()},
		{"OnHealing", new LinkedList<UnitEffect>()},
		{"OnStartMove", new LinkedList<UnitEffect>()},
		{"OnEndMove", new LinkedList<UnitEffect>()},
		{"OnMoving", new LinkedList<UnitEffect>()},

		{"OnGetMaxHp", new LinkedList<UnitEffect>()},
		{"OnGetMaxStamina", new LinkedList<UnitEffect>()},
		{"OnGetMaxMovement", new LinkedList<UnitEffect>()},
	};

	public LinkedList<Skill> skills = new();

	public GameMap map;

	public string unitName;

	public int baseMaxHp;
	public int maxHp { get { return StatGetter(baseMaxHp, "OnGetMaxHp"); } } 
	public int currentHp;

	public int baseMaxStamina;
	public int maxStamina { get { return StatGetter(baseMaxStamina, "OnGetMaxStamina"); } }
	public int currentStamina;

	public int baseMaxMovement;
	public int maxMovement{ get { return StatGetter(baseMaxMovement, "OnGetMaxMovement"); } }
	public int currentMovement;

	public int x;
	public int y;

	// TODO create constructor

	// TODO Could probably make this smarter.
	// like setting an ID for each sequence of effects, 
	// and then checking if the sequence is the same
	public int StatGetter(int baseStat, string trigger){
		int fetchedStat = baseStat;
		foreach(TileEffect e in GetTile().tileEffects){
			if(e.trigger == trigger){
				e.Getter(ref fetchedStat);
			}
		}
		foreach(UnitEffect e in unitEffects[trigger]){
			e.Getter(ref fetchedStat);
		}
		return fetchedStat;
	}

	public Tile GetTile(){
		return map.tileMap[x,y];
	}

	// TODO add parentUnit, check for stackability and or uniqueness
	public void AddUnitEffect(UnitEffect effect){
		effect.parentUnit = this;
		if(!unitEffects[effect.trigger].Any()){
			unitEffects[effect.trigger].AddLast(effect);
			return;
		}
		else{
			for(LinkedListNode<UnitEffect> e = unitEffects[effect.trigger].First; e != null; ){
				if(e.Value.priority < effect.priority){
					unitEffects[effect.trigger].AddBefore(e, effect);
					return;
				}
				e = e.Next;
			}
			unitEffects[effect.trigger].AddLast(effect);
		}
	}
	
	public bool HasEffect(string effectName, string effectTrigger = null){
		if(effectTrigger != null){
			foreach(UnitEffect e in unitEffects[effectTrigger]){
				if(e.name == effectName){
					return true;
				}
			}
			return false;
		}
		foreach(KeyValuePair<string, LinkedList<UnitEffect>> list in unitEffects){
			foreach(UnitEffect e in list.Value){
				if(e.name == effectName){
					return true;
				}
			}
		}
		return false;
	}

	// TODO remove UnitEffect by name (and source?)
	public void RemoveUnitEffect(UnitEffect effect){
		unitEffects[effect.trigger].Remove(effect);
	}

	// TODO check for duplicates. Perserve only strongest
	public void AddSkill(Skill skill){
		skill.source = this;
		skills.AddLast(skill);
	}

	public void RemoveSkill(Skill skill){
		skills.Remove(skill);
	}

	// TODO create a function for first selecting skill, and then targetting 
		// Maybe then we no longer will have skillName, but just reference to picked skill?
	// TODO overload for cells, multiple targets
	public void FireSkill(string skillName, Unit target){
		foreach(Skill s in skills){
			if(s.name == skillName){
				s.Fire(target);
				return;
			}
		}
	}

	public void OnBeginTurn(){
		ExecuteEffects("OnBeginTurn");
	}

	public void OnEndTurn(){
		ExecuteEffects("OnEndTurn");
	}

	// Receive a packet
	public void OnDamage(Packet damage){
		ExecuteEffects("OnDamage", damage);
		ApplyCommands(damage);
	}

	public void OnHeal(Packet heal){
		ExecuteEffects("OnHeal", heal);
		ApplyCommands(heal);
	}

	// Fire a packet
	public void OnAttacking(Packet attack){
		ExecuteEffects("OnAttacking", attack);
		GD.Print($"Attacking with: {attack.name}->{attack.value}");
		attack.target.OnDamage(attack);
	}

	public void OnHealing(Packet healing){
		ExecuteEffects("OnHealing", healing);
		GD.Print($"Healing with: {healing.name}->{healing.value}");
		healing.target.OnHeal(healing);
	}

	public void OnStartMove(){

	}

	public void OnMoving(ref float movementCost, Tile tile){
		foreach(TileEffect e in tile.tileEffects){
			if(e.trigger == "OnMovingThrough"){
				e.MovementExecute(ref movementCost, tile, this);
			}
		}
		foreach(TileEffect e in GetTile().tileEffects){
			if(e.trigger == "OnMoving"){
				e.MovementExecute(ref movementCost, tile, this);
			}
		}
		foreach(UnitEffect e in unitEffects["OnMoving"]){
			e.MovementExecute(ref movementCost, tile, this);
		}
	}

	void ExecuteEffects(string trigger, Packet packet = null){
		foreach(TileEffect e in GetTile().tileEffects){
			if(e.trigger == trigger){
				e.Execute(packet);
			}
		}
		foreach(UnitEffect e in unitEffects[trigger]){
			e.Execute(packet);
		}
	}

	void ApplyCommands(Packet packet){
		foreach(Command e in packet.commands){
			e.Apply(packet);
		}
	}


}

