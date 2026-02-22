using Godot;
using System;

public partial class GameIdle : Control
{
	[Signal] public delegate void AuthorizedEventHandler();

	[Export] private Timer _timer;
	[Export] private Label _countdownLabel;
	[Export] private Button _authorizeButton;

	public override void _Ready()
	{
		_timer.WaitTime = 10f;
		_timer.Timeout += OnTimerTimeout;
		_authorizeButton.Pressed += OnAuthorizePressed;
		_timer.Start();
	}

	public override void _Process(double delta)
	{
		_countdownLabel.Text = ((int)_timer.TimeLeft).ToString();
	}

	private void OnAuthorizePressed()
	{
		EmitSignal(SignalName.Authorized);
	}

	private void OnTimerTimeout()
	{
		EmitSignal(SignalName.Authorized);
	}
}
