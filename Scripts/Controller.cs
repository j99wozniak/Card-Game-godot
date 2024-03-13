using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Controller : Node
{
  private Camera2D camera;
  private Viewport viewport;
  Node2D root;
  const float MovementSpeed = 300.0f;
  Dictionary<Key, Vector2> movementKeys = new Dictionary<Key, Vector2>();
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
    root = GetTree().Root.GetNode<Node2D>("Node2D");
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    Vector2 totalMotion = Vector2.Zero;
    foreach (Vector2 motion in movementKeys.Values) {
        totalMotion += motion;
    }
    totalMotion = totalMotion.Normalized() * MovementSpeed * (float)GetProcessDeltaTime();
    Vector2 newCameraPosition = camera.Position + totalMotion;
    if(newCameraPosition!=camera.Position)
      UpdateMouseSelection();
    camera.Position += totalMotion;
  }

  public override void _Input(InputEvent @event)
  {
    if(@event is InputEventKey eventKey){
      GD.Print("Keys");
      if (eventKey.Pressed) {
        if (!movementKeys.ContainsKey(eventKey.Keycode))
            movementKeys[eventKey.Keycode] = Vector2.Zero;

        if (eventKey.Keycode == Key.W)
            movementKeys[eventKey.Keycode] = Vector2.Up;
        if (eventKey.Keycode == Key.A)
            movementKeys[eventKey.Keycode] = Vector2.Left;
        if (eventKey.Keycode == Key.S)
            movementKeys[eventKey.Keycode] = Vector2.Down;
        if (eventKey.Keycode == Key.D)
            movementKeys[eventKey.Keycode] = Vector2.Right;
    } else {
        if (movementKeys.ContainsKey(eventKey.Keycode))
            movementKeys[eventKey.Keycode] = Vector2.Zero;
    }
      if (eventKey.Pressed && eventKey.Keycode == Key.Up){
        parentGame.LoadGame();
      }
      if (eventKey.Pressed && eventKey.Keycode == Key.Down){
        parentGame.SaveGame();
      }
      if (eventKey.Pressed && eventKey.Keycode == Key.Right){
        parentGame.NextTurn();
      }
    }
    else if (@event is InputEventMouseMotion eventMouseMotion && IsInstanceValid(parentGame.map)){
      UpdateMouseSelection();
    }
    else if (@event is InputEventMouseButton inputEventMouseButton && !parentGame.hudController.isMouseOverUI){
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
          parentGame.hudController.ShowUnitBar(selectedTile.GetUnit());
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
        if(camera.Zoom.X<5)
          camera.Zoom = new Vector2(camera.Zoom.X*1.1f, camera.Zoom.Y*1.1f);
        UpdateMouseSelection();
      }
      else if (inputEventMouseButton.IsPressed() && inputEventMouseButton.ButtonIndex == MouseButton.WheelDown){
        if(camera.Zoom.X>0.5)
          camera.Zoom = new Vector2(camera.Zoom.X/1.1f, camera.Zoom.Y/1.1f);
        UpdateMouseSelection();
      }
    }
      //GD.Print(GetTree().Root.GetNode("Node2D").GetNode<Node2D>("Node2D").GetGlobalMousePosition());
  }

  private void UpdateMouseSelection(){
    if(!parentGame.hudController.isMouseOverUI){
      Vector2 currentPosition = root.GetGlobalMousePosition();
      int xTile = (int)Math.Floor((currentPosition.X + Game.tileSize/2) / Game.tileSize);
      int yTile = (int)Math.Floor((currentPosition.Y + Game.tileSize/2) / Game.tileSize);
      if(currentState == State.SELECTING_UNIT){
        if(xTile>=0 && yTile>=0 && xTile < parentGame.map.sizeX && yTile < parentGame.map.sizeY){
          if(selectedTile==null || xTile!=selectedTile.x || yTile!=selectedTile.y){
            selectedTile = parentGame.map.tileMap[xTile, yTile];
            selectedTile.SelectTile();
            parentGame.hudController.UpdateTileInfobox(selectedTile);
            if(selectedTile.GetUnit()!=null){
              parentGame.hudController.ShowUnitBar(selectedTile.GetUnit());
            }
            else{
              parentGame.hudController.HideUnitBar();
            }
          }
        }
      }
      else if(currentState == State.TARGET_SKILL || currentState == State.TARGET_MOVEMENT){
        if(rangeDict.ContainsKey((xTile, yTile))){
          if(selectedTile==null || xTile!=selectedTile.x || yTile!=selectedTile.y){
            selectedTile = parentGame.map.tileMap[xTile, yTile];
            selectedTile.SelectTile();
            parentGame.hudController.UpdateTileInfobox(selectedTile);
          }
        }
      }
    }
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
