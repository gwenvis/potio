using Godot;

namespace Potio;

public interface IHurtable
{
    void Hurt(Node2D who, double amount);
}