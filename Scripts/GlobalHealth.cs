using Godot;
using System;

public partial class GlobalHealth : Node
{
	public static GlobalHealth Instance { get; private set; }

	[Export] public float MaxHealth = 100f;
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
		CurrentHealth = MaxHealth;
		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
	}
	private void TriggerGameOver()
	{
		GD.Print("Global health is empty");
		SetProcess(false); //stop multiple triggers
		//GetTree().ChangeSceneToFile("res://GameOver.tscn");
	}
}
