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
	private CompressedTexture2D cityImage = ResourceLoader.Load<CompressedTexture2D>("res://Assets/Images/LevelPreviews/CityLevelPreview.png");
	private CompressedTexture2D plainsImage = ResourceLoader.Load<CompressedTexture2D>("res://Assets/Images/LevelPreviews/PlainsLevelPreview.png");
	private CompressedTexture2D spaceImage = ResourceLoader.Load<CompressedTexture2D>("res://Assets/Images/LevelPreviews/SpaceLevelPreview.png");

	public override void _Ready() {
		// connect buttons
		GetNode<Button>("LeftArrow").Connect("button_up", Callable.From(() => CycleLevel(-1)));
		GetNode<Button>("RightArrow").Connect("button_up", Callable.From(() => CycleLevel(1)));
		GetNode<Button>("PlayLevelButton").Connect("button_up", Callable.From(() => OnSelectedLevel()));
		GetNode<Button>("BackButton").Connect("button_up", Callable.From(() => OnBack()));
		GetNode<Control>("LoadingScreen").Connect("draw", Callable.From(() => OnLoadingScreenDraw()));

		// load an image on first load up
		GetNode<LevelPreviewHandler>("LevelPreview").DisplayLevel(
			Enum.GetName(typeof(Level), levelIndex), levelNames[levelIndex], levelDifficulties[levelIndex]
		);
	}

	private void OnBack() {
		(GetParent() as MenuHandler).SoundCollection.Play("Click", overlapMusic: true);
		EmitSignal(SignalName.BackToMainMenu);
	}

	private void OnSelectedLevel() {
		(GetParent() as MenuHandler).SoundCollection.Play("Click", overlapMusic: true);
		Control loadingScreen = GetNode<Control>("LoadingScreen");
		TextureRect loadImage = loadingScreen.GetNode<TextureRect>("LevelImage");
		
		if (levelIndex == 0) loadImage.Texture = cityImage;
		else if (levelIndex == 1) loadImage.Texture = plainsImage;
		else if (levelIndex == 2) loadImage.Texture = spaceImage;
		
		loadingScreen.Visible = true;
	}

	async private void OnLoadingScreenDraw() {
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); // wait for frame before draw frame to finish
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); // wait for draw frame to finish
		EmitSignal(SignalName.SelectedLevel, levelIndex);
	}

	private void CycleLevel(int indexShift) {
		levelIndex += indexShift;
		if (levelIndex < 0) levelIndex = totalLevels - 1;
		else if (levelIndex >= totalLevels) levelIndex = 0;
		
		(GetParent() as MenuHandler).SoundCollection.Play("Click", overlapMusic: true);
		GetNode<LevelPreviewHandler>("LevelPreview").DisplayLevel(
			Enum.GetName(typeof(Level), levelIndex), levelNames[levelIndex], levelDifficulties[levelIndex]
		);
	}
}
