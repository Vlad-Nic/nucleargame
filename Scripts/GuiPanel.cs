using Godot;
using System;

public partial class GuiPanel : Node3D
{
	[Export] protected Area3D _area;
	[Export] protected SubViewport _subViewport;
	
	public virtual void OnFocused()
	{
		// actions enabled
	}
	
	public virtual void OnUnfocused()
	{
		// actions disabled
	}
	
	protected Vector2 WorldHitToViewport(Vector3 worldPos, SubViewport subViewport, Area3D area)
	{
		Vector3 localPos = ToLocal(worldPos);

		const float BASE_WIDTH  = 1.8f;
		const float BASE_HEIGHT = 1.4f;

		var meshInstance = area.GetChild<MeshInstance3D>(0);
		Vector3 scale = meshInstance.GlobalTransform.Basis.Scale;

		float halfWidth  = (BASE_WIDTH  * scale.X) * 0.5f;
		float halfHeight = (BASE_HEIGHT * scale.Y) * 0.5f;

		float u = (localPos.X + halfWidth)  / (halfWidth  * 2f);
		float v = (-localPos.Y + halfHeight) / (halfHeight * 2f);

		return new Vector2(
			u * subViewport.Size.X,
			v * subViewport.Size.Y
		);
	}

}
