using Godot;
using System;
using System.Diagnostics;

public partial class MenuHandler : Control {
	public enum MenuScreen { MainMenu, LevelSelect }
	
	[Export] Control MainMenuScreen;
	[Export] Control LevelSelectScreen;

	[Signal] public delegate void ScreenChangeEventHandler();

	public MenuScreen CurrentScreen { get; private set; } = MenuScreen.MainMenu;

	public override void _Ready() {
		MainMenuScreen.Connect("LeaveMainMenu", Callable.From(() => TransitionScreen(MenuScreen.LevelSelect)));
		LevelSelectScreen.Connect("SelectedLevel", Callable.From((int levelIndex) => StartGame(levelIndex)));
		LevelSelectScreen.Connect("BackToMainMenu", Callable.From(() => TransitionScreen(MenuScreen.MainMenu)));
		TransitionScreen(MenuScreen.MainMenu);
	}

	private void StartGame(int levelIndex) {
		if (levelIndex == 0) {
			GetTree().ChangeSceneToFile("res://Scenes/Screens/CityLevel.tscn");

		} else if (levelIndex == 1) {
			GetTree().ChangeSceneToFile("res://Scenes/Screens/PlainsLevel.tscn");

		} else if (levelIndex == 2) {
			GetTree().ChangeSceneToFile("res://Scenes/Screens/SpaceLevel.tscn");

		}
	}

	private void TransitionScreen(MenuScreen screen) {
		CurrentScreen = screen;
		foreach (Control menu in GetChildren()) {
			if (menu.Name == Enum.GetName(CurrentScreen)) menu.Visible = true;
			else menu.Visible = false;
		}
	}
}
