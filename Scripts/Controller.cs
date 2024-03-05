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
  private Unit selectedUnit;
  private Targeter targeter;
  private State currentState = State.SELECTING_UNIT;
  Dictionary<(int x, int y), float> rangeDict = new();
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
          if(selectedTile==null || xTile!=selectedTile.x || yTile!=selectedTile.y){
            selectedTile = parentGame.map.tileMap[xTile, yTile];
            selectedTile.SelectTile();
          }
        }
      }
      else if(currentState == State.TARGET_SKILL || currentState == State.TARGET_MOVEMENT){
        if(rangeDict.ContainsKey((xTile, yTile))){
          if(selectedTile==null || xTile!=selectedTile.x || yTile!=selectedTile.y){
            selectedTile = parentGame.map.tileMap[xTile, yTile];
            selectedTile.SelectTile();
          }
        }
      }
    }
    else if (@event is InputEventMouseButton inputEventMouseButton){
      if(inputEventMouseButton.IsPressed() && inputEventMouseButton.ButtonIndex == MouseButton.Right){
        GD.Print("Right Click");
        if(currentState == State.SELECTING_UNIT){
          popupMenu.Clear();
          selectedUnit = selectedTile.GetUnit();
          if(selectedUnit != null){
            int id = 0;
            foreach(Skill skill in selectedUnit.allSkills){
              popupMenu.AddItem($"{skill.name} | {skill.currentPower}🗡️ | {skill.currentCost}⚡", id);
              popupMenu.SetItemTooltip(id, $"{skill.currentRange}🏹");
              if(selectedUnit.currentStamina < skill.currentCost){
                popupMenu.SetItemDisabled(id, true);
              }
              id++;
            }
            popupMenu.Popup(new Rect2I((int)inputEventMouseButton.Position.X, (int)inputEventMouseButton.Position.Y, 10, 10));
            currentState = State.SELECTING_SKILL;
          }
        }
        else if(currentState == State.TARGET_SKILL || currentState == State.TARGET_MOVEMENT){
          if(targeter == null || targeter.RemoveLastTileFromTargets() == Targeter.AlreadyEmpty){
            RemoveHighlights();
            rangeDict = new();
            targeter = null;
            currentState = State.SELECTING_UNIT;
          }
        }
      }
      else if (inputEventMouseButton.IsPressed() && inputEventMouseButton.ButtonIndex == MouseButton.Left){
        GD.Print("Left Click");
        if(currentState == State.TARGET_SKILL){
          if(targeter.AddTileToTargeting(selectedTile) == Targeter.AllTargetsSelected){
            targeter.targetingSkill.UseSkill(targeter.targetedTiles);
            RemoveHighlights();
            rangeDict = new();
            targeter = null;
            currentState = State.SELECTING_UNIT;
          }
        }
        else if(currentState == State.SELECTING_SKILL){
          currentState = State.SELECTING_UNIT;
        }
        else if(selectedTile.GetUnit()!=null && (currentState == State.SELECTING_UNIT || currentState == State.TARGET_MOVEMENT)){
          selectedUnit = selectedTile.GetUnit();
          RemoveHighlights();
          rangeDict = Range.GetAccessibleMovementTiles(selectedUnit, parentGame.map);
          Tile.SetHighlightColor("BLUE_VIOLET");
          HighlightTiles();
          selectedTile.RemoveSelection();
          selectedTile = null;
          currentState = State.TARGET_MOVEMENT;
        }
        else if(currentState == State.TARGET_MOVEMENT){
          if(selectedTile.GetUnit()==null){
            selectedUnit.MoveUnit(selectedTile);
            RemoveHighlights();
            selectedUnit = null;
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

  public void Reset(){
    currentState = State.SELECTING_UNIT;
    RemoveHighlights();
  }

  
  public void SelectSkill(int id){
    Skill selectedSkill = selectedTile.GetUnit().allSkills.ElementAt(id);
    targeter = new Targeter(selectedSkill);
    currentState = State.TARGET_SKILL;
    
    RemoveHighlights();
    rangeDict = Range.GetAccessibleSkillTiles(selectedSkill, parentGame.map);
    Tile.SetHighlightColor("AQUAMARINE");
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
