using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Tile : Node
{
  public int ID;
  public TilePreset tilePreset = TilePreset.none;
  public LinkedList<TileEffect> tileEffects = new();
  public GameMap map;
  public Node2D parentNode;
  public string tileName;
  public float cost;
  public int x;
  public int y;
  public Sprite2D tileSprite;
  public static Texture2D selectTexture = (Texture2D)GD.Load("res://Sprites/Tiles/select.png");
  public TileTexture tileTexture;
  static Sprite2D selectSprite;
  static Color highlightColor = new Color(0.5f, 1f, 1f);
  
  static public Node2D createTileNode(Tile tile){
    return createTileNode(tile, Factory.GetTileTexture(tile.tileTexture));
  }
  
  static public Node2D createTileNode(Tile tile, Texture2D texture){
    Node2D tileNode = new Node2D();
    tileNode.Name = $"{tile.x}x{tile.y}y#{tile.ID}";
    tileNode.AddChild(tile);
    Sprite2D sprite = new Sprite2D(); // Create a new Sprite2D.
    sprite.Texture = texture;
    sprite.Name = "spriteNode";
    tile.tileSprite = sprite;
    tileNode.AddChild(sprite);
    tileNode.Position = tile.GetRealPosition();
    tile.parentNode = tileNode;
    return tileNode;
  }

  public Tile(GameMap map, string tileName, float cost, int x, int y, TileTexture tileTexture = TileTexture.none){
    this.ID = map.maxSize*x+y;
    this.map = map;
    this.tileName = tileName;
    this.cost = cost;
    this.x = x;
    this.y = y;
    this.tileTexture = tileTexture;
    map.tileMap[x,y] = this;
  }

  public Vector2 GetRealPosition(){
    return new Vector2(x*Game.tileSize, y*Game.tileSize);
  }

  public Unit GetUnit(){
    return map.unitMap[x,y];
  }

  static public void SetHighlightColor(string color){
    highlightColor = new Color(color);
  }
  public void HighlightTile(){
    tileSprite.SelfModulate = highlightColor;
  }

  public void RemoveHighlight(){
    tileSprite.SelfModulate = new Color(1f, 1f, 1f);
  }

  public void SelectTile(){
    if(!IsInstanceValid(selectSprite)){
      selectSprite = new Sprite2D{Texture = selectTexture, Name = "selectNode", ZIndex = 1};
    }
    selectSprite.GetParent()?.RemoveChild(selectSprite); // (maybe add if GetParent not null)
    parentNode.AddChild(selectSprite);
  }

  public void RemoveSelection(){
    //parentNode.GetNode<Sprite2D>("selectNode").QueueFree();
    parentNode.RemoveChild(parentNode.GetNode<Sprite2D>("selectNode"));
  }

  public void AddTileEffect(TileEffect effect){
    // TODO add Unit source (that can be null) and implement related logic
    TileEffect existingEffect = GetTileEffectByNameAndSource(effect.name, effect.source, effect.trigger);
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

  public TileEffect GetTileEffectByNameAndSource(string effectName, Unit source, Trigger effectTrigger = Trigger.none){
    if(effectTrigger != Trigger.none){
      foreach(TileEffect e in tileEffects){
      if(e.name == effectName && e.source == source && e.trigger == effectTrigger){
        return e;
      }
    }
      return null;
    }
    foreach(TileEffect e in tileEffects){
      if(e.name == effectName && e.source == source){
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
