using Godot;
using System;

public partial class MonitorScreenButton : Control
{
	private enum ScreenState { ButtonMashGame, CriticalError }

	[Export] private ButtonMash _buttonGame;
	[Export] private CriticalError _criticalError;
	private ScreenState _currentState;
	private bool _healthCritical = false;
	
	public override void _Ready()
	{
		GlobalHealth.Instance.HealthChanged += OnHealthChanged;
		SetState(ScreenState.ButtonMashGame);
	}
	
	private void OnHealthChanged(float current, float max)
	{
		if (current <= 0)
		{
			_healthCritical = true;
			SetState(ScreenState.CriticalError);
		}
	}
	
	public override void _ExitTree()
	{
		GlobalHealth.Instance.HealthChanged -= OnHealthChanged;
	}
	
	private void SetState(ScreenState state)
	{
		// Once health is critical, ignore all other state changes
		if (_healthCritical && state != ScreenState.CriticalError)
			return;
		
		_buttonGame.Visible = state == ScreenState.ButtonMashGame;
		_criticalError.Visible = state == ScreenState.CriticalError;
	}
	
}
