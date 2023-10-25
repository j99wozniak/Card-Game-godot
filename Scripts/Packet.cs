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
  public Packet(string name, Type type, Trigger trigger, int value, Unit target, Unit source, Command command){
    this.name = name;
    this.type = type;
    this.trigger = trigger;
    this.value = value;
    this.target = target;
    this.source = source;
    this.commands = new List<Command>(new[]{command});
  }
  public Packet(Skill skill, Unit target, int value, Damage damageCommand){
    this.name = skill.name;
    this.type = skill.type;
    this.trigger = Trigger.OnAttacking;
    this.value = value;
    this.target = target;
    this.source = skill.source;
    this.commands = new List<Command>(new[]{damageCommand});
  }
  public Packet(Skill skill, Unit target, int value, Heal healCommand){
    this.name = skill.name;
    this.type = skill.type;
    this.trigger = Trigger.OnHealing;
    this.value = value;
    this.target = target;
    this.source = skill.source;
    this.commands = new List<Command>(new[]{healCommand});
  }
  public Packet(Skill skill, ConsumeStamina consumeStaminaCommand){
    this.name = skill.name;
    this.type = Type.Biological;
    this.trigger = Trigger.OnConsumeStamina;
    this.value = skill.currentCost;
    this.target = skill.source;
    this.source = skill.source;
    this.commands = new List<Command>(new[]{consumeStaminaCommand});
  }
} 

public abstract class Command
{
  public abstract void Apply(Packet packet);
}

public class Damage : Command
{
  public override void Apply(Packet packet){
    packet.target.currentHp = packet.target.currentHp - packet.value > 0 ? packet.target.currentHp - packet.value : 0;
  }
}

public class Heal : Command
{
  public override void Apply(Packet packet){
    packet.target.currentHp = packet.target.currentHp + packet.value < packet.target.maxHp ? packet.target.currentHp + packet.value : packet.target.maxHp;
  }
}

public class ConsumeStamina : Command
{
  public override void Apply(Packet packet){
    packet.target.currentStamina = packet.target.currentStamina - packet.value > 0 ? packet.target.currentStamina - packet.value : 0;
  }
}

public class ReplenishStamina : Command
{
  public override void Apply(Packet packet){
    packet.target.currentStamina = packet.target.currentStamina + packet.value < packet.target.maxStamina ? packet.target.currentStamina + packet.value : packet.target.maxStamina;
  }
}

public class VampiricHeal : Command
{
  public override void Apply(Packet packet){
    packet.source.currentHp = packet.source.currentHp + packet.value < packet.source.maxHp ? packet.source.currentHp + packet.value : packet.source.maxHp;
  }
}
