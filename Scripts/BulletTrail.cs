using Godot;
using System;

public partial class BulletTrail : Node3D {
	// exported variables
	[Export] public AnimationPlayer Animator;

	public override void _Ready() {
		Animator.Connect("animation_finished", Callable.From((StringName animation) => QueueFree()));
		Animator.Play("shoot");
	}
}
