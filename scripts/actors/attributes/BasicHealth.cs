using Godot;

namespace Potio;

public class BasicHealth(double health) : IHurtable, IHealable
{
    public event System.Action<Node2D, double>? Damaged;
    public event System.Action<Node2D>? Depleted;
    public event System.Action<Node2D, double>? Healed;

    public double StartingHealth => health;
    public double CurrentHealth { get; set; } = health;

#if DEBUG
    public bool IsDebug { get; set; } = false;
#endif
    
    public void Hurt(Node2D who, double amount)
    {
        if (CurrentHealth < Epsilon)
        {
            return;
        }

        CurrentHealth = Max(CurrentHealth - amount, 0);
        if (CurrentHealth < Epsilon)
        {
            #if DEBUG
            if (IsDebug)
            {
                GlobalLogger.Info($"Health depleted by {who.Name}");
            }
            #endif
            Depleted?.Invoke(who);
        }

        #if DEBUG
        if (IsDebug)
        {
            GlobalLogger.Info($"Health {CurrentHealth}/{StartingHealth} (-{amount}) by {who.Name}");
        }
        #endif
        Damaged?.Invoke(who, amount);
    }

    public void Heal(Node2D who, double amount)
    {
        Healed?.Invoke(who, amount);
        #if DEBUG
        if (IsDebug)
        {
            GlobalLogger.Info($"Health +{amount} to {CurrentHealth}/{StartingHealth}");
        }
        #endif
    }
}

public interface IBasicHealth : IHurtable, IHealable
{
    BasicHealth Health { get; }
    void IHurtable.Hurt(Node2D who, double amount) => Health.Hurt(who, amount);
    void IHealable.Heal(Node2D who, double amount) => Health.Heal(who, amount);
}