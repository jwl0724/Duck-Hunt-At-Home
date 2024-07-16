using Godot;
using System;

public partial class LevelPreviewHandler : Control {
	private Control previewPictures;
	
	public override void _Ready() {
		previewPictures = GetNode<Control>("PreviewPictures");
	}

	public void DisplayLevel(string level, string levelName, string difficulty) {
		foreach (TextureRect child in previewPictures.GetChildren()) {
			if (child.Name == $"{level}Preview") child.Visible = true;
			else child.Visible = false;
		}
		GetNode<Label>("LevelName").Text = levelName.ToUpper();
		GetNode<Label>("Difficulty").Text = $"Difficulty: {difficulty}";
	}
}
