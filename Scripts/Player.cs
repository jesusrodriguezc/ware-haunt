using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed { get; set; } = 90.0f;

    [Export]
    public float Force { get; set; } = 10.0f;

	[Export(PropertyHint.Range, "0,8,1")]
	public int PlayerStackLevel { get; set; } = 0;

	[Export(PropertyHint.Range, "4,64,1")]
	public float LevelHeight { get; set; } = 12.0f;

	private StackEntityBase _stackBase;

	public override void _Ready()
	{
		_stackBase = new StackEntityBase(
			this,
			GetNodeOrNull<Node2D>("Sprite"),
			GetNodeOrNull<CollisionShape2D>("Collision"),
			GetNodeOrNull<Sprite2D>("Shadow")
		);
	}

	public override void _PhysicsProcess(double delta)
	{
		StackSystem.SetPlayerStackLevel(PlayerStackLevel);
		_stackBase?.ApplyVisualOffset(PlayerStackLevel, LevelHeight);
		_stackBase?.UpdateInteractionState(PlayerStackLevel);
		_stackBase?.UpdateShadow(PlayerStackLevel, false);

		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Velocity = direction * Speed;
		MoveAndSlide();

		System.Collections.Generic.HashSet<ulong> pushedThisFrame = new();
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision2D collision = GetSlideCollision(i);
			Node collider = collision.GetCollider() as Node;
			if (collider == null || !pushedThisFrame.Add(collider.GetInstanceId()))
			{
				continue;
			}

			Vector2 pushDir = -collision.GetNormal();
			Vector2 impulse = pushDir * Force;
			if (collider is BoxController boxController)
			{
				boxController.ApplyPushImpulse(impulse, PlayerStackLevel);
			}
			else if (collider is RigidBody2D body)
			{
				body.ApplyCentralImpulse(impulse);
			}
		}
	}
}
