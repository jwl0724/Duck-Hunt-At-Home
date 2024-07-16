using Godot;
using System;

public partial class MainMenuBackgroundHandler : Node3D {
	private AnimationPlayer cameraMovement;
	private string[] animationList;
	public override void _Ready() {
		cameraMovement = GetNode<AnimationPlayer>("Animation");
		cameraMovement.Connect("animation_finished", Callable.From((StringName name) => OnFinishedAnimation(name)));
		animationList = cameraMovement.GetAnimationList();
		cameraMovement.Play(animationList[GD.Randi() % animationList.Length]);
	}

	private void OnFinishedAnimation(StringName name) {
		long randomIndex = GD.Randi() % animationList.Length;
		while (animationList[randomIndex] == name || animationList[randomIndex] == "RESET") {
			randomIndex = GD.Randi() % animationList.Length;
		}
		cameraMovement.Play(animationList[randomIndex]);
	}
}
