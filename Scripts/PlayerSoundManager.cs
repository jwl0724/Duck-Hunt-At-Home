using Godot;
using System;

public partial class PlayerSoundManager : Node3D {
	// exported variables
	[Export] public Player Player;
	[Export] public Hitscan HitscanLine;
	[Export] public SoundCollection SoundCollection;

	public override void _Ready() {
		// connect signals
		Player.Connect("PlayerGrapple", Callable.From(() => OnPlayerGrapple()));
		Player.Connect("PlayerDamaged", Callable.From(() => SoundCollection.Play("Damaged")));
		Player.Connect("PlayerEmptyMag", Callable.From(() => SoundCollection.Play("EmptyMag")));
		Player.Connect("PlayerReload", Callable.From(() => SoundCollection.Play("Reload")));
		Player.Connect("PlayerDied", Callable.From(() => SoundCollection.Play("Dead")));
		Player.Connect("PlayerShoot", Callable.From(() => SoundCollection.Play("Shoot")));
		HitscanLine.Connect("ShotLanded", Callable.From(() => SoundCollection.Play("ShotLanded")));
	}

	private void OnPlayerGrapple() {
		if (!Player.Grappled) return; // player sets grapple state before emitting signal
		SoundCollection.Play("GrappleHook");
	}
}
