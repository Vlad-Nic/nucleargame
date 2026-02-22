using Godot;
using System;

public partial class Game : Node3D
{
	[Export] public Camera3D PlayerCam;
	[Export] public Vector3 _zoomedPosition;
	
	private bool focusedOnMonitor = false;
	private Vector3 _targetRotation;
	private Vector3 _targetPosition;
	private Vector3 _defaultPosition;
	
	public override void _Ready()
	{
		_defaultPosition = PlayerCam.Position;
		_targetPosition = PlayerCam.Position;
		_targetRotation = new Vector3(0,0,0);
	}
	
	public override void _Process(double delta)
	{
		_targetRotation.Y = Mathf.Clamp(_targetRotation.Y, -90, 90);
		PlayerCam.RotationDegrees = PlayerCam.RotationDegrees.Lerp(_targetRotation, 5f * (float)delta);
		PlayerCam.Position = PlayerCam.Position.Lerp(_targetPosition, 5f * (float)delta);
		
		if(Input.IsActionJustPressed("Right"))
		{
			_targetRotation += new Vector3(0,-90,0);
		}
		else if(Input.IsActionJustPressed("Left"))
		{
			_targetRotation += new Vector3(0,90,0);
		}
		
		if(Input.IsActionJustPressed("mouse_left") && !focusedOnMonitor)
		{
			focusedOnMonitor = true;
			_targetPosition = _zoomedPosition;
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		if(Input.IsActionJustPressed("mouse_right") && focusedOnMonitor)
		{
			focusedOnMonitor = false;
			_targetPosition = _defaultPosition;
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	}
}
