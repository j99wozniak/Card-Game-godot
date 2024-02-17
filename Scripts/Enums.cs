
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
  OnGetUnitCost,
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

public enum UnitSpriteFrames {
  none,
  blueArcher
}

public enum TileTexture{
  none,
  Plains,
  Sands
}

public enum TilePreset{
  none,
  Plains,
  Sands
}