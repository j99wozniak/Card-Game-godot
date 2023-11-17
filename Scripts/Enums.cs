using Godot;
using System;

public enum Trigger {
  none,
  OnBeginTurn, 
  OnEndTurn,
  OnDeath,
  OnDamage,
  OnHeal,
  OnConsumeStamina,
  OnReplenishStamina,
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
  OnGetSkillCost,
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