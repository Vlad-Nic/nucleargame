using Godot;
using System;

public partial class Game : Node3D
{
	[Export] public Node3D _centerPanel;
	[Export] public Node3D _leftPanel;
	[Export] public Node3D _rightPanel;
	
	[Export] public Camera3D PlayerCam;
	[Export] public Vector3 _zoomedPositionCenter = new Vector3(0.071f, 1.234f, 4.5f);
	[Export] public Vector3 _zoomedPositionLeft = new Vector3(-1.4f, 1.256f, 6.34f);
	[Export] public Vector3 _zoomedPositionRight = new Vector3(2.2f, 1.246f, 6.187f);
	
	private bool focusedOnMonitor = false;
	private Vector3 _targetRotation;
	private Vector3 _targetPosition;
	private Vector3 _defaultPosition;
	private GuiPanel _focusedPanel = null;
	
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
		
		if(!focusedOnMonitor)
		{
			if(Input.IsActionJustPressed("Right"))
			{
				_targetRotation += new Vector3(0,-90,0);
			}
			else if(Input.IsActionJustPressed("Left"))
			{
				_targetRotation += new Vector3(0,90,0);
			}
		}

		
		if(Input.IsActionJustPressed("mouse_left") && !focusedOnMonitor)
		{
			focusedOnMonitor = true;
			Input.MouseMode = Input.MouseModeEnum.Visible;
			
			if(_targetRotation.Y == -90)
			{
				_targetPosition = _zoomedPositionRight;
				_focusedPanel = _rightPanel as GuiPanel;
				_focusedPanel?.OnFocused();
			}
			else if(_targetRotation.Y == 90)
			{
				_targetPosition = _zoomedPositionLeft;
				_focusedPanel = _leftPanel as GuiPanel;
				_focusedPanel?.OnFocused();
			}
			else
			{
				_targetPosition = _zoomedPositionCenter;
				_focusedPanel = _centerPanel as GuiPanel;
				_focusedPanel?.OnFocused();
			}
		}
		
		if(Input.IsActionJustPressed("mouse_right") && focusedOnMonitor)
		{
			focusedOnMonitor = false;
			_targetPosition = _defaultPosition;
			Input.MouseMode = Input.MouseModeEnum.Captured;
			_focusedPanel?.OnUnfocused();
			_focusedPanel = null;
		}
	}
}
