using Godot;
using System;
public partial class RodGui : GuiPanel
{
	[Export] private Area3D _area;
	[Export] private SubViewport _subViewport;
	public override void _Ready()
	{
		_subViewport.GuiDisableInput = true;
		_area.InputEvent += OnAreaInput;
	}

	private void OnAreaInput(Node camera, InputEvent inputEvent, Vector3 position, Vector3 normal, long shapeIdx)
	{
		if(!_focused) return;

		if(inputEvent is InputEventMouseButton || inputEvent is InputEventMouseMotion)
		{
			// Convert 3D position to local UV coordinates
			Vector3 localPos = ToLocal(position);

			// Scale to SubViewport size - adjust these multipliers to match your mesh size
			Vector2 viewportPos = new Vector2(
				(localPos.X + 0.9f) / 1.8f * _subViewport.Size.X,
				(-localPos.Y + 0.7f) / 1.4f * _subViewport.Size.Y
			);

			if(inputEvent is InputEventMouseButton btn)
				btn.Position = viewportPos;
			else if(inputEvent is InputEventMouseMotion mot)
				mot.Position = viewportPos;
		}

		_subViewport.PushInput(inputEvent);
	}
	private bool _focused = false;
	public override void OnFocused()
	{
		_focused = true;
		_subViewport.GuiDisableInput = false;
	}
	public override void OnUnfocused()
	{
		_focused = false;
		_subViewport.GuiDisableInput = true;
	}

	public override void _Input(InputEvent inputEvent)
	{
		if(!_focused) return;

		if(inputEvent is InputEventKey)
		{
			_subViewport.PushInput(inputEvent);
		}
	}
}
