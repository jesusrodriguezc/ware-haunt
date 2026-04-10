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
		MotionMode = MotionModeEnum.Floating;
		PlatformFloorLayers = 0;
		PlatformWallLayers = 0;

		_stackBase = new StackEntityBase(
			this,
			GetNodeOrNull<Node2D>("Sprite"),
			GetNodeOrNull<CollisionShape2D>("Collision"),
			GetNodeOrNull<Sprite2D>("Shadow")
		);
	}

	public override void _PhysicsProcess(double delta)
	{
        #region Debug control
        if (Input.IsActionJustPressed("stack_down"))
		{
			PlayerStackLevel = Mathf.Max(0, PlayerStackLevel - 1);
		}

		if (Input.IsActionJustPressed("stack_up"))
		{
			PlayerStackLevel = Mathf.Min(StackSystem.MaxStackLevel, PlayerStackLevel + 1);
		}
        #endregion

        StackSystem.SetPlayerStackLevel(PlayerStackLevel);
		uint levelMask = StackSystem.GetLevelCollisionBit(PlayerStackLevel);
		CollisionLayer = levelMask;
		CollisionMask = levelMask;
		_stackBase?.ApplyVisualOffset(PlayerStackLevel, LevelHeight);
		_stackBase?.UpdateInteractionState(PlayerStackLevel);
		_stackBase?.UpdateShadow(PlayerStackLevel, false);

		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Velocity = direction * Speed;
		MoveAndSlide();

		if (direction == Vector2.Zero)
		{
			return;
		}

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
			if (direction.Dot(pushDir) <= 0.1f)
			{
				continue;
			}

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
