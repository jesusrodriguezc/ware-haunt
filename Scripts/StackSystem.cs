using Godot;
using System.Collections.Generic;

public static class StackSystem
{
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
}
