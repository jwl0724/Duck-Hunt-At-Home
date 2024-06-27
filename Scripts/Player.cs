using Godot;
using System;

public partial class Player : CharacterBody3D
{	// exported variables
	[Export] public float Gravity = -20f;
	[Export] public float Speed = 100f;
	[Export] public float JumpSpeed = 12.5f;
	[Export] public float MouseSensitivity = 0.1f;
	[Export] public int Health = 500;
	[Export] public int Attack = 25;

	// signals
	[Signal] public delegate void PlayerShootEventHandler();
	[Signal] public delegate void PlayerDiedEventHandler();

	// instance variables
	private Vector3 direction = new();
	private Vector3 knockbackVector = new();
	private Camera3D camera;
	

	public override void _Ready() {
		camera = GetNode<Camera3D>("Camera");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _PhysicsProcess(double delta) {
		if (Health <= 0) {
			EmitSignal(SignalName.PlayerDied);
			return;
		}
		ProcessInput();
		ProcessMovement((float) delta);
	}

	public override void _Input(InputEvent mouseMovement) {
		if (mouseMovement is not InputEventMouseMotion) return;
		if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
		ProcessMouse(mouseMovement as InputEventMouseMotion);
	}

	// HELPER FUNCTIONS
	private void ProcessInput() {
		// check if player jumped
		if (IsOnFloor()) {
			if (Input.IsActionJustPressed("jump")) 
				Velocity = new Vector3(Velocity.X, JumpSpeed, Velocity.Z);
		}

		// Account for camera rotation (multiply Basis by input vector = always forward)
		// TODO: FIX BUG WHERE LOOKING DOWN CAUSES FORWARD MOVEMENT TO SLOW
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

		// calcualte velocity via interpolation
		Vector3 velocity = Velocity.Lerp(direction * Speed, 5);
		float Xcomponent = velocity.X * delta + knockbackVector.X;
		float Zcomponent = velocity.Z * delta + knockbackVector.Z;
		Velocity = new(Xcomponent, Ycomponent, Zcomponent);

		// handle knockback
		bool collided = MoveAndSlide();
		if (collided && GetLastSlideCollision().GetCollider() is not StaticBody3D) {
			HandleCollision();
			return;
		}
		if (IsOnFloor()) knockbackVector = new();
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

	private void HandleCollision() {
		var collidingObject = GetLastSlideCollision().GetCollider();
		if (collidingObject is CharacterBody3D) {
			if (collidingObject is not Enemy enemy) return;
			Health -= enemy.Attack;
			ApplyKnockback(enemy.Velocity, enemy.KnockbackStrength);

		} else if (collidingObject is RigidBody3D) {
			if (collidingObject is not Projectile projectile) return;
			Health -= projectile.Damage;
			ApplyKnockback(projectile.LinearVelocity, Projectile.KnockbackStrength);
		}
	}

	private void ApplyKnockback(Vector3 movementDirection, int knockbackStrength) {
		int knockbackHeight = (int) Mathf.Min(knockbackStrength * 1.5f, 20);
		Velocity = new Vector3(Velocity.X, knockbackHeight, Velocity.Z);
		Vector3 knockbackDirection = movementDirection.Normalized();
		knockbackVector += new Vector3(knockbackDirection.X * knockbackStrength, 0, knockbackDirection.Z * knockbackStrength);
	}
}
