using System;
using System.Collections.Generic;
using Godot;

public class HudController{
  Color[] teamColors = new Color[2];
  Game parentGame;
  CanvasLayer hud;
  public bool isMouseOverUI = false;

  MarginContainer unitScrollContainer;
  TextureProgressBar scrollBackground;
  TextureProgressBar staminaBar;
  Label staminaLabel;
  Sprite2D portraitSprite;
  TextureProgressBar healthBar;
  Label healthLabel;
  Label unitNameLabel;
  VBoxContainer unitEffectList;

  MarginContainer tileScrollContainer;
  Sprite2D tileSprite;
  Label tileCoordinatesLabel;
  Label tileNameLabel;
  VBoxContainer tileEffectsList;

  MarginContainer conditionsScrollContainer;
  Button hideConditionsButton;
  Label winText;
  Label loseText;

  public MarginContainer conditionsClosedScrollContainer;
  Button showConditionsButton;

  PackedScene effectItem = (PackedScene)GD.Load("res://effect_item.tscn"); 
  Texture2D portrait = (Texture2D)GD.Load("res://Sprites/Units/Portraits/portrait_Archer_Blue1.png");
  Texture2D icon = (Texture2D)GD.Load("res://Sprites/Misc/Heal_Icon.png");

  public HudController(CanvasLayer hud, Game parentGame){
    this.hud = hud;
    this.parentGame = parentGame;
    teamColors[0] = new Color(0.224f, 0.671f, 1);
    teamColors[1] = new Color(0.463f, 0, 0.16f);
    // TODO add more team colors

    // Unit Bar
      // Up
    unitScrollContainer = (MarginContainer)hud.FindChild("UnitScrollContainer");
    scrollBackground = (TextureProgressBar)hud.FindChild("ScrollBackground");
    staminaBar = (TextureProgressBar)hud.FindChild("StaminaBar");
    staminaLabel = (Label)hud.FindChild("StaminaLabel");
    portraitSprite = (Sprite2D)hud.FindChild("PortraitSprite");
    healthBar = (TextureProgressBar)hud.FindChild("HealthBar");
    healthLabel = (Label)hud.FindChild("HealthLabel");
      // Bottom
    unitNameLabel = (Label)hud.FindChild("UnitNameLabel");
    unitEffectList = (VBoxContainer)hud.FindChild("UnitEffectsList");
    unitEffectList = (VBoxContainer)hud.FindChild("UnitEffectsList");

    // Tile scroll
    tileScrollContainer = (MarginContainer)hud.FindChild("TileScrollContainer");
    tileSprite = (Sprite2D)hud.FindChild("TileSprite");
    tileCoordinatesLabel = (Label)hud.FindChild("TileCoordinatesLabel");
    tileNameLabel = (Label)hud.FindChild("TileNameLabel");
    tileEffectsList = (VBoxContainer)hud.FindChild("TileEffectsList");

    // Conditions
      // Opened
    conditionsScrollContainer = (MarginContainer)hud.FindChild("ConditionsScrollContainer");
    hideConditionsButton = (Button)hud.FindChild("HideConditionsButton");
    winText = (Label)hud.FindChild("WinText");
    loseText = (Label)hud.FindChild("LoseText");
      // Closed
    conditionsClosedScrollContainer = (MarginContainer)hud.FindChild("ConditionsClosedScrollContainer");
    showConditionsButton = (Button)hud.FindChild("ShowConditionsButton");

    // Signals to disable clickthrough
    unitScrollContainer.Connect("mouse_entered", Callable.From(OnMouseEntered));
    unitScrollContainer.Connect("mouse_exited", Callable.From(OnMouseExited));

    tileScrollContainer.Connect("mouse_entered", Callable.From(OnMouseEntered));
    tileScrollContainer.Connect("mouse_exited", Callable.From(OnMouseExited));

    conditionsScrollContainer.Connect("mouse_entered", Callable.From(OnMouseEntered));
    conditionsScrollContainer.Connect("mouse_exited", Callable.From(OnMouseExited));
    hideConditionsButton.Connect("pressed", Callable.From(HideConditions));
    hideConditionsButton.Connect("mouse_entered", Callable.From(OnMouseEntered));
    hideConditionsButton.Connect("mouse_exited", Callable.From(OnMouseExited));

    showConditionsButton.Connect("pressed", Callable.From(ShowConditions));
    showConditionsButton.Connect("mouse_entered", Callable.From(OnMouseEntered));
    showConditionsButton.Connect("mouse_exited", Callable.From(OnMouseExited));

    unitScrollContainer.Visible = false;
    ShowConditions();

    // Mouse events should pass through scroll elements so that isMouseOverUI works correctly
    foreach(ScrollContainer sc in hud.FindChildren("", "ScrollContainer")){
      sc.GetVScrollBar().MouseFilter = Control.MouseFilterEnum.Pass;
    }
  }

