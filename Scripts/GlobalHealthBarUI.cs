using Godot;
using System;

public partial class GlobalHealthBarUI : Control
{
	private ProgressBar _bar;
	[Export] public Timer timer;
	[Export] public Label _timeRemaining;

	public override void _Ready()
	{
		_bar = GetNode<ProgressBar>("ProgressBar");

		GlobalHealth.Instance.HealthChanged += OnHealthChanged;
		OnHealthChanged(GlobalHealth.Instance.CurrentHealth,GlobalHealth.Instance.MaxHealth);
		
		timer.WaitTime = 240f;
		timer.Start();
	}
	
	public override void _Process(double delta)
	{
		int totalSeconds = Mathf.CeilToInt((float)timer.TimeLeft);
		totalSeconds = Mathf.Max(0, totalSeconds);

		int minutes = totalSeconds / 60;
		int seconds = totalSeconds % 60;

		_timeRemaining.Text = $"{minutes}:{seconds:00}";
	}
	
	public override void _ExitTree()
	{
		GlobalHealth.Instance.HealthChanged -= OnHealthChanged;
	}

	private void OnHealthChanged(float current, float max)
	{
		_bar.MaxValue = max;
		_bar.Value = current;
	}
	
	private void OnTimerTimeout()
	{
		GetTree().ChangeSceneToFile("res://Scenes/EndMenu.tscn");
	}
}
