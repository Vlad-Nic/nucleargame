using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("Panel/VBoxContainer/PlayButton").Pressed += OnPlay;
		GetNode<Button>("Panel/VBoxContainer/QuitButton").Pressed += OnQuit;
	}

	private void OnPlay()
	{
		GetTree().ChangeSceneToFile("res://Game/Game.tscn");
	}

	private void OnQuit()
	{
		GetTree().Quit();
	}
}
