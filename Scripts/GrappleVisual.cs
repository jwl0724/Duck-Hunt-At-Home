using Godot;
using System;

public partial class GrappleVisual : Node3D {
	// exported variables
	[Export] public Player Player;
	[Export] public AnimationPlayer Animator;

	[Signal] public delegate void GrappleGunLiftedEventHandler();

	public override void _Ready() {
		Visible = false; // hide grapple when first loading in
		Player.Connect("PlayerGrapple", Callable.From(() => OnPlayerGrapple()));
		Animator.Connect("animation_finished", Callable.From((StringName name) => OnAnimationFinished(name)));
		Animator.Play("holster");
	}

	private void OnPlayerGrapple() {
		if (!Visible) Visible = true;
		if (Player.Grappled) {
			Animator.Play("lift");
		} else Animator.Play("holster");
	}

	private void OnAnimationFinished(StringName animationName) {
		if (animationName == "lift") EmitSignal(SignalName.GrappleGunLifted);
	}
}
