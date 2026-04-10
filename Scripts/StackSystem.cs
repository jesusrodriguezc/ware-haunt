using Godot;
using System.Collections.Generic;

public static class StackSystem
{
	private const int LevelLayerStartBit = 20;
	private const int MaxSupportedStackLevel = 8;
	public const int MaxStackLevel = MaxSupportedStackLevel;

	public static int PlayerStackLevel { get; private set; } = 0;

	private static readonly List<BoxController> Boxes = new();

	public static void Register(BoxController box)
	{
		if (!Boxes.Contains(box))
		{
			Boxes.Add(box);
		}
	}

	public static void Unregister(BoxController box)
	{
		Boxes.Remove(box);
	}

	public static void SetPlayerStackLevel(int stackLevel)
	{
		PlayerStackLevel = stackLevel;
	}

	public static uint GetLevelCollisionBit(int stackLevel)
	{
		int fixedStackLevel = Mathf.Clamp(stackLevel, 0, MaxSupportedStackLevel);
		return 1u << (LevelLayerStartBit + fixedStackLevel);
	}

    public static BoxController FindSupportFor(BoxController candidate)
	{
		if (candidate.StackLevel <= 0)
		{
			return null;
		}

		foreach (BoxController box in Boxes)
		{
			if (box == candidate || !GodotObject.IsInstanceValid(box))
			{
				continue;
			}

			if (box.StackLevel != candidate.StackLevel - 1)
			{
				continue;
			}

			if (candidate.GlobalPosition.DistanceTo(box.GlobalPosition) <= candidate.SupportSnapDistance)
			{
                return box;
			}

        }

		return null;
	}

	public static int GetSupportLevel(BoxController box)
	{
		BoxController support = FindSupportFor(box);
		return support?.StackLevel ?? 0;
	}

	public static List<BoxController> GetBoxesAboveRecursive(BoxController root)
	{
		List<BoxController> result = new();
		HashSet<ulong> visited = new();
		CollectAbove(root, result, visited);
		return result;
	}

	private static void CollectAbove(BoxController support, List<BoxController> result, HashSet<ulong> visited)
	{
		foreach (BoxController box in Boxes)
		{
			if (!GodotObject.IsInstanceValid(box))
			{
				continue;
			}

			if (!visited.Add(box.GetInstanceId()))
			{
				continue;
			}

			if (box == support)
			{
				continue;
			}

			if (box.StackLevel != support.StackLevel + 1)
			{
				continue;
			}

			if (box.GlobalPosition.DistanceTo(support.GlobalPosition) > box.SupportSnapDistance)
			{
				continue;
			}

			BoxController supportOfBox = FindSupportFor(box);
			if (supportOfBox != support)
			{
				continue;
			}

			result.Add(box);
			CollectAbove(box, result, visited);
		}
	}
}
