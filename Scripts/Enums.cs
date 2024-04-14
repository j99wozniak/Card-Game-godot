public enum Trigger {
  none, //0
  OnBeginTurn, //1 
  OnEndTurn, //2
  OnDeath, //3
  OnDamage, //4
  OnHeal, //5
  OnConsumeStamina, //6
  OnReplenishStamina, //7
  OnAttacking, //8
  OnHealing, //9
  OnStartMove, //10
  OnEndMove, //11
  OnMoving, //12 // Applied when there are effects on unit / tile we start on, that influence cost for CONSIDERED tile (goes last)
  OnGetMaxHp, //13
  OnGetMaxStamina, //14
  OnGetMaxMovement, //15
  OnGetUnitCost, //16
  OnMovingThrough, //17 // Applied when calculating current cost for THIS tile (goes first)
  OnGetSkillPower, //18
  OnGetSkillCost, //19
  OnGetSkillRange, //20
  OnGetSkillList //21
}

public enum Type {
  none, //0
  Physical, //1
  Chemical, //2
  Biological, //3
  Elemental, //4
  Energy //5
}

public enum Category {
  none, //0
  Offensive, //1
  Defensive, //2
  Supportive, //3
  Utility //4
}

public enum Target {
  none, //0
  Unit, //1
  EnemyUnit, //2
  AllyUnit, //3
  EmptyTile, //4
  Any, //5
  Self //6
}

public enum UnitSpriteFrames {
  none, //0
  blueArcher, //1
  redArcher //2
}

public enum TileTexture{
  none, //0
  Plains, //1
  Sands, //2
  Rocky //3
}

public enum TilePreset{
  none, //0
  Plains, //1
  Sands, //2
  Rocky //3
}

public enum Directions{
  none,
  UP,
  DOWN,
  RIGHT,
  LEFT
}