  public void ShowUnitBar(Unit unit){
    unitScrollContainer.Visible = true;

    scrollBackground.TintOver = teamColors[unit.player.team-1];
    // Set bars
    staminaBar.MaxValue = unit.maxStamina;
    staminaBar.Value = unit.currentStamina;
    staminaLabel.Text = $"{unit.currentStamina}/{unit.maxStamina}";

    healthBar.MaxValue = unit.maxHp;
    healthBar.Value = unit.currentHp;
    healthLabel.Text = $"{unit.currentHp}/{unit.maxHp}";

    // TODO actually have portraits
    portraitSprite.Texture = portrait;
    unitNameLabel.Text = unit.unitName;
    foreach(Node child in unitEffectList.GetChildren()){
      unitEffectList.RemoveChild(child);
      child.QueueFree();
    }
    foreach(KeyValuePair<Trigger, LinkedList<UnitEffect>> list in unit.unitEffects){
      foreach(UnitEffect e in list.Value){
        HBoxContainer effectItemInst = (HBoxContainer)effectItem.Instantiate();
        ((Sprite2D)effectItemInst.FindChild("EffectIcon")).Texture = icon;
        ((Label)effectItemInst.FindChild("EffectText")).Text = e.name;
        unitEffectList.AddChild(effectItemInst);
      }
    }
  }
  public void HideUnitBar(){
     unitScrollContainer.Visible = false;
     isMouseOverUI = false;
  }

  public void UpdateTileInfobox(Tile tile){
    tileSprite.Texture = tile.tileSprite.Texture;
    tileCoordinatesLabel.Text = $"({tile.x},{tile.y})";
    tileNameLabel.Text = tile.tileName;
    foreach(Node child in tileEffectsList.GetChildren()){
      tileEffectsList.RemoveChild(child);
      child.QueueFree();
    }
    foreach(TileEffect effect in tile.tileEffects){
      HBoxContainer effectItemInst = (HBoxContainer)effectItem.Instantiate();
      ((Sprite2D)effectItemInst.FindChild("EffectIcon")).Texture = icon;
      ((Label)effectItemInst.FindChild("EffectText")).Text = effect.name;
      tileEffectsList.AddChild(effectItemInst);
    }
  }

  private void OnMouseEntered(){
    isMouseOverUI = true;
  }
  private void OnMouseExited(){
    isMouseOverUI = false;
  }

  private void HideConditions(){
    conditionsScrollContainer.Visible = false;
    conditionsClosedScrollContainer.Visible = true;
    isMouseOverUI = false;
  }
  public void ShowConditions(){
    conditionsScrollContainer.Visible = true;
    conditionsClosedScrollContainer.Visible = false;
    isMouseOverUI = false;
    winText.Text = parentGame.players[parentGame.currentTeam-1].winCondition.ToString();
    loseText.Text = parentGame.players[parentGame.currentTeam-1].loseCondition.ToString();
  }
}