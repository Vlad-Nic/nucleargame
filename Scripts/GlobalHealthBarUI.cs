using Godot;
using System.Threading.Tasks;

public partial class GlobalHealthBarUI : Control
{
	private ProgressBar _bar;
	[Export] public Timer timer;
	[Export] public Label _timeRemaining;
	[Export] public Label _halfHealthWarning;
	[Export] public AudioStreamPlayer _reactorWarning;
	private bool _hasFlashed50Percent = false;

	public override void _Ready()
	{
		_bar = GetNode<ProgressBar>("ProgressBar");

		GlobalHealth.Instance.HealthChanged += OnHealthChanged;
		OnHealthChanged(GlobalHealth.Instance.CurrentHealth,GlobalHealth.Instance.MaxHealth);
		
		timer.WaitTime = 240f;
		
		timer.Timeout += OnTimerTimeout;
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
		timer.Timeout -= OnTimerTimeout;
	}

	private void OnHealthChanged(float current, float max)
	{
		_bar.MaxValue = max;
		_bar.Value = current;
		
		float healthPercent = current / max;
		if(healthPercent <= 0.5f && !_hasFlashed50Percent)
		{
			_hasFlashed50Percent = true;
			FlashLabel(_halfHealthWarning);
			_reactorWarning.Play();
		}
	}
	
	private async void FlashLabel(Label label, int flashCount = 5)
	{
		for (int i = 0; i < flashCount; i++)
		{
			label.Visible = false;
			await Task.Delay(500);
			label.Visible = true;
			await Task.Delay(500);
		}
		label.Visible = false;
	}

	private void OnTimerTimeout()
	{
		GetTree().ChangeSceneToFile("res://Scenes/WinScreen.tscn");
	}
}
