using Godot;
using System;
using System.Linq;

public partial class Numble : Control
{
	[Export] public AudioStreamPlayer3D _fail;
	[Export] public AudioStreamPlayer3D _success;
	[Export] public AudioStreamPlayer3D _invalid;
	[Signal] public delegate void GameWonEventHandler();
	[Signal] public delegate void GameLostEventHandler();
	[Export] public float GlobalHealthPenalty = 15f; //how much the global health will lose on fail
	
	[Export] public Font font { get; set; } = null!;

	[Export] private Label _timerLabel = null!;
	[Export] private Label _messageLabel = null!;
	[Export] private GridContainer _guessGrid = null!;
	[Export] private Button _submitButton = null!;
	[Export] private Timer _gameTimer = null!;
	[Export] private LineEdit _slot0 = null!;
	[Export] private LineEdit _slot1 = null!;
	[Export] private LineEdit _slot2 = null!;
	[Export] private LineEdit _slot3 = null!;

	private LineEdit[] _inputSlots = null!;

	private const int Slots = 4;
	private const int MaxGuesses = 5;
	private const int TimeLimit = 60;

	private static readonly Color CCorrect = new(0.18f, 0.62f, 0.35f);
	private static readonly Color CPresent = new(0.79f, 0.63f, 0.14f);
	private static readonly Color CAbsent = new(0.24f, 0.24f, 0.27f);
	private static readonly Color CEmpty = new(0.18f, 0.18f, 0.21f);
	private static readonly Color CWhite = new(1f, 1f, 1f);

	private (StyleBoxFlat Style, Label Lbl)[,] _cells = null!;

	private int[] _secret = Array.Empty<int>();
	private int _guessCount = 0;
	private int _timeLeft = TimeLimit;
	private bool _gameOver = false;

