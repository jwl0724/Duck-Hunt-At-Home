using Godot;
using System;

public partial class BulletTrail : MeshInstance3D {
	// exported variables
	[Export] public Player Player;

	// static variables
	private static readonly float trailDuration = 0.02f;

	// timers
	private float trailTimer = 0;

	public override void _Ready() {
		Visible = false;
		Player.Connect("PlayerShoot", Callable.From(() => OnPlayerShoot()));
	}

	public override void _Process(double delta) {
		if (Visible && trailTimer >= trailDuration) {
			trailTimer = 0;
			Visible = false;
		} else if (Visible) trailTimer += (float) delta;
	}

	private void OnPlayerShoot() {
		Visible = true;
	}
}
