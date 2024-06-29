using Godot;
using System;

public partial class CameraBob : Camera3D {
	// exported variables
	[Export] public Player Player;
	[Export] public Node3D RotationalHelper;

	// static variables
	private static readonly float headbobAngle = Mathf.DegToRad(2);
	private static readonly float maxFallingAngle = Mathf.DegToRad(5);
	private static readonly float bobSpeed = 10;
	
	public override void _Ready() {

	}

	public override void _Process(double delta) {
		Vector3 parentRotation = RotationalHelper.Rotation;
		if (!Player.Velocity.IsZeroApprox() && Player.IsOnFloor()) {
			// moving on the ground
			if (Rotation.X <= parentRotation.X - headbobAngle) {
				Rotation = Rotation.Lerp(
					new Vector3(parentRotation.X + headbobAngle, parentRotation.Y, parentRotation.Z), 
					bobSpeed * (float) delta
				);
				
			} else if (Rotation.X >= parentRotation.X + headbobAngle){
				Rotation = Rotation.Lerp(
					new Vector3(RotationalHelper.Rotation.X - headbobAngle, parentRotation.Y, parentRotation.Z),
					bobSpeed * (float) delta
				);
			}

		} else if (!Player.IsOnFloor()) {
			// falling
			Rotation = Rotation.Lerp(new Vector3(parentRotation.X + maxFallingAngle, parentRotation.Y, parentRotation.Z),
			bobSpeed / 10 * (float) delta
		);

		} else {
			// not moving
			Rotation = Rotation.Lerp(parentRotation, bobSpeed * (float) delta);
		}
	}
}
