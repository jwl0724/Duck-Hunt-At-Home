using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export] public float Gravity = -20f;
	[Export] public float Speed = 50f;
	[Export] public float JumpSpeed = 14f;
	[Export] public float MouseSensitivity = 0.1f;
	private Vector3 direction = new();
	private Camera3D camera;

	public override void _Ready() {
		camera = GetNode<Camera3D>("Camera");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _PhysicsProcess(double delta) {
		ProcessInput();
		ProcessMovement((float) delta);
	}

	public override void _Input(InputEvent mouseMovement) {
		if (mouseMovement is not InputEventMouseMotion) return;
		if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
		ProcessMouse(mouseMovement as InputEventMouseMotion);
	}

	private void ProcessInput() {
		direction = new();
		Vector2 groundVector = new();
		if (Input.IsActionPressed("forward")) groundVector.Y += 1;
		if (Input.IsActionPressed("backward")) groundVector.Y -= 1;
		if (Input.IsActionPressed("left")) groundVector.X -= 1;
		if (Input.IsActionPressed("right")) groundVector.X += 1;
		groundVector = groundVector.Normalized();

		// check if player jumped
		if (IsOnFloor()) {
			if (Input.IsActionJustPressed("jump")) 
				Velocity = new Vector3(Velocity.X, JumpSpeed, Velocity.Z);
		}

		// TODO: why????
		Transform3D cameraPosition = camera.GetCameraTransform();
		direction += cameraPosition.Basis.X * groundVector.X;
		direction += cameraPosition.Basis.Y * groundVector.Y;

		// pause game (free cursor)
		if (Input.IsActionJustPressed("pause")) {
			if (Input.MouseMode == Input.MouseModeEnum.Visible)
				Input.MouseMode = Input.MouseModeEnum.Captured;
			else Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	private void ProcessMovement(float delta) {
		float Ycomponent = delta * Gravity + Velocity.Y;

		// TODO: WHY??????	
		float Xcomponent = direction.X * Speed * delta;
		float Zcomponent = direction.Z * Speed * delta;
		Velocity = new(Xcomponent, Ycomponent, Zcomponent);

		MoveAndSlide();
	}

	private void ProcessMouse(InputEventMouseMotion movement) {
		// handle left/right camera movement
		float rotationDegreesY = camera.Rotation.Y + Mathf.DegToRad(-movement.Relative.X * MouseSensitivity);

		// handle up/down camera movement
		float rotationDegreesX = Mathf.Clamp(
			Mathf.DegToRad(-movement.Relative.Y * MouseSensitivity) + camera.Rotation.X, 
			Mathf.DegToRad(-75), Mathf.DegToRad(75)
		);

		// normalize vector components
		Vector2 normalizeVectors = new(rotationDegreesX, rotationDegreesY);
		camera.Rotation = new Vector3(normalizeVectors.X, normalizeVectors.Y, 0);
	}
}
