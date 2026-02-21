using Godot;
using System;

public partial class Game : Node3D
{
	[Export] public Camera3D PlayerCam;
	
	
	private Vector3 _targetRotation;
	
	public override void _Ready()
	{
		_targetRotation = new Vector3(0,0,0);
	}
	
	public override void _Process(double delta)
	{
		
		_targetRotation.Y = Mathf.Clamp(_targetRotation.Y, -90, 90);
		PlayerCam.RotationDegrees = PlayerCam.RotationDegrees.Lerp(_targetRotation, 5f * (float)delta);
		
		if(Input.IsActionJustPressed("Right"))
		{
			_targetRotation += new Vector3(0,-90,0);
		}
		else if(Input.IsActionJustPressed("Left"))
		{
			_targetRotation += new Vector3(0,90,0);
		}
	}
}
