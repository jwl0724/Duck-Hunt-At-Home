using Godot;
using System;

public partial class ShootVisualEffects : Node3D {
	// exported variables
	[Export] public Player Player;
	[Export] public MeshInstance3D MuzzleFlashModel;

	// static variables
	private static readonly float muzzleFlashDuration = 0.1f;

	// timers
	private float muzzleFlashTimer = 0;

	public override void _Ready() {
		MuzzleFlashModel.Visible = false;
		Player.InputManager.Connect("PlayerShoot", Callable.From(() => OnPlayerShoot()));
	}

	public override void _Process(double delta) {
		if (MuzzleFlashModel.Visible && muzzleFlashTimer >= muzzleFlashDuration) {
			muzzleFlashTimer = 0;
			MuzzleFlashModel.Visible = false;
		} else if (MuzzleFlashModel.Visible) muzzleFlashTimer += (float) delta;
	}

	private void OnPlayerShoot() {
		MuzzleFlashModel.Visible = true;
	}
}
