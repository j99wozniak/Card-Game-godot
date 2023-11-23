using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Controller : Node
{
  private Camera2D camera;
  private Viewport viewport;
  public Game parentGame;
  
  private PopupMenu popupMenu;
  private Tile selectedTile;
  private Skill selectedSkill;
  private State currentState = State.SELECTING_UNIT;
  Dictionary<(int x, int y), float> rangeDict;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    viewport = GetViewport();
    camera = viewport.GetCamera2D();
    popupMenu = new PopupMenu();
    GetTree().Root.GetNode<Node2D>("Node2D").AddChild(popupMenu);
    Action<int> callback = new Action<int>(SelectSkill);
    popupMenu.Connect("id_pressed", Callable.From(callback));
    camera.Zoom = new Vector2(2.2f, 2.2f);
    camera.Position = new Vector2(220f, 120f);
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
  }

  public override void _Input(InputEvent @event)
  {
    if(@event is InputEventKey eventKey){
      GD.Print("Keys");
      if (eventKey.Pressed && eventKey.Keycode == Key.W){
        camera.Position = new Vector2(camera.Position.X, camera.Position.Y-20);
      }
      if (eventKey.Pressed && eventKey.Keycode == Key.A){
        camera.Position = new Vector2(camera.Position.X-20, camera.Position.Y);
      }
      if (eventKey.Pressed && eventKey.Keycode == Key.S){
        camera.Position = new Vector2(camera.Position.X, camera.Position.Y+20);
      }
      if (eventKey.Pressed && eventKey.Keycode == Key.D){
        camera.Position = new Vector2(camera.Position.X+20, camera.Position.Y);
      }
      if (eventKey.Pressed && eventKey.Keycode == Key.Left){
        parentGame.LoadGame();
      }
    }
    else if (@event is InputEventMouseMotion eventMouseMotion && IsInstanceValid(parentGame.map)){
      Vector2 currentPosition = GetTree().Root.GetNode<Node2D>("Node2D").GetGlobalMousePosition();
      int xTile = (int)Math.Floor((currentPosition.X + Game.tileSize/2) / Game.tileSize);
      int yTile = (int)Math.Floor((currentPosition.Y + Game.tileSize/2) / Game.tileSize);
      if(currentState == State.SELECTING_UNIT){
        if(xTile>=0 && yTile>=0 && xTile < parentGame.map.sizeX && yTile < parentGame.map.sizeY){
          selectedTile = parentGame.map.tileMap[xTile, yTile];
          selectedTile.SelectTile();
        }
      }
      else if(currentState == State.TARGET_SKILL){
        if(rangeDict.ContainsKey((xTile, yTile))){
          selectedTile = parentGame.map.tileMap[xTile, yTile];
          selectedTile.SelectTile();
        }
      }
    }
    else if (@event is InputEventMouseButton inputEventMouseButton){
      if(inputEventMouseButton.IsPressed() && inputEventMouseButton.ButtonIndex == MouseButton.Right){
        GD.Print("Right Click");
        if(currentState == State.SELECTING_UNIT){
          popupMenu.Clear();
          Unit selectedUnit = selectedTile.GetUnit();
          if(selectedUnit != null){
            int id = 0;
            foreach(Skill skill in selectedUnit.skills){
              popupMenu.AddItem($"{skill.name} | {skill.currentPower}🗡️ | {skill.currentCost}⚡", id);
              popupMenu.SetItemTooltip(id, $"{skill.currentRange}🏹");
              if(selectedUnit.currentStamina < skill.currentCost){
                popupMenu.SetItemDisabled(id, true);
              }
              id++;
            }

            popupMenu.Popup(new Rect2I((int)inputEventMouseButton.Position.X, (int)inputEventMouseButton.Position.Y, 10, 10));
          }
        }
        else if(currentState == State.TARGET_SKILL){
          RemoveHighlights();
          rangeDict = null;
          selectedSkill = null;
          currentState = State.SELECTING_UNIT;
        }
      }
      else if (inputEventMouseButton.IsPressed() && inputEventMouseButton.ButtonIndex == MouseButton.Left){
        GD.Print("Left Click");
        if(currentState == State.TARGET_SKILL){
          // Different targeting modes
          if(selectedTile.GetUnit()!=null){
            selectedSkill.UseSkill(new List<Tile> {selectedTile});
            RemoveHighlights();
            rangeDict = null;
            selectedSkill = null;
            currentState = State.SELECTING_UNIT;
          }
        }
      }
      else if (inputEventMouseButton.IsPressed() && inputEventMouseButton.ButtonIndex == MouseButton.WheelUp){
        camera.Zoom = new Vector2(camera.Zoom.X+0.2f, camera.Zoom.Y+0.2f);
      }
      else if (inputEventMouseButton.IsPressed() && inputEventMouseButton.ButtonIndex == MouseButton.WheelDown){
        camera.Zoom = new Vector2(camera.Zoom.X-0.2f, camera.Zoom.Y-0.2f);
      }
    }
      //GD.Print(GetTree().Root.GetNode("Node2D").GetNode<Node2D>("Node2D").GetGlobalMousePosition());
  }

  
  public void SelectSkill(int id){
    selectedSkill = selectedTile.GetUnit().skills.ElementAt(id);
    currentState = State.TARGET_SKILL;

    rangeDict = Range.GetAccessibleTiles(selectedSkill, parentGame.map);
    HighlightTiles();
    selectedTile.RemoveSelection();
    selectedTile = null;
  }

  private void HighlightTiles(){
    foreach (var kvp in rangeDict) {
      parentGame.map.tileMap[kvp.Key.x, kvp.Key.y].HighlightTile();
    }
  }

  private void RemoveHighlights(){
    foreach (var kvp in rangeDict) {
      parentGame.map.tileMap[kvp.Key.x, kvp.Key.y].RemoveHighlight();
    }
  }

  private enum State{
    SELECTING_UNIT,
    SELECTING_SKILL,
    TARGET_MOVEMENT,
    TARGET_SKILL
  }
}
