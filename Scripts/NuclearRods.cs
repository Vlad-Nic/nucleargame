using Godot;
using System;
using System.Collections.Generic;

public partial class NuclearRods : Control
{
	//Constants
	private const int   RodCount  = 5;
	private const int   TimeLimit = 30;

	// Rod sizes (width x height) — XS to XL
	private static readonly Vector2[] RodSizes = new Vector2[]
	{
		new(14, 40),   // size 0 — smallest
		new(18, 54),  // size 1
		new(22, 68),  // size 2
		new(26, 82),  // size 3
		new(30, 96),  // size 4 — largest
	};

	// Box sizes — slightly larger than rod so it looks like a slot
	private static readonly Vector2[] BoxSizes = new Vector2[]
	{
		new(26,  52),
		new(30,  66),
		new(34,  80),
		new(38,  94),
		new(42, 108),
	};

	// Colour per size index (eerie nuclear greens / yellows)
	private static readonly Color[] RodColors = new Color[]
	{
		new(0.20f, 0.90f, 0.30f),
		new(0.60f, 0.95f, 0.10f),
		new(0.95f, 0.85f, 0.05f),
		new(0.95f, 0.55f, 0.05f),
		new(0.90f, 0.20f, 0.20f),
	};

	//Node refs
	[Export] private Label   _timerLabel   = null!;
	[Export] private Label   _messageLabel = null!;
	[Export] private Label   _instructionLabel = null!;
	[Export] private Control _rodContainer = null!;
	[Export] private Control _boxContainer = null!;
	[Export] private Timer   _gameTimer    = null!;

	//Runtime data
	private record RodData(Panel Visual, Vector2 HomePosition, int SizeIndex, bool Placed = false);
	private record BoxData(Panel Visual, Vector2 Centre, int SizeIndex, bool Filled);

	private RodData[] _rods = null!;
	private BoxData[] _boxes= null!;

	// Drag state
	private int     _draggedRod    = -1;
	private Vector2 _dragOffset    = Vector2.Zero;

	//Game state
	private int  _timeLeft  = TimeLimit;
	private bool _gameOver  = false;
	private int  _placed    = 0;

	//Lifecycle
	public override void _Ready()
	{
		_gameTimer.Timeout     += OnTick;
		_gameTimer.WaitTime     = 1.0;
		_gameTimer.OneShot      = false;

		StartGame();
	}

