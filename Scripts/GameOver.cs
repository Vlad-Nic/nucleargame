using Godot;
public partial class GameOver : Control
{
	[Export] public AudioStream GameOverAudio { get; set; } = null!;
	private Label _textLabel = null!;
	private Timer _charTimer = null!;
	private AudioStreamPlayer _audio = null!;
	private TransitionManager _tm = null!;
	private string _fullText = "";
	private int _charIndex = 0;
	private bool _done = false;
	
	public override async void _Ready()
	{
		_tm = GetNode<TransitionManager>("/root/TransitionManager");
		await _tm.FadeInTransition();
		
		MouseFilter = MouseFilterEnum.Stop;
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
	
	private async void HandleInput()
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
			await _tm.FadeOutTransition();
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
