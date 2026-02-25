using Godot;

public partial class MainMenu : Control
{
	[Export] AudioStreamPlayer _menuMusic;
	
	public override async void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetNode<Button>("Panel/VBoxContainer/PlayButton").Pressed += OnPlay;
		GetNode<Button>("Panel/VBoxContainer/QuitButton").Pressed += OnQuit;
		_menuMusic.Play();
		
		var tm = GetNode<TransitionManager>("/root/TransitionManager");
		await tm.FadeInTransition();
	}

	private async void OnPlay()
	{
		var tm = GetNode<TransitionManager>("/root/TransitionManager");
		await tm.FadeOutTransition();
		GetTree().ChangeSceneToFile("res://Scenes/Intro.tscn");
	}

	private void OnQuit()
	{
		GetTree().Quit();
	}
}
