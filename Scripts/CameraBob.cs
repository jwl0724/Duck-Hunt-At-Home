using Godot;
using System;

public partial class CameraBob : Camera3D {
	// exported variables
	[Export] public Player Player;
	[Export] public Hitscan HitscanLine;
	[Export] public Node3D BulletTrail;
	[Export] public AnimationPlayer Animator;

	// static variables
	private static readonly float headbobAngle = Mathf.DegToRad(0.3f);
	private static readonly float maxFallingAngle = Mathf.DegToRad(5);
	private static readonly float bobSpeed = 10;

	// instance variables
	private float currentAngle = headbobAngle;

	public override void _Ready() {
		Player.Connect("PlayerDamaged", Callable.From(() => Animator.Play("damaged")));
	}

	public override void _Process(double delta) {
		if (!Player.Velocity.IsZeroApprox() && Player.IsOnFloor()) {
			// moving on the ground
			if (Rotation.X > headbobAngle + Mathf.DegToRad(0.1f)) {
				// restore original rotation after falling
				Rotation = Rotation.Lerp(new Vector3(), bobSpeed * (float) delta);
				return;
			}

			if (!IsBetween(Rotation.X, -headbobAngle, headbobAngle) && Rotation.X < 0)
				currentAngle = headbobAngle;
			else if (!IsBetween(Rotation.X, -headbobAngle, headbobAngle) && Rotation.X > 0) 
				currentAngle = -headbobAngle;
			Rotation = Rotation.Lerp(new Vector3(Rotation.X + currentAngle, Rotation.Y, Rotation.Z), bobSpeed * (float) delta);

		} else if (!Player.IsOnFloor()) {
			// falling
			Rotation = Rotation.Lerp(new Vector3(Rotation.X + maxFallingAngle, Rotation.Y, Rotation.Z), bobSpeed / 10 * (float) delta);

		} else {
			// not moving
			Rotation = Rotation.Lerp(new Vector3(), bobSpeed * (float) delta);
		}
		// adjust rotation of hitscan line so it matches reticle
		HitscanLine.Rotation = HitscanLine.Rotation.Lerp(Rotation, bobSpeed * (float) delta);
		BulletTrail.Rotation = new Vector3(0, 0, -Rotation.X);
	}

	private static bool IsBetween(float number, float min, float max) {
		return min <= number && number <= max;
	}
}