	public override void _Ready()
	{
		_inputSlots = new LineEdit[] { _slot0, _slot1, _slot2, _slot3 };

		for (int i = 0; i < Slots; i++)
		{
			int captured = i;
			_inputSlots[i].TextChanged += (text) => OnSlotChanged(text, captured);
			_inputSlots[i].GuiInput += (e) => OnSlotInput(e, captured);
		}

		_submitButton.Pressed += OnSubmit;

		_gameTimer.WaitTime = 1.0;
		_gameTimer.OneShot  = false;
		_gameTimer.Timeout  += OnTick;

		BuildGrid();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationVisibilityChanged && Visible)
			CallDeferred(nameof(StartGameSafe));
	}

	private void StartGameSafe()
	{
		if (!IsInsideTree()) return;
		NewGame();
	}

	private void BuildGrid()
	{
		foreach (Node child in _guessGrid.GetChildren())
			child.QueueFree();

		_cells = new (StyleBoxFlat, Label)[MaxGuesses, Slots];

		for (int row = 0; row < MaxGuesses; row++)
		{
			for (int col = 0; col < Slots; col++)
			{
				var style = new StyleBoxFlat
				{
					BgColor = CEmpty,
					CornerRadiusTopLeft = 6,
					CornerRadiusTopRight = 6,
					CornerRadiusBottomLeft = 6,
					CornerRadiusBottomRight = 6
				};

				var panel = new PanelContainer
				{
					CustomMinimumSize = new Vector2(100, 60)
				};
				panel.AddThemeStyleboxOverride("panel", style);

				var lbl = new Label
				{
					Text = "",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
					AutowrapMode = TextServer.AutowrapMode.Off
				};
				lbl.AddThemeFontOverride("font", font);
				lbl.AddThemeFontSizeOverride("font_size", 28);
				lbl.AddThemeColorOverride("font_color", CWhite);

				panel.AddChild(lbl);
				_guessGrid.AddChild(panel);

				_cells[row, col] = (style, lbl);
			}
		}
	}

	private void NewGame()
	{
		var pool = Enumerable.Range(0, 10).ToList();
		var rng = new RandomNumberGenerator();
		rng.Randomize();

		_secret = new int[Slots];
		for (int i = 0; i < Slots; i++)
		{
			int idx = rng.RandiRange(0, pool.Count - 1);
			_secret[i] = pool[idx];
			pool.RemoveAt(idx);
		}

		_guessCount = 0;
		_timeLeft = TimeLimit;
		_gameOver = false;

		for (int r = 0; r < MaxGuesses; r++)
			for (int c = 0; c < Slots; c++)
				SetCell(r, c, "", CEmpty);

		ClearInput();
		SetInputEnabled(true);

		_messageLabel.Text = "Guess the 4-digit code — no repeats!";
		UpdateTimerLabel();

		_gameTimer.Start();
	}

	private void OnTick()
	{
		if (_gameOver) return;

		_timeLeft--;
		UpdateTimerLabel();

		if (_timeLeft <= 0)
			EndGame(false, $"Time's up! Secret was: {SecretString()}");
	}

	private void UpdateTimerLabel()
	{
		_timerLabel.Text = $"Time: {_timeLeft}";
		_timerLabel.AddThemeColorOverride("font_color",
			_timeLeft <= 5 ? new Color(0.9f, 0.2f, 0.2f) : CWhite);
	}

	private void OnSubmit()
	{
		if (_gameOver) return;

		var guess = new int[Slots];
		for (int i = 0; i < Slots; i++)
		{
			string text = _inputSlots[i].Text.Trim();
			if (text.Length == 0 || !int.TryParse(text, out int digit) || digit < 0 || digit > 9)
			{
				_messageLabel.Text = "Fill all 4 slots with a digit (0–9)!";
				_invalid.Play();
				return;
			}
			guess[i] = digit;
		}

		if (guess.Distinct().Count() != Slots)
		{
			_messageLabel.Text = "No repeating digits allowed!";
			_invalid.Play();
			return;
		}

		int[] result = Score(guess);
		DisplayGuess(guess, result);
		_guessCount++;
		ClearInput();

		if (result.All(r => r == 2))
		{
			EndGame(true, $"Correct in {_guessCount} guess{(_guessCount == 1 ? "" : "es")}!");
		}
		else if (_guessCount >= MaxGuesses)
		{
			EndGame(false, $"No guesses left! Code was: {SecretString()}");
		}
		else
		{
			_messageLabel.Text = $"Guess {_guessCount}/{MaxGuesses} — keep going!";
		}
	}

	private int[] Score(int[] guess)
	{
		var result = new int[Slots];
		for (int i = 0; i < Slots; i++)
		{
			if (guess[i] == _secret[i])
				result[i] = 2;
			else if (_secret.Contains(guess[i]))
				result[i] = 1;
		}
		return result;
	}

	private void DisplayGuess(int[] guess, int[] result)
	{
		for (int col = 0; col < Slots; col++)
		{
			Color bg = result[col] switch
			{
				2 => CCorrect,
				1 => CPresent,
				_ => CAbsent
			};
			SetCell(_guessCount, col, guess[col].ToString(), bg);
		}
	}

	private void EndGame(bool won, string message)
	{
		if (won)
		{
			_messageLabel.Text = message;
			_messageLabel.AddThemeColorOverride("font_color", new Color(0.2f, 1f, 0.4f));
			_messageLabel.Visible = true;
			_success.Play();
		}
		else
		{
			_messageLabel.Text = message;
			_messageLabel.Visible = true;
			GlobalHealth.Instance.Drain(GlobalHealthPenalty);
			_fail.Play();
		}

		_gameOver = true;
		_gameTimer.Stop();
		SetInputEnabled(false);
		if (won)
		{
			EmitSignal(SignalName.GameWon);
		}	
		else
		{
			EmitSignal(SignalName.GameLost);
		}
	}

	private void SetCell(int row, int col, string text, Color bg)
	{
		_cells[row, col].Style.BgColor = bg;
		_cells[row, col].Lbl.Text = text;
	}

	private void OnSlotChanged(string newText, int slotIdx)
	{
		string filtered = "";
		foreach (char ch in newText)
		{
			if (char.IsDigit(ch)) { filtered = ch.ToString(); break; }
		}

		if (_inputSlots[slotIdx].Text != filtered)
		{
			_inputSlots[slotIdx].Text = filtered;
			_inputSlots[slotIdx].CaretColumn = filtered.Length;
		}

		if (filtered.Length == 1 && slotIdx < Slots - 1)
			_inputSlots[slotIdx + 1].GrabFocus();
	}

	private void OnSlotInput(InputEvent e, int slotIdx)
	{
		if (e is not InputEventKey key || !key.Pressed) return;

		if (key.Keycode == Key.Backspace)
		{
			_inputSlots[slotIdx].Text = "";
			if (slotIdx > 0)
			{
				_inputSlots[slotIdx - 1].GrabFocus();
				_inputSlots[slotIdx - 1].Text = "";
			}
			GetViewport().SetInputAsHandled();
		}
		else if (key.Keycode == Key.Enter || key.Keycode == Key.KpEnter)
		{
			OnSubmit();
			GetViewport().SetInputAsHandled();
		}
	}

	private void ClearInput()
	{
		foreach (var slot in _inputSlots)
			slot.Text = "";
		_inputSlots[0].GrabFocus();
	}

	private void SetInputEnabled(bool enabled)
	{
		foreach (var slot in _inputSlots)
			slot.Editable = enabled;
		_submitButton.Disabled = !enabled;
	}

	private string SecretString() => string.Join("", _secret);
}
