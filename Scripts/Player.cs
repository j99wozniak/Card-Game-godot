

using System.Collections.Generic;

public class Player
{
  private static string[] teamColors = new string[] {"Blue", "Red"};
  public Game game;
  public int team;
  public bool isCPU;

  public Condition winCondition;
  public Condition loseCondition;

  public List<Unit> deck;

  public Player(Game game, int team, bool isCPU){
    this.game = game;
    this.team = team;
    this.isCPU = isCPU;
  }

  public string GetTeamColor(){
    return teamColors[team-1];
  }
}