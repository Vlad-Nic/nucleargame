using Godot;

public partial class PauseMenu : CanvasLayer
{
	private Button _resumeButton = null!;
	private Button _restartButton = null!;
	private Button _mainMenuButton = null!;
	private Button _quitButton = null!;
	private Input.MouseModeEnum _previousMouseMode;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		_resumeButton = GetNode<Button>("Panel/VBoxContainer/ResumeButton");
		_restartButton = GetNode<Button>("Panel/VBoxContainer/RestartButton");
		_mainMenuButton = GetNode<Button>("Panel/VBoxContainer/MainMenuButton");
		_quitButton = GetNode<Button>("Panel/VBoxContainer/QuitButton");

		_resumeButton.Pressed += OnResume;
		_restartButton.Pressed += OnRestart;
		_mainMenuButton.Pressed += OnMainMenu;
		_quitButton.Pressed += OnQuit;
	}

	public override void _Input(InputEvent evt)
	{
		if (evt is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Escape)
			Toggle();
	}

	public void Toggle()
	{
		if (Visible) CloseMenu();
		else OpenMenu();
	}

	public void OpenMenu()
	{
		_previousMouseMode = Input.MouseMode;
		Visible = true;
		GetTree().Paused = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public void CloseMenu()
	{
		Visible = false;
		GetTree().Paused = false;
		Input.MouseMode = _previousMouseMode;
	}

	private void OnResume()
	{
		CloseMenu();
	}

	private void OnRestart()
	{
		GetTree().Paused = false;
		Input.MouseMode = Input.MouseModeEnum.Hidden;
		GetTree().ReloadCurrentScene();
	}

	private void OnMainMenu()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}

	private void OnQuit()
	{
		GetTree().Quit();
	}
}
