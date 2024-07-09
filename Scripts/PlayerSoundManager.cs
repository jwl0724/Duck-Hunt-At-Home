using Godot;
using System;

public partial class PlayerSoundManager : Node3D {
	// exported variables
	[Export] public Player Player;
	[Export] public Hitscan HitscanLine;
	[Export] public SoundCollection SoundCollection;

	public override void _Ready() {
		// connect signals
		Player.InputManager.Connect("PlayerGrapple", Callable.From(() => OnPlayerGrapple()));
		Player.Connect("PlayerDamaged", Callable.From(() => SoundCollection.Play("Damaged")));
		Player.InputManager.Connect("PlayerEmptyMag", Callable.From(() => SoundCollection.Play("EmptyMag")));
		Player.InputManager.Connect("PlayerReload", Callable.From(() => SoundCollection.Play("Reload")));
		Player.Connect("PlayerDied", Callable.From(() => SoundCollection.Play("Dead")));
		Player.InputManager.Connect("PlayerShoot", Callable.From(() => SoundCollection.Play("Shoot")));
		HitscanLine.Connect("ShotLanded", Callable.From(() => SoundCollection.Play("ShotLanded")));
	}

	private void OnPlayerGrapple() {
		if (!Player.Grappled) return;
		SoundCollection.Play("GrappleHook");
	}
}
