using Godot;
using System;

public partial class MainMenuHandler : Control {
	private enum MainMenuScreen { Intro, Menu };

	[Export] private Control IntroScreen;
	[Export] private Control MenuScreen;

	[Signal] public delegate void LeaveMainMenuEventHandler();

	private MainMenuScreen currentScreen = MainMenuScreen.Intro;

	public override void _Ready() {
		// setup intro screen
		IntroScreen.GetNode<Timer>("FlickerTimer").Connect("timeout", Callable.From(() => OnFlicker()));
		MenuScreen.Visible = false;
		IntroScreen.Visible = true;

		// set up menu screen
		MenuScreen.GetNode<AnimationPlayer>("Fade").Connect("animation_finished", Callable.From((StringName name) => OnMenuFadeIn()));
		MenuScreen.GetNode<Button>("Play").Connect("button_up", Callable.From(() => EmitSignal(SignalName.LeaveMainMenu)));
		MenuScreen.GetNode<Button>("Quit").Connect("button_up", Callable.From(() => GetTree().Quit()));
	}

	public override void _Input(InputEvent inputEvent) {
		if (currentScreen == MainMenuScreen.Intro) {
			// handle intro screen
			if (inputEvent is InputEventMouseButton || inputEvent is InputEventKey) {
				IntroScreen.GetNode<Timer>("FlickerTimer").Stop();
				IntroScreen.Visible = false;
				currentScreen = MainMenuScreen.Menu;
				MenuScreen.GetNode<AnimationPlayer>("Fade").Play("FadeIn");
				MenuScreen.Visible = true;
			}
		}
	}

	private void OnMenuFadeIn() {
		Button playButton = MenuScreen.GetNode<Button>("Play");
		Button quitButton = MenuScreen.GetNode<Button>("Quit");
		playButton.Disabled = false;
		quitButton.Disabled = false;
		playButton.Visible = true;
		quitButton.Visible = true;
	}

	private void OnFlicker() {
		Label startMessage =  IntroScreen.GetNode<Label>("StartMessage");
		startMessage.Visible = !startMessage.Visible;
	}
}
