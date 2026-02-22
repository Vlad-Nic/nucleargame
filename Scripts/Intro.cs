using Godot;

public partial class Intro : Control
{
	[Export] public AudioStream IntroAudio { get; set; } = null!;

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

		_fullText = GetNode<TextEdit>("IntroText").Text.Trim();
		_textLabel.Text = "";

		_charTimer.WaitTime = 0.08;
		_charTimer.OneShot = false;
		_charTimer.Timeout += OnCharTick;
		_charTimer.Start();

		if (IntroAudio != null)
		{
			_audio.Stream = IntroAudio;
			_audio.Play();
		}
	}

	public override void _Input(InputEvent evt)
	{
		if (evt is InputEventKey key && key.Pressed && !key.Echo)
			Finish();
		else if (evt is InputEventMouseButton mb && mb.Pressed)
			Finish();
	}

	private void OnCharTick()
	{
		if (_charIndex >= _fullText.Length)
		{
			_charTimer.Stop();
			if (!_done)
				GetTree().CreateTimer(2.0).Timeout += LoadGame;
			_done = true;
			return;
		}

		_textLabel.Text += _fullText[_charIndex].ToString();
		_charIndex++;
	}

	private void Finish()
	{
		if (_done) return;
		_done = true;
		_charTimer.Stop();
		_audio.Stop();
		_textLabel.Text = _fullText;
		GetTree().CreateTimer(0.5).Timeout += LoadGame;
	}

	private void LoadGame()
	{
		GetTree().ChangeSceneToFile("res://Game/Game.tscn");
	}
}
