using Godot;
using System;
using System.Threading.Tasks;

public partial class GlobalHealth : Node
{	
	public static GlobalHealth Instance { get; private set; }

	public float _maxHealth = 60f;

	[Export]
	public float MaxHealth
	{
		get => _maxHealth;
		set => _maxHealth = value;
	}
	
	public float CurrentHealth { get; private set; }

	[Signal]
	public delegate void HealthChangedEventHandler(float current, float max);
	
	public override void _EnterTree()
	{
		if (Instance != null && Instance != this)
		{
			try
			{
				Instance.QueueFree();
			}
			catch (ObjectDisposedException)
			{
				// Old instance already disposed, that's fine
			}
		}
		Instance = this;
	}

	public override void _Ready()
	{
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
	
	private async void TriggerGameOver()
	{
		SetProcess(false);
		SirenContainer.Instance.StartSiren();
		await Task.Delay(10000);
		
		try
		{
			GetTree().ChangeSceneToFile("res://Scenes/GameOver.tscn");
		}
		catch (ObjectDisposedException)
		{
			// GlobalHealth was disposed, that's okay
		}
	}
}
