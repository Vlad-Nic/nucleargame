using Godot;
using System;
using System.Threading.Tasks;

public partial class TransitionManager : Node
{
	private ColorRect colorRect;
	
	private ColorRect GetColorRect()
	{
		var currentScene = GetTree().CurrentScene;
		var cr = currentScene.FindChild("ColorRect", owned: false, recursive: true) as ColorRect;
		return cr;
	}
	
	public async Task FadeOutTransition()
	{
		var cr = GetColorRect();
		cr.Modulate = cr.Modulate with { A = 0.0f };
		var tween = GetTree().CreateTween();
		tween.TweenProperty(cr, "modulate:a", 1.0f, 1.0f);
		await ToSignal(tween, "finished");
	}
	
	public async Task FadeInTransition()
	{
		var cr = GetColorRect();
		cr.Modulate = cr.Modulate with { A = 1.0f };
		var tween = GetTree().CreateTween();
		tween.TweenProperty(cr, "modulate:a", 0.0f, 1.0f);
		await ToSignal(tween, "finished");
	}
}
