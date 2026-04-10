using Godot;

public partial class StackEntityBase
{
	private readonly CanvasItem _ownerCanvasItem;
	private readonly Node2D _visualRoot;
	private readonly CollisionShape2D _interactionCollider;
	private readonly Sprite2D _shadowSprite;
	private readonly float _baseVisualY;
	private readonly Label _stackLevelLabel;

	public StackEntityBase(Node2D owner, Node2D visualRoot, CollisionShape2D interactionCollider, Sprite2D shadowSprite)
	{
		_ownerCanvasItem = owner;
		_visualRoot = visualRoot;
		_interactionCollider = interactionCollider;
		_shadowSprite = shadowSprite;
		_baseVisualY = visualRoot?.Position.Y ?? 0.0f;
	}

	public void ApplyVisualOffset(int stackLevel, float levelHeight)
	{
		if (_visualRoot == null)
		{
			return;
		}

		float yOffset = -stackLevel * levelHeight;
		_visualRoot.Position = new Vector2(_visualRoot.Position.X, _baseVisualY + yOffset);
		if (_ownerCanvasItem != null)
		{
			_ownerCanvasItem.ZIndex = stackLevel;
		}
	}

	public void UpdateInteractionState(int stackLevel)
	{
		if (_interactionCollider == null)
		{
			return;
		}

		_interactionCollider.Disabled = stackLevel != StackSystem.PlayerStackLevel;
	}

	public void UpdateShadow(int stackLevelDiff, bool isSupportedByElement)
	{
		if (_shadowSprite == null)
		{
			return;
		}

		_shadowSprite.Visible = !isSupportedByElement;
		if (!_shadowSprite.Visible)
		{
			return;
		}

		_shadowSprite.Modulate = GetShadowColor(stackLevelDiff);
	}

	private static Color GetShadowColor(int diff)
	{
		if (diff <= 0)
		{
			return new Color(0, 0, 0, 0);
		}

		if (diff == 1)
		{
			return new Color(46.0f / 255.0f, 34.0f / 255.0f, 47.0f / 255.0f, 1.0f);
		}

		if (diff == 2)
		{
			return new Color(62.0f / 255.0f, 53.0f / 255.0f, 70.0f / 255.0f, 190.0f / 255.0f);
		}

		int alpha = 128;
		if (diff > 3)
		{
			alpha = Mathf.Max(0, 128 - 64 * (diff - 3));
		}

		return new Color(98.0f / 255.0f, 85.0f / 255.0f, 101.0f / 255.0f, alpha / 255.0f);
	}
}
