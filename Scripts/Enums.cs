
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
  OnMoving, // Applied when there are effects on unit / tile we start on, that influence cost for CONSIDERED tile (goes last)
  OnGetMaxHp,
  OnGetMaxStamina,
  OnGetMaxMovement,
  OnGetUnitCost,
  OnMovingThrough, // Applied when calculating current cost for THIS tile (goes first)
  OnGetSkillPower,
  OnGetSkillCost,
  OnGetSkillRange,
  OnGetSkillList
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

public enum Target {
  none,
  Unit,
  EnemyUnit,
  AllyUnit,
  EmptyTile,
  Any,
  Self
}

public enum UnitSpriteFrames {
  none,
  blueArcher,
  redArcher
}

public enum TileTexture{
  none,
  Plains,
  Sands,
  Rocky
}

public enum TilePreset{
  none,
  Plains,
  Sands,
  Rocky
}

public enum Directions{
  none,
  UP,
  DOWN,
  RIGHT,
  LEFT
}
