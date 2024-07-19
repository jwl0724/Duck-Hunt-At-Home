using Godot;
using System;

public partial class ShootVisualEffects : Node3D {
	// exported variables
	[Export] public Player Player;

	// static variables
	private GpuParticles3D muzzleFlash;

	// timers
	private float muzzleFlashTimer = 0;

	public override void _Ready() {
		muzzleFlash = GetNode<GpuParticles3D>("MuzzleFlash");
		muzzleFlash.Emitting = false;
		Player.InputManager.Connect("PlayerShoot", Callable.From(() => OnPlayerShoot()));
	}

	async private void OnPlayerShoot() {
		muzzleFlash.Emitting = true;
		await ToSignal(GetTree().CreateTimer(0.15), Timer.SignalName.Timeout);
		muzzleFlash.Emitting = false;
	}
}
