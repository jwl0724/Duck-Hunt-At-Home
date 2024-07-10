using Godot;
using System;

public partial class InputManager : Node3D {
	[Export] public Node3D RotationalHelper;
	[Export] public GunVisual GunModel;
	[Export] public GrappleVisual GrappleModel;

	[Signal] public delegate void PlayerShootEventHandler();
	[Signal] public delegate void PlayerEmptyMagEventHandler();
	[Signal] public delegate void PlayerReloadEventHandler();
	[Signal] public delegate void PlayerGrappleEventHandler();

	private Player player;

	public override void _Ready() {
		player = GetParent() as Player;
	}

	public override void _Process(double delta) {
		ProcessMovementInputs();
		ProcessGrappleInput();
	}

	public override void _Input(InputEvent inputEvent) {
		if (inputEvent is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
			ProcessMouse(inputEvent as InputEventMouseMotion);

		if (inputEvent is InputEventMouseButton)
			ProcessMouseButtonInput();

		if (inputEvent is InputEventKey) {
			ProcessReload();
			ProcessKeyInputs();
		}
	}

	private void ProcessMouse(InputEventMouseMotion movement) {
		// handle left/right camera movement
		float rotationDegreesY = RotationalHelper.Rotation.Y + Mathf.DegToRad(-movement.Relative.X * player.MouseSensitivity);

		// handle up/down camera movement
		float rotationDegreesX = Mathf.Clamp(
			Mathf.DegToRad(-movement.Relative.Y * player.MouseSensitivity) + RotationalHelper.Rotation.X, 
			Mathf.DegToRad(-89), Mathf.DegToRad(85)
		);

		// normalize vector components
		Vector2 normalizeVectors = new(rotationDegreesX, rotationDegreesY);
		RotationalHelper.Rotation = new Vector3(normalizeVectors.X, normalizeVectors.Y, 0);

		float tiltY = movement.Relative.X / -500f;
		float tiltZ = movement.Relative.Y / -500f;

		GunModel.Rotation = GunModel.Rotation.Lerp(new Vector3(0, tiltY, tiltZ), 0.3f);
		GrappleModel.Rotation = GrappleModel.Rotation.Lerp(new Vector3(0, tiltY, tiltZ), 0.3f);
	}


	private void ProcessMouseButtonInput() {
		// check shooting
		if (Input.IsActionJustPressed("shoot") && !player.Reloading) {
			if (player.Bullets == 0) {
				EmitSignal(SignalName.PlayerEmptyMag);
				return;
			}
			EmitSignal(SignalName.PlayerShoot);
		}
	}

	private void ProcessReload() {
		if (Input.IsActionJustPressed("reload") && !player.Reloading) {
			if (player.Bullets == player.ClipSize) return;
			EmitSignal(SignalName.PlayerReload);
		}
	}

	private void ProcessGrappleInput() {
		if (Input.IsActionPressed("grapple")) {
			if (player.Grappled) return;
			player.Grappled = true;
			EmitSignal(SignalName.PlayerGrapple);

		} else if (Input.IsActionJustReleased("grapple")) {
			player.Grappled = false;
			player.GrapplePoint = Vector3.Zero;
			EmitSignal(SignalName.PlayerGrapple);
		}
	}

	private void ProcessMovementInputs() {
		// check if player jumped
		if (player.IsOnFloor()) {
			if (Input.IsActionJustPressed("jump") && player.GrapplePoint.IsZeroApprox()) 
				player.ApplyForce(Vector3.Up * player.JumpSpeed / (float) GetProcessDeltaTime());
		}
		// Account for camera rotation
		Vector2 inputVector = Input.GetVector("left", "right", "forward", "backward");
		Vector3 forwardVector = RotationalHelper.Transform.Basis * new Vector3(inputVector.X, 0, inputVector.Y);
		Vector3 movementDirection = new(forwardVector.X, 0, forwardVector.Z);
		
		player.ApplyForce(movementDirection, true);
	}

	private void ProcessKeyInputs() {
		// pause game (free cursor)
		if (Input.IsActionJustPressed("pause")) {
			if (Input.MouseMode == Input.MouseModeEnum.Visible)
				Input.MouseMode = Input.MouseModeEnum.Captured;
			else Input.MouseMode = Input.MouseModeEnum.Visible;
			return;
		}
	}
}
