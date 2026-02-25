using Godot;
using System;

public partial class SirenContainer : Node3D
{
	public static SirenContainer Instance { get; private set; }
	
	[Export] public double rotationSpeed = 90.0;
	[Export] public AudioStreamPlayer3D _siren;
	[Export] public AudioStreamPlayer3D _finalwarning;
	private bool _isActive = false;
	
	public override void _Process(double delta)
	{
		if(_isActive)
		{
			float degPerFrame = (float)(rotationSpeed * delta);
			float radPerFrame = Mathf.DegToRad(degPerFrame);
			this.Rotate(Vector3.Up, radPerFrame);
		}
	}
	
	public override void _Ready()
	{
		Instance = this;
	}
	
	public void StartSiren()
	{
		this.Visible = true;
		_isActive = true;
		Node3D lighting = GetNode<Node3D>("../Lighting");
		lighting.Visible = false;
		
		if (_finalwarning != null && !_finalwarning.Playing)
		{
			_finalwarning.Play();
		}
		
		if (_siren != null && !_siren.Playing)
		{
			_siren.Play();
		}
	}
	
	public void Reset()
	{
		if (!IsNodeReady()) return;  // Node has been deleted
		
		_isActive = false;
		this.Visible = false;
		// Restore the lighting
		Node3D lighting = GetNode<Node3D>("../Lighting");
		lighting.Visible = true;
	}
}
