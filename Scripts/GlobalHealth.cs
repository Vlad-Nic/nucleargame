using Godot;
using System;

public partial class GlobalHealth : Node
{
	public static GlobalHealth Instance { get; private set; }

	[Export] public float MaxHealth = 10f;
	public float CurrentHealth { get; private set; }

	[Signal]
	public delegate void HealthChangedEventHandler(float current, float max);

	public override void _Ready()
	{
		Instance = this;
		CurrentHealth = MaxHealth;
		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
	}

	public void Drain(float amount)
	{
		CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, MaxHealth);
		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
		if (CurrentHealth <= 0)
		{
			TriggerGameOver();
		}
	}

	public void Heal(float amount)
	{
		CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
	}

	public void ResetHealth()
	{
		SetProcess(true);
		CurrentHealth = MaxHealth;
		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
	}
	private void TriggerGameOver()
	{
		SetProcess(false); //stop multiple triggers
		//GetTree().ChangeSceneToFile("res://GameOver.tscn");
	}
}
