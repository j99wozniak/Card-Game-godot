using Godot;
using System.Collections.Generic;

public class Packet
{
	public string name;
	public string type;
	public string trigger;
	public int value;
	public Unit target;
	public Unit source;
	public LinkedList<Command> commands;
	public Packet(string name, string type, string trigger, int value, Unit target, Unit source, LinkedList<Command> commands){
		this.name = name;
		this.type = type;
		this.trigger = trigger;
		this.value = value;
		this.target = target;
		this.source = source;
		this.commands = commands;
	}
} 

public abstract class Command
{
	public abstract void Apply(Packet packet);
}

public class Damage : Command
{
  public override void Apply(Packet packet)
  {
	packet.target.currentHp = packet.target.currentHp - packet.value > 0 ? packet.target.currentHp - packet.value : 0;
  }
}

public class Heal : Command
{
  public override void Apply(Packet packet)
  {
	packet.target.currentHp = packet.target.currentHp + packet.value < packet.target.maxHp ? packet.target.currentHp + packet.value : packet.target.maxHp;
  }
}

public class VampiricHeal : Command
{
  public override void Apply(Packet packet)
  {
	packet.source.currentHp = packet.source.currentHp + packet.value < packet.source.maxHp ? packet.source.currentHp + packet.value : packet.source.maxHp;
  }
}
