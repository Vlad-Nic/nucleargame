using Godot;
using System;

public partial class MonitorScreen : Control
{
	[Export] private GameIdle _gameIdle;
	[Export] private Control _numble;

	public override void _Ready()
	{
		_gameIdle.Authorized += OnAuthorized;
		SetState(false); // start in idle
	}

	private void SetState(bool alert)
	{
		_gameIdle.Visible = !alert;
		_numble.Visible = alert;
	}

	private void OnAuthorized()
	{
		SetState(true);
	}
}
