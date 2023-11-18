using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Tile : Node
{
  public int ID;
  public LinkedList<TileEffect> tileEffects = new();
  public GameMap map;
  public Node2D parentNode;
  public string tileName;
  public float cost;
  public int x;
  public int y;
  public Sprite2D tileSprite;
  public static Texture2D selectTexture = (Texture2D)GD.Load("res://Sprites/Tiles/select.png");
  static Sprite2D selectSprite = new Sprite2D{Texture = selectTexture, Name = "selectNode", ZIndex = 1}; // FOR EXCLUSIVE SELECTION
  
  
  static public Node2D createTileNode(Tile tile, Texture2D texture){
    Node2D tileNode = new Node2D();
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

  public Tile(GameMap map, string tileName, float cost, int x, int y){
    this.ID = map.sizeX*x+y;
    this.map = map;
    this.tileName = tileName;
    this.cost = cost;
    this.x = x;
    this.y = y;
    map.tileMap[x,y] = this;
  }

  public Vector2 GetRealPosition(){
    return new Vector2(x*Game.TileSize, y*Game.TileSize);
  }

  public Unit GetUnit(){
    return map.unitMap[x,y];
  }

  public void HighlightTile(){
    tileSprite.SelfModulate = new Color(0.5f, 1f, 1f);
  }

  public void RemoveHighlight(){
    tileSprite.SelfModulate = new Color(1f, 1f, 1f);
  }

  public void SelectTile(){
    //Sprite2D selectSprite = new Sprite2D{Texture = selectTexture, Name = "selectNode", ZIndex = 1};
    if(selectSprite.GetParent() != null){
      selectSprite.GetParent().RemoveChild(selectSprite); // (maybe add if GetParent not null)
    }
    parentNode.AddChild(selectSprite);
  }

  public void RemoveSelection(){
    //parentNode.GetNode<Sprite2D>("selectNode").QueueFree();
    parentNode.RemoveChild(parentNode.GetNode<Sprite2D>("selectNode"));
  }

  public void AddTileEffect(TileEffect effect){
    // TODO add Unit source (that can be null) and implement related logic
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
