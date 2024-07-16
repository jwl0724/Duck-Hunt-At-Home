using Godot;
using System;
using System.Collections.Generic;

public partial class LevelSelectHandler : Control {
	public enum Level { City, Plains, Space };

	[Signal] public delegate void SelectedLevelEventHandler();
	[Signal] public delegate void BackToMainMenuEventHandler();

	// order of difficulties must correspond with enum order
	private string[] levelDifficulties = {"Easy", "Medium", "Hard"};
	private string[] levelNames = {"Duck City", "Misty Plains", "Neon Space"};
	private int levelIndex = 0;
	private int totalLevels = Enum.GetNames(typeof(Level)).Length;

	public override void _Ready() {
		// connect buttons
		GetNode<Button>("LeftArrow").Connect("button_up", Callable.From(() => CycleLevel(-1)));
		GetNode<Button>("RightArrow").Connect("button_up", Callable.From(() => CycleLevel(1)));
		GetNode<Button>("PlayLevelButton").Connect("button_up", Callable.From(() => EmitSignal(SignalName.SelectedLevel, levelIndex)));
		GetNode<Button>("BackButton").Connect("button_up", Callable.From(() => EmitSignal(SignalName.BackToMainMenu)));

		CycleLevel(0); // load an image on first load up
	}

	private void CycleLevel(int indexShift) {
		levelIndex += indexShift;
		if (levelIndex < 0) levelIndex = totalLevels - 1;
		else if (levelIndex >= totalLevels) levelIndex = 0;

		GetNode<LevelPreviewHandler>("LevelPreview").DisplayLevel(
			Enum.GetName(typeof(Level), levelIndex), levelNames[levelIndex], levelDifficulties[levelIndex]
		);
	}
}
