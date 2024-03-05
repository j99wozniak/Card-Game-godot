using System;
using System.Collections.Generic;
using Godot;

public partial class GameMap : Node2D
{
  public int sizeX;
  public int sizeY;
  public int maxSize {get {return Math.Max(sizeX, sizeY);}} // Longer axis
  public Tile[,] tileMap;
  public Unit[,] unitMap;
  public List<Unit> graveyard = new();
  public int TileID(int x, int y){
    return x*maxSize+y;
  }
  public (int, int) IDtoXY(int ID){
    return (ID / maxSize, ID % maxSize);
  }
  public GameMap(int sizeX, int sizeY){
    this.sizeX = sizeX;
    this.sizeY = sizeY;
    tileMap = new Tile[sizeX,sizeY];
    unitMap = new Unit[sizeX,sizeY];
  }
  
}