	//Build/reset
	private void StartGame()
	{
		_gameOver = false;
		_placed   = 0;
		_draggedRod = -1;
		_timeLeft = TimeLimit;

		_messageLabel.Text      = "";
		_timerLabel.Text        = $"Time: {_timeLeft}";
		_timerLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));

		// Clear old visuals
		foreach (Node c in _rodContainer.GetChildren()) c.QueueFree();
		foreach (Node c in _boxContainer.GetChildren())  c.QueueFree();

		// Shuffle size indices so rods appear in random order on screen
		int[] order = ShuffledIndices(RodCount);

		//Build rods
		// Spread rods evenly across RodContainer width (480px)
		float rodAreaWidth = _rodContainer.Size.X > 0 ? _rodContainer.Size.X : 480f;
		float rodAreaHeight= _rodContainer.Size.Y > 0 ? _rodContainer.Size.Y : 300f;
		float step = rodAreaWidth / RodCount;

		_rods = new RodData[RodCount];
		for (int i = 0; i < RodCount; i++)
		{
			int sizeIdx = order[i];
			Vector2 size = RodSizes[sizeIdx];

			float cx = step * i + step * 0.5f;
			float cy = rodAreaHeight * 0.5f;
			Vector2 pos = new Vector2(cx - size.X * 0.5f, cy - size.Y * 0.5f);

			var panel = MakeRod(size, RodColors[sizeIdx], sizeIdx);
			panel.Position = pos;
			_rodContainer.AddChild(panel);

			_rods[i] = new RodData(panel, pos, sizeIdx);
		}

		//Build boxes
		// Boxes are always in size order so the player has to figure it out
		float boxAreaWidth  = _boxContainer.Size.X > 0 ? _boxContainer.Size.X : 480f;
		float boxAreaHeight = _boxContainer.Size.Y > 0 ? _boxContainer.Size.Y : 230f;
		float bstep = boxAreaWidth / RodCount;

		_boxes = new BoxData[RodCount];
		int[] boxOrder = ShuffledIndices(RodCount);
		for (int i = 0; i < RodCount; i++)
		{
			int sizeIdx = boxOrder[i];
			Vector2 size = BoxSizes[sizeIdx];

			float cx = bstep * i + bstep * 0.5f;
			float cy = boxAreaHeight * 0.5f;
			Vector2 pos = new Vector2(cx - size.X * 0.5f, cy - size.Y * 0.5f);

			var panel = MakeBox(size);
			panel.Position = pos;
			_boxContainer.AddChild(panel);

			Vector2 centre = new Vector2(cx, cy);
			_boxes[i] = new BoxData(panel, centre, sizeIdx, false);
		}

		_gameTimer.Start();
	}

	//Input
	public override void _Input(InputEvent evt)
	{
		if (_gameOver) return;

		if (evt is InputEventMouseButton mb)
		{
			if (mb.ButtonIndex == MouseButton.Left)
			{
				if (mb.Pressed)  TryBeginDrag(mb.GlobalPosition);
				else             TryDrop(mb.GlobalPosition);
			}
		}
		else if (evt is InputEventMouseMotion mm)
		{
			if (_draggedRod >= 0)
			{
				var rod = _rods[_draggedRod];
				// Convert global mouse to RodContainer local, then apply offset
				Vector2 localMouse = _rodContainer.GetLocalMousePosition();
				rod.Visual.Position = localMouse - _dragOffset;
			}
		}
	}

	private void TryBeginDrag(Vector2 globalPos)
	{
		for (int i = 0; i < RodCount; i++)
		{
			var rod = _rods[i];
			if (rod.Visual == null) continue;
			if (rod.Placed) continue; // already locked in a box

			// Convert to RodContainer local space
			Vector2 local = _rodContainer.GetLocalMousePosition();
			Rect2 rect = new Rect2(rod.Visual.Position, rod.Visual.Size);
			if (rect.HasPoint(local))
			{
				_draggedRod = i;
				_dragOffset = local - rod.Visual.Position;
				rod.Visual.ZIndex = 10;
				return;
			}
		}
	}

	private void TryDrop(Vector2 globalPos)
	{
		if (_draggedRod < 0) return;

		var rod = _rods[_draggedRod];
		rod.Visual.ZIndex = 0;

		// Find the rod's centre in BoxContainer local space
		Vector2 rodCentreGlobal = rod.Visual.GlobalPosition + rod.Visual.Size * 0.5f;
		Vector2 rodCentreLocal  = rodCentreGlobal - _boxContainer.GlobalPosition;

		// Check overlap with each unfilled box
		int hitBox = -1;
		float bestDist = float.MaxValue;
		for (int b = 0; b < RodCount; b++)
		{
			if (_boxes[b].Filled) continue;
			Rect2 boxRect = new Rect2(_boxes[b].Visual.Position, _boxes[b].Visual.Size);
			if (boxRect.HasPoint(rodCentreLocal))
			{
				float d = rodCentreLocal.DistanceTo(_boxes[b].Centre);
				if (d < bestDist) { bestDist = d; hitBox = b; }
			}
		}

		if (hitBox >= 0)
		{
			if (_boxes[hitBox].SizeIndex == rod.SizeIndex)
			{
				//Correct — snap rod into box
				SnapRodToBox(_draggedRod, hitBox);
			}
			else
			{
				// ❌ Wrong box — instant game over
				rod.Visual.Position = rod.HomePosition; // snap back first so it's visible
				EndGame(false);
			}
		}
		else
		{
			// Dropped in empty space — return home
			rod.Visual.Position = rod.HomePosition;
		}

		_draggedRod = -1;
	}

	private void SnapRodToBox(int rodIdx, int boxIdx)
	{
		var rod = _rods[rodIdx];
		var box = _boxes[boxIdx];

		// Calculate the snap position in global space BEFORE reparenting
		// so the parent change doesn't affect the maths
		Vector2 boxGlobalCentre = _boxContainer.GlobalPosition + box.Visual.Position + box.Visual.Size * 0.5f;
		Vector2 snapGlobal      = boxGlobalCentre - rod.Visual.Size * 0.5f;

		// Reparent rod from RodContainer → BoxContainer, then convert snap to local space
		rod.Visual.Reparent(_boxContainer);
		rod.Visual.Position = snapGlobal - _boxContainer.GlobalPosition;
		rod.Visual.ZIndex   = 1;

		// Mark rod as placed so it can't be dragged again
		_rods[rodIdx] = rod with { Placed = true };

		// Mark box as filled
		_boxes[boxIdx] = box with { Filled = true };

		_placed++;
		if (_placed == RodCount)
			EndGame(true);
	}

	//Timer
	private void OnTick()
	{
		if (_gameOver) return;
		_timeLeft--;
		_timerLabel.Text = $"Time: {_timeLeft}";
		_timerLabel.AddThemeColorOverride("font_color",
			_timeLeft <= 8 ? new Color(0.9f, 0.2f, 0.2f) : new Color(1, 1, 1));
		if (_timeLeft <= 0)
			EndGame(false);
	}

	//End game
	private void EndGame(bool won)
	{
		_gameOver = true;
		_gameTimer.Stop();
		_draggedRod = -1;

		if (won)
		{
			_messageLabel.Text = "All rods stored safely!";
			_messageLabel.AddThemeColorOverride("font_color", new Color(0.2f, 1f, 0.3f));
		}
		else if (_timeLeft <= 0)
		{
			_messageLabel.Text = "Meltdown! Time ran out!";
			_messageLabel.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0.2f));
		}
		else
		{
			_messageLabel.Text = "Wrong slot! Reactor breach!";
			_messageLabel.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0.2f));
		}
	}

	//Visual factories
	private static Panel MakeRod(Vector2 size, Color color, int sizeIdx)
	{
		var style = new StyleBoxFlat
		{
			BgColor                 = color,
			CornerRadiusTopLeft     = 4,
			CornerRadiusTopRight    = 4,
			CornerRadiusBottomLeft  = 4,
			CornerRadiusBottomRight = 4,
			BorderColor             = new Color(1, 1, 1, 0.3f),
			BorderWidthTop          = 1,
			BorderWidthBottom       = 1,
			BorderWidthLeft         = 1,
			BorderWidthRight        = 1,
		};

		var panel = new Panel { CustomMinimumSize = size, Size = size };
		panel.AddThemeStyleboxOverride("panel", style);

		// Size label
		var lbl = new Label
		{
			Text                = (sizeIdx + 1).ToString(),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment   = VerticalAlignment.Center,
			AnchorRight         = 1f,
			AnchorBottom        = 1f,
			OffsetRight         = 0,
			OffsetBottom        = 0,
		};
		lbl.AddThemeFontSizeOverride("font_size", 14);
		lbl.AddThemeColorOverride("font_color", new Color(0, 0, 0, 0.8f));
		panel.AddChild(lbl);

		return panel;
	}

	private static Panel MakeBox(Vector2 size)
	{
		var style = new StyleBoxFlat
		{
			BgColor                 = new Color(0.12f, 0.15f, 0.12f),
			CornerRadiusTopLeft     = 4,
			CornerRadiusTopRight    = 4,
			CornerRadiusBottomLeft  = 4,
			CornerRadiusBottomRight = 4,
			BorderColor             = new Color(0.25f, 0.45f, 0.28f),
			BorderWidthTop          = 2,
			BorderWidthBottom       = 2,
			BorderWidthLeft         = 2,
			BorderWidthRight        = 2,
		};

		var panel = new Panel { CustomMinimumSize = size, Size = size };
		panel.AddThemeStyleboxOverride("panel", style);

		return panel;
	}

	//Helpers
	private static int[] ShuffledIndices(int count)
	{
		var arr = new int[count];
		for (int i = 0; i < count; i++) arr[i] = i;
		var rng = new RandomNumberGenerator();
		rng.Randomize();
		for (int i = count - 1; i > 0; i--)
		{
			int j = rng.RandiRange(0, i);
			(arr[i], arr[j]) = (arr[j], arr[i]);
		}
		return arr;
	}
}
