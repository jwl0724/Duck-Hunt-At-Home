using Godot;
using System;

public partial class StartUpHandler : Control {
	public override void _Ready() {
		AnimationPlayer fade = GetNode<AnimationPlayer>("Fade");
		fade.Connect("animation_finished", Callable.From((StringName name) => OnFadeOut()));
		fade.Play("LogoIntro");
	}

	private void OnFadeOut() {
		GetTree().ChangeSceneToFile("res://Scenes/Screens/Menu.tscn");
	}
}
