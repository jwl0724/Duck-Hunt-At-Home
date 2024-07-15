using Godot;
using System;

public partial class MenuHandler : Control {
	public enum MenuScreen { BootUp, MainMenu, LevelSelect, InGame }
	
	[Export] Control BootUpScreen;
	[Export] Control MainMenuScreen;
	[Export] Control LevelSelectScreen;

	public MenuScreen CurrentScreen { get; private set; } = MenuScreen.BootUp;

	public override void _Ready() {
		RunBootUp();
	}

	public override void _Process(double delta) {
	}

	public void TransitionScreen(MenuScreen screen) {
		CurrentScreen = screen;
		foreach (Control menu in GetChildren()) {
			if (menu.Name == Enum.GetName(CurrentScreen)) menu.Visible = true;
			else menu.Visible = false;
		}
	}

	private void RunBootUp() {
		TransitionScreen(MenuScreen.BootUp);
		AnimationPlayer animator = BootUpScreen.GetNode<AnimationPlayer>("Animation");
		animator.Connect("animation_finished", Callable.From((StringName name) => RunMainMenu()));
		animator.Play("LogoIntro");
	}

	private void RunMainMenu() {
		TransitionScreen(MenuScreen.MainMenu);
		GD.Print("Main menu here");
	}

	private void RunLevelSelect() {
		TransitionScreen(MenuScreen.LevelSelect);
	}
}
