using Godot;

public partial class BoxController : RigidBody2D, IStackEntity
{
	[Export]
	public BoxConfig Config { get; set; }

	[Export]
	public NodePath TileMapPath { get; set; }

	[Export(PropertyHint.Range, "0.05,2.0,0.01")]
	public float TileFrictionNormal { get; set; } = 0.85f;

	[Export(PropertyHint.Range, "0.05,2.0,0.01")]
	public float TileFrictionWet { get; set; } = 0.35f;

	[Export(PropertyHint.Range, "0.05,2.0,0.01")]
	public float TileFrictionFallback { get; set; } = 0.6f;

	[Export(PropertyHint.Range, "0,8,1")]
	public int StackLevel { get; set; } = 0;

	[Export(PropertyHint.Range, "4,64,1")]
	public float LevelHeight { get; set; } = 12.0f;

	[Export(PropertyHint.Range, "2,64,1")]
	public float SupportSnapDistance { get; set; } = 14.0f;

	[Export(PropertyHint.Range, "0.01,1.0,0.01")]
	public float LostSupportDelay { get; set; } = 0.12f;

	private TileMap _tileMap;
	private float _baseLinearDamp;
	private float _baseDragFriction = 1.0f;
	private double _unsupportedSeconds;
	private BoxController _supportBox;
	private Vector2 _supportOffset;
	private StackEntityBase _stackBase;
	private CollisionShape2D _mainCollisionShape;
	private bool _isFollowingSupport;

	public override void _Ready()
	{
		_baseLinearDamp = LinearDamp;
		if (Config != null)
		{
			_baseDragFriction = Mathf.Max(0.01f, Config.DragFriction);
		}

		_tileMap = ResolveTileMap();
		_mainCollisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		_stackBase = new StackEntityBase(
			this,
			GetNodeOrNull<Node2D>("Sprite2D"),
			GetNodeOrNull<CollisionShape2D>("InteractionShape"),
			GetNodeOrNull<Sprite2D>("Shadow")
		);

		ApplyPseudoHeightVisual();
		StackSystem.Register(this);
	}

	public override void _ExitTree()
	{
		StackSystem.Unregister(this);
	}

	public override void _PhysicsProcess(double delta)
	{
		UpdatePhysicalCollisionState();
		ApplyGroundFrictionDamp();
		UpdateSupportState(delta);
		_stackBase?.UpdateInteractionState(StackLevel);
		int supportLevel = _supportBox?.StackLevel ?? 0;
		int diff = StackLevel - supportLevel;
		_stackBase?.UpdateShadow(diff, _supportBox != null);
	}

	public void ApplyPushImpulse(Vector2 impulse, int actorStackLevel)
	{
		if (actorStackLevel != StackLevel)
		{
			return;
		}

		float massFactor = Config?.PushMassFactor ?? 1.0f;
		float frictionTile = GetGroundTileFriction();
		float finalFriction = Mathf.Sqrt(_baseDragFriction * frictionTile);
		float safeFriction = Mathf.Max(0.05f, finalFriction);
		float scaledMagnitude = impulse.Length() / (safeFriction * massFactor * Mass);
		Vector2 adjustedImpulse = impulse.Normalized() * scaledMagnitude;
		ApplyCentralImpulse(adjustedImpulse);
	}

	private void ApplyGroundFrictionDamp()
	{
		float frictionTile = GetGroundTileFriction();
		float finalFriction = Mathf.Sqrt(_baseDragFriction * frictionTile);
		float desiredDamp = _baseLinearDamp * (1.0f + finalFriction);

		if (Config != null)
		{
			desiredDamp = Mathf.Clamp(desiredDamp, Config.MinLinearDamp, Config.MaxLinearDamp);
		}

		LinearDamp = desiredDamp;
	}

	private void UpdateSupportState(double delta)
	{
		BoxController newSupport = StackSystem.FindSupportFor(this);
		if (newSupport != null)
		{
			if (_supportBox != newSupport)
			{
				_supportBox = newSupport;
				_supportOffset = GlobalPosition - _supportBox.GlobalPosition;
			}

			_unsupportedSeconds = 0.0f;
			FollowSupportMotion();
			return;
		}

		_supportBox = null;
		_isFollowingSupport = false;
		Freeze = false;
		if (StackLevel <= 0)
		{
			return;
		}

		_unsupportedSeconds += delta;
		if (_unsupportedSeconds >= LostSupportDelay)
		{
			StackLevel -= 1;
			_unsupportedSeconds = 0.0f;
			ApplyPseudoHeightVisual();
		}
	}

	private void FollowSupportMotion()
	{
		if (_supportBox == null || !IsInstanceValid(_supportBox))
		{
			return;
		}

		_isFollowingSupport = true;
		Freeze = true;
		LinearVelocity = Vector2.Zero;
		AngularVelocity = 0.0f;
		GlobalPosition = _supportBox.GlobalPosition + _supportOffset;
	}

	private void ApplyPseudoHeightVisual()
	{
		_stackBase?.ApplyVisualOffset(StackLevel, LevelHeight);
	}

	private TileMap ResolveTileMap()
	{
		if (TileMapPath != null && !TileMapPath.IsEmpty)
		{
			return GetNodeOrNull<TileMap>(TileMapPath);
		}

		return GetTree().Root.FindChild("TileMap", true, false) as TileMap;
	}

	private float GetGroundTileFriction()
	{
		if (_tileMap == null)
		{
			return TileFrictionFallback;
		}

		Vector2 localPos = _tileMap.ToLocal(GlobalPosition);
		Vector2I cell = _tileMap.LocalToMap(localPos);
		Vector2I atlas = _tileMap.GetCellAtlasCoords(0, cell);

		if (atlas == new Vector2I(0, 0))
		{
			return TileFrictionNormal;
		}

		if (atlas == new Vector2I(0, 1))
		{
			return TileFrictionWet;
		}

		return TileFrictionFallback;
	}

	private void UpdatePhysicalCollisionState()
	{
		if (_mainCollisionShape == null)
		{
			return;
		}

		// Different pseudo-heights should not physically collide in XY.
		_mainCollisionShape.Disabled = StackLevel != StackSystem.PlayerStackLevel;
		if (_isFollowingSupport)
		{
			_mainCollisionShape.Disabled = true;
		}
	}
}
