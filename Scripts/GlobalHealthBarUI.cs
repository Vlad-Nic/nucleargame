using Godot;
using System;

public partial class GlobalHealthBarUI : Control
{
	private ProgressBar _bar;

	public override void _Ready()
	{
		_bar = GetNode<ProgressBar>("ProgressBar");

		GlobalHealth.Instance.HealthChanged += OnHealthChanged;

		OnHealthChanged(GlobalHealth.Instance.CurrentHealth,GlobalHealth.Instance.MaxHealth);
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
}
