using Godot;

public interface IStackEntity
{
	int StackLevel { get; }
	Vector2 GlobalPosition { get; }
}
