using Godot;
using System;

public partial class GunVisual : Node3D {
	// exported variables
	[Export] public Player Player;
	[Export] public AnimationPlayer Animator;

	// instance variables	
	public override void _Ready() {
		Player.Connect("PlayerShoot", Callable.From(() => OnPlayerShoot()));
		Player.Connect("PlayerReload", Callable.From(() => OnPlayerReload()));
	}

	public override void _Process(double delta) {
	}

	private void OnPlayerReload() {
		Animator.Play("reload");
	}

	private void OnPlayerShoot() {
		Animator.Play("fire");
	}
}
