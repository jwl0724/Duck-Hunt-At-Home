using Godot;
using System;

public partial class PlayerMenuManager : Control {
	[Export] private Control pauseMenu;
	[Export] private Control gameOverMenu;

	[Signal] public delegate void RestartGameEventHandler();

	private Player player;

	public override void _Ready() {
		player = GetParent() as Player;
		player.Connect("PlayerDied", Callable.From(() => OnPlayerDeath()));

		SetUpPauseMenu();
		SetUpGameOverMenu();
	}

	public override void _Input(InputEvent inputEvent) {
		if (player.Health <= 0) return;

		// pause game with esc
		if (Input.IsActionJustPressed("pause")) {
			if (Input.MouseMode == Input.MouseModeEnum.Visible) PauseGame(false);
			else PauseGame(true);
		}
	}

	private void PauseGame(bool pause) {
		if (pause) Input.MouseMode = Input.MouseModeEnum.Visible;
		else Input.MouseMode = Input.MouseModeEnum.Captured;
		pauseMenu.Visible = pause;
		GetTree().Paused = pause;
	}

	private void SetUpPauseMenu() {
		// connect buttons
		pauseMenu.GetNode<Button>("ResumeButton").Connect("button_up", Callable.From(() => PauseGame(false)));
		pauseMenu.GetNode<Button>("RestartButton").Connect("button_up", Callable.From(() => ResetState()));
		pauseMenu.GetNode<Button>("MenuButton").Connect("button_up", Callable.From(() => BackToMenu()));
		pauseMenu.Visible = false;
	}

	private void SetUpGameOverMenu() {
		// connect buttons
		AnimationPlayer gameOverAnimator = gameOverMenu.GetNode<AnimationPlayer>("Fade");
		gameOverAnimator.Stop();
		gameOverAnimator.Connect("animation_finished", Callable.From((StringName name) => DisplayTexts(true)));
		gameOverMenu.GetNode<Button>("RetryButton").Connect("button_up", Callable.From(() => ResetState()));
		gameOverMenu.GetNode<Button>("MenuButton").Connect("button_up", Callable.From(() => BackToMenu()));
		DisplayTexts(false);
	}

	private void DisplayTexts(bool display) {
		foreach (var child in gameOverMenu.GetChildren()) {
			if (child is Label label) label.Visible = display;
			else if (child is Button button) button.Visible = display;
		}
	}

	private void ResetState() {
		SceneTree tree = GetTree();
		if (tree.Paused) tree.Paused = false;
		DisplayTexts(false);
		gameOverMenu.Visible = false;
		pauseMenu.Visible = false;
		EmitSignal(SignalName.RestartGame);
	}

	private void BackToMenu() {
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/Screens/Menu.tscn");
	}

	private void OnPlayerDeath() {
		player.HUD.Visible = false;
		gameOverMenu.GetNode<Label>("Score").Text = player.HUD.TimeAndScore.GetNode<Label>("ScoreLabel").Text;
		gameOverMenu.GetNode<Label>("Time").Text = player.HUD.TimeAndScore.GetNode<Label>("TimeLabel").Text;
		gameOverMenu.Visible = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;
		gameOverMenu.GetNode<AnimationPlayer>("Fade").Play("FadeIn");
	}
}
