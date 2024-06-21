using Godot;
using System;

public partial class Player : CharacterBody3D
{	// exported variables
	[Export] public float Gravity = -20f;
	[Export] public float Speed = 100f;
	[Export] public float JumpSpeed = 12.5f;
	[Export] public float MouseSensitivity = 0.1f;
	[Export] public int Health = 300;
	[Export] public int Attack = 25;

	// signals
	[Signal] public delegate void PlayerShootEventHandler();

	// instance variables
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

		// check if player jumped
		if (IsOnFloor()) {
			if (Input.IsActionJustPressed("jump")) 
				Velocity = new Vector3(Velocity.X, JumpSpeed, Velocity.Z);
		}

		// Account for camera rotation (multiply Basis by input vector = always forward)
		Vector2 inputVector = Input.GetVector("left", "right", "forward", "backward");
		direction = camera.Transform.Basis * new Vector3(inputVector.X, 0, inputVector.Y);

		// pause game (free cursor)
		if (Input.IsActionJustPressed("pause")) {
			if (Input.MouseMode == Input.MouseModeEnum.Visible)
				Input.MouseMode = Input.MouseModeEnum.Captured;
			else Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	private void ProcessMovement(float delta) {
		float Ycomponent = delta * Gravity + Velocity.Y;

		// interpolate for smoother movement
		Vector3 velocity = Velocity;
		velocity = velocity.Lerp(direction * Speed, 5);
		float Xcomponent = velocity.X * delta;
		float Zcomponent = velocity.Z * delta;

		Velocity = new(Xcomponent, Ycomponent, Zcomponent);		
		MoveAndSlide();
	}

	private void ProcessMouse(InputEventMouseMotion movement) {
		// handle left/right camera movement
		float rotationDegreesY = camera.Rotation.Y + Mathf.DegToRad(-movement.Relative.X * MouseSensitivity);

		// handle up/down camera movement
		float rotationDegreesX = Mathf.Clamp(
			Mathf.DegToRad(-movement.Relative.Y * MouseSensitivity) + camera.Rotation.X, 
			Mathf.DegToRad(-85), Mathf.DegToRad(85)
		);

		// normalize vector components
		Vector2 normalizeVectors = new(rotationDegreesX, rotationDegreesY);
		camera.Rotation = new Vector3(normalizeVectors.X, normalizeVectors.Y, 0);
	}
}
