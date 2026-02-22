using Godot;

public partial class GameOver : Control
{
	[Export] public AudioStream GameOverAudio { get; set; } = null!;

	private Label _textLabel = null!;
	private Timer _charTimer = null!;
	private AudioStreamPlayer _audio = null!;
	private string _fullText = "";
	private int _charIndex = 0;
	private bool _done = false;

	public override void _Ready()
	{
		_textLabel = GetNode<Label>("TextLabel");
		_charTimer = GetNode<Timer>("CharTimer");
		_audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

		_fullText = GetNode<TextEdit>("GameOverText").Text.Trim();
		_textLabel.Text = "";

		_charTimer.WaitTime = 0.06;
		_charTimer.OneShot = false;
		_charTimer.Timeout += OnCharTick;
		_charTimer.Start();

		if (GameOverAudio != null)
		{
			_audio.Stream = GameOverAudio;
			_audio.Play();
		}
	}

	public override void _Input(InputEvent evt)
	{
		if (evt is InputEventKey key && key.Pressed && !key.Echo)
			GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
		else if (evt is InputEventMouseButton mb && mb.Pressed)
			GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}

	private void OnCharTick()
	{
		if (_charIndex >= _fullText.Length)
		{
			_charTimer.Stop();
			_done = true;
			return;
		}

		_textLabel.Text += _fullText[_charIndex].ToString();
		_charIndex++;
	}

	private void GoToMainMenu()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}
}
