using Godot;

namespace Potio;

public interface IHealable
{
    void Heal(Node2D who, double amount);
}