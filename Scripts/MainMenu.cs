using Godot;

public partial class MainMenu : Control
{
	[Export] AudioStreamPlayer _menuMusic;
	
	public override void _Ready()
	{
		GetNode<Button>("Panel/VBoxContainer/PlayButton").Pressed += OnPlay;
		GetNode<Button>("Panel/VBoxContainer/QuitButton").Pressed += OnQuit;
		_menuMusic.Play();
	}

	private void OnPlay()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Intro.tscn");
	}

	private void OnQuit()
	{
		GetTree().Quit();
	}
}
