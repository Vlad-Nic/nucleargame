using Godot;

public partial class GameComplete : Control
{
	[Signal] public delegate void DoneEventHandler();

	private Timer _timer;
	private RandomNumberGenerator _rng = new();
	private bool _timerReady = false;

	public override void _Ready()
	{
		_rng.Randomize();

		_timer = new Timer { OneShot = true };
		AddChild(_timer);
		_timer.Timeout += () => EmitSignal(SignalName.Done);
		_timerReady = true;

		// If already visible when _Ready runs, start the timer now
		if (Visible)
			StartTimerSafe();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationVisibilityChanged && Visible && _timerReady)
		{
			_timer.WaitTime = _rng.RandfRange(5f, 8f);
			CallDeferred(nameof(StartTimerSafe));
		}
	}

	private void StartTimerSafe()
	{
		if (_timer != null && _timer.IsInsideTree())
			_timer.Start();
	}
}
