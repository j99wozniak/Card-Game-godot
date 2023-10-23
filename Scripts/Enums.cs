using Godot;
using System;

public enum Trigger {
	none,
	OnBeginTurn, 
	OnEndTurn,
	OnDamage,
	OnHeal,
	OnAttacking,
	OnHealing,
	OnStartMove,
	OnEndMove,
	OnMoving,
	OnGetMaxHp,
	OnGetMaxStamina,
	OnGetMaxMovement,
	OnMovingThrough,
	OnGetSkillPower,
	OnGetSkillRange
}

public enum Type {
	none,
	Physical,
	Chemical,
	Biological,
	Elemental,
	Energy
}

public enum Category {
	none,
	Offensive,
	Defensive,
	Supportive,
	Utility
}