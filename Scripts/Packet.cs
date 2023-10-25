using Godot;
using System.Collections.Generic;

public class Packet
{
  public string name;
  public Type type;
  public Trigger trigger;
  public int value;
  public Unit target;
  public Unit source;
  public List<Command> commands;
  public Packet(string name, Type type, Trigger trigger, int value, Unit target, Unit source, List<Command> commands){
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

public class ConsumeStamina : Command
{
  public override void Apply(Packet packet)
  {
	packet.target.currentStamina = packet.target.currentStamina - packet.value > 0 ? packet.target.currentStamina - packet.value : 0;
  }
}

public class ReplenishStamina : Command
{
  public override void Apply(Packet packet)
  {
	packet.target.currentStamina = packet.target.currentStamina + packet.value < packet.target.maxStamina ? packet.target.currentStamina + packet.value : packet.target.maxStamina;
  }
}

public class VampiricHeal : Command
{
  public override void Apply(Packet packet)
  {
	packet.source.currentHp = packet.source.currentHp + packet.value < packet.source.maxHp ? packet.source.currentHp + packet.value : packet.source.maxHp;
  }
}
