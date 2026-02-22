using Godot;

public partial class WinScreen : Control
{
	[Export] public AudioStream WinAudio { get; set; } = null!;

	private Label _textLabel = null!;
	private Timer _charTimer = null!;
	private AudioStreamPlayer _audio = null!;
	private string _fullText = "";
	private int _charIndex = 0;
	private bool _done = false;

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Stop;

		_textLabel = GetNode<Label>("TextLabel");
		_charTimer = GetNode<Timer>("CharTimer");
		_audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

		_fullText = GetNode<TextEdit>("WinText").Text.Trim();
		_textLabel.Text = "";

		_charTimer.WaitTime = 0.074;
		_charTimer.OneShot = false;
		_charTimer.Timeout += OnCharTick;
		_charTimer.Start();

		if (WinAudio != null)
		{
			_audio.Stream = WinAudio;
			_audio.Play();
		}
	}

	
	public override void _UnhandledInput(InputEvent evt)
	{
		if (evt is InputEventKey k && k.Pressed && !k.Echo)
			HandleInput();
	}

	
	public override void _GuiInput(InputEvent evt)
	{
		if (evt is InputEventMouseButton mb && mb.Pressed)
			HandleInput();
	}

	private void HandleInput()
	{
		if (!_done)
		{
			_charTimer.Stop();
			_audio.Stop();
			_textLabel.Text = _fullText;
			_done = true;
		}
		else
		{
			GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
		}
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
}
