using Godot;
using System;

public partial class MonitorScreenNumble : Control
{
	private enum ScreenState { Idle, Playing, Complete, Fail }

	[Export] private GameIdle _gameStateIdle;
	[Export] private Numble _numble;
	[Export] private GameComplete _gameComplete;
	[Export] private GameFail _gameFail;

	public override void _Ready()
	{
		_gameStateIdle.Authorized += OnAuthorized;
		_numble.GameWon += () => SetState(ScreenState.Complete);
		_numble.GameLost += () => SetState(ScreenState.Fail);
		_gameComplete.Done += () => SetState(ScreenState.Idle);
		_gameFail.Done += () => SetState(ScreenState.Idle);

		SetState(ScreenState.Idle);
	}

	private void SetState(ScreenState state)
	{
		_gameStateIdle.Visible = state == ScreenState.Idle;
		_gameComplete.Visible = state == ScreenState.Complete;
		_gameFail.Visible = state == ScreenState.Fail;
		_numble.Visible = state == ScreenState.Playing;
	}

	private void OnAuthorized()
	{
		SetState(ScreenState.Playing);
	}
}
