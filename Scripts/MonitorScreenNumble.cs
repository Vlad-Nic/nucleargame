using Godot;
using System;

public partial class MonitorScreenNumble : Control
{
	private enum ScreenState { Idle, Playing, Complete, Fail, CriticalError }

	[Export] private GameIdle _gameStateIdle;
	[Export] private Numble _numble;
	[Export] private GameComplete _gameComplete;
	[Export] private GameFail _gameFail;
	[Export] private CriticalError _criticalError;
	private ScreenState _currentState;
	private bool _healthCritical = false;

	public override void _Ready()
	{
		GlobalHealth.Instance.HealthChanged += OnHealthChanged;
		_gameStateIdle.Authorized += OnAuthorized;
		_numble.GameWon += () => SetState(ScreenState.Complete);
		_numble.GameLost += () => SetState(ScreenState.Fail);
		_gameComplete.Done += () => SetState(ScreenState.Idle);
		_gameFail.Done += () => SetState(ScreenState.Idle);

		SetState(ScreenState.Idle);
	}
	
	public override void _ExitTree()
	{
		GlobalHealth.Instance.HealthChanged -= OnHealthChanged;
	}
	
	private void OnHealthChanged(float current, float max)
	{
		if (current <= 0)
		{
			_healthCritical = true;
			SetState(ScreenState.CriticalError);
		}
	}

	private void SetState(ScreenState state)
	{
		// Once health is critical, ignore all other state changes
		if (_healthCritical && state != ScreenState.CriticalError)
			return;
		
		_gameStateIdle.Visible = state == ScreenState.Idle;
		_gameComplete.Visible = state == ScreenState.Complete;
		_gameFail.Visible = state == ScreenState.Fail;
		_numble.Visible = state == ScreenState.Playing;
		_criticalError.Visible = state == ScreenState.CriticalError;
	}

	private void OnAuthorized()
	{
		SetState(ScreenState.Playing);
	}
}
