using Godot;
using System;

public partial class ButtonMash : Control
{
	[Export] public float DrainSpeed = 4f; //Speed at which the progressbar drains
	[Export] public float RefillAmount = 1f; //How much the progressbar will refill when the button is pressed
	[Export] public float GlobalHealthPenalty = 5f; //how much the global health will lose on fail
	
	private ProgressBar _progressbar;
	private Button _button;
	
	public override void _Ready()
	{
		_progressbar = GetNode<ProgressBar>("ProgressBar");
		_button = GetNode<Button>("Button");
		_button.Pressed += OnButtonPressed;
	}
	
	public override void _Process(double delta)
	{
		_progressbar.Value -= DrainSpeed * delta; // Should make the bar empty over time
		_progressbar.Value = Mathf.Max(_progressbar.Value, 0); // Make sure the bar cant go pass 0
		
		if (_progressbar.Value <= 0)
		{
			GD.Print("The Bar is empty");
			GlobalHealth.Instance.Drain(GlobalHealthPenalty);
			_progressbar.Value += 100;
			// GetTree().ChangeSceneToFile("res://GameOver.tscn") from my test file with the mp4
		}
	}
	
	private void OnButtonPressed()
	{
		_progressbar.Value += RefillAmount; //Progress bar goes back up based off the set refill val
		_progressbar.Value = Mathf.Min(_progressbar.Value, _progressbar.MaxValue); //cap at 100
	}
}
