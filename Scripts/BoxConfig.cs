using Godot;

[GlobalClass]
public partial class BoxConfig : Resource
{
	[Export(PropertyHint.Range, "0.01,4.0,0.01")]
	public float DragFriction { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.1,50.0,0.1")]
	public float PushMassFactor { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.1,30.0,0.1")]
	public float MinLinearDamp { get; set; } = 2.0f;

	[Export(PropertyHint.Range, "0.1,30.0,0.1")]
	public float MaxLinearDamp { get; set; } = 14.0f;
}
