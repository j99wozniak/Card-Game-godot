using Godot;

public class GameMap
{
	public int sizeX;
	public int sizeY;
	public Tile[,] tileMap;
	public Unit[,] unitMap;

	public GameMap(int sizeX, int sizeY){
		this.sizeX = sizeX;
		this.sizeY = sizeY;
		tileMap = new Tile[sizeX,sizeY];
		unitMap = new Unit[sizeX,sizeY];
	}
}
