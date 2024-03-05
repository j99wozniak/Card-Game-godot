using Godot;
using System;
using System.Linq;
using System.Reflection;

// TODO if we need optimization we can convert switches to operate on enums
public static class Factory
{
  public static Condition GetCondition(string conditionName, Player player, string extraData = null){
    System.Type targetType = Assembly.GetExecutingAssembly().GetTypes()
        .FirstOrDefault(t => t.IsSubclassOf(typeof(Condition)) && t.Name == conditionName);
    if (targetType != null){
      if(extraData == null){
        return (Condition)Activator.CreateInstance(targetType, player);
      }
      return (Condition)Activator.CreateInstance(targetType, player, extraData);
    }
    return null;
  }
  public static Skill GetSkill(string skillName){
    // Find the type with the specified class name that is derived from Skill
    System.Type targetType = Assembly.GetExecutingAssembly().GetTypes()
        .FirstOrDefault(t => t.IsSubclassOf(typeof(Skill)) && t.Name == skillName);
    if (targetType != null){
      return (Skill)Activator.CreateInstance(targetType);
    }
    return null;
  }
  public static UnitEffect GetUnitEffect(string unitEffectName){
    System.Type targetType = Assembly.GetExecutingAssembly().GetTypes()
        .FirstOrDefault(t => t.IsSubclassOf(typeof(UnitEffect)) && t.Name == unitEffectName);
    if (targetType != null){
      return (UnitEffect)Activator.CreateInstance(targetType);
    }
    return null;
  }
  public static TileEffect GetTileEffect(string tileEffectName){
    System.Type targetType = Assembly.GetExecutingAssembly().GetTypes()
        .FirstOrDefault(t => t.IsSubclassOf(typeof(TileEffect)) && t.Name == tileEffectName);
    if (targetType != null){
      return (TileEffect)Activator.CreateInstance(targetType);
    }
    return null;
  }
  public static Tile GetPresetTile(TilePreset tilePreset, int x, int y, GameMap map){
    return GetPresetTile(tilePreset, map.TileID(x,y), map);
  }
  public static Tile GetPresetTile(TilePreset tilePreset, int ID, GameMap map){
    int x = ID / map.maxSize;
    int y = ID % map.maxSize;
    switch(tilePreset) 
    {
      case TilePreset.Plains:
        return new Tile(map, "plains", 1, x, y, TileTexture.Plains){tilePreset = tilePreset};
      case TilePreset.Sands:
        return new Tile(map, "sands", 1, x, y, TileTexture.Sands){tilePreset = tilePreset};
      default:
        return null;
    }
  }
  
  static SpriteFrames blueArcherSprites;
  public static SpriteFrames GetUnitSpriteFrames(UnitSpriteFrames spriteFrameEnum){
      switch(spriteFrameEnum)
      {
          case UnitSpriteFrames.blueArcher:
              return blueArcherSprites ??= (SpriteFrames)GD.Load("res://Sprites/Units/Frames/ArcherFrames.tres");
          default:
              return null;
      }
  }

  static Texture2D plainsTexture;
  static Texture2D sandsTexture;
  public static Texture2D GetTileTexture(TileTexture textureEnum){
      switch(textureEnum)
      {
          case TileTexture.Plains:
              return plainsTexture ??= (Texture2D)GD.Load("res://Sprites/Tiles/plains.png");
          case TileTexture.Sands:
              return sandsTexture ??= (Texture2D)GD.Load("res://Sprites/Tiles/sands.png");
          default:
              return null;
      }
  }
}
