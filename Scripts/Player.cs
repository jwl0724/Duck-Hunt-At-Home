using Godot;
using System;

// TODO:
// IMPLEMENT ACTUAL SHOOTING MECHANICS
// ADD RELOADING MECHANICS
// ADD THROWING GERNADE
// ADD GRAPPLING HOOK MECHANIC
// HAVE HUD WITH RELEVANT INFO
public partial class Player : CharacterBody3D
{	// exported variables
	[Export] public float Gravity = -20f;
	[Export] public float Speed = 100f;
	[Export] public float JumpSpeed = 12.5f;
	[Export] public float MouseSensitivity = 0.1f;
	[Export] public int Health = 500;
	[Export] public int Attack = 25;
	[Export] public Timer IFrameTimer;
	[Export] public GunVisual GunModel;

	// signals
	[Signal] public delegate void PlayerShootEventHandler();
	[Signal] public delegate void PlayerDiedEventHandler();
	[Signal] public delegate void PlayerReloadEventHandler();

	// instance variables
	private Vector3 direction = new();
	private Vector3 knockbackVector = new();
	private Node3D RotationalHelper;
	private bool reloading = false;

	public override void _Ready() {
		RotationalHelper = GetNode<Node3D>("RotationalHelper");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GunModel.Animator.Connect("animation_finished", Callable.From((StringName name) => OnGunAnimationFinished(name)));
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

	// SIGNAL HANDLERS
	private void OnGunAnimationFinished(StringName animationName) {
		if (animationName != "reload") return;
		reloading = false;
	}

	// HELPER FUNCTIONS
	private void ProcessInput() {
		// check if player jumped
		if (IsOnFloor()) {
			if (Input.IsActionJustPressed("jump")) 
				Velocity = new Vector3(Velocity.X, JumpSpeed, Velocity.Z);
		}
		// check reloading
		if (Input.IsActionJustPressed("reload") && !reloading) {
			reloading = true;
			EmitSignal(SignalName.PlayerReload);
			// TODO: ADD RELOAD FUNCTIONALITY
		} 
		// check shooting
		if (Input.IsActionJustPressed("shoot") && !reloading) {
			EmitSignal(SignalName.PlayerShoot);
			// TODO: ADD SHOOTING FUNCTIONALITY
		}

		// Account for camera rotation (multiply Basis by input vector = always forward)
		// TODO: FIX BUG WHERE LOOKING DOWN CAUSES FORWARD MOVEMENT TO SLOW
		Vector2 inputVector = Input.GetVector("left", "right", "forward", "backward");
		direction = RotationalHelper.Transform.Basis * new Vector3(inputVector.X, 0, inputVector.Y);

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
		float rotationDegreesY = RotationalHelper.Rotation.Y + Mathf.DegToRad(-movement.Relative.X * MouseSensitivity);

		// handle up/down camera movement
		float rotationDegreesX = Mathf.Clamp(
			Mathf.DegToRad(-movement.Relative.Y * MouseSensitivity) + RotationalHelper.Rotation.X, 
			Mathf.DegToRad(-85), Mathf.DegToRad(85)
		);

		// normalize vector components
		Vector2 normalizeVectors = new(rotationDegreesX, rotationDegreesY);
		RotationalHelper.Rotation = new Vector3(normalizeVectors.X, normalizeVectors.Y, 0);

		// turn gun slightly when turning
		float tiltZ = 0, tiltY = 0;
		if (movement.Relative.X < -50) tiltY = 0.261799f;
		else if (movement.Relative.X > 50) tiltY = -0.261799f;
		if (movement.Relative.Y < -50) tiltZ = 0.261799f;
		else if (movement.Relative.Y > 50) tiltZ = -0.261799f;
		GunModel.Rotation = GunModel.Rotation.Lerp(new Vector3(0, tiltY, tiltZ), 0.3f);
	}

	private void HandleCollision() {
		if (IFrameTimer.TimeLeft != 0) return;
		var collidingObject = GetLastSlideCollision().GetCollider();
		if (collidingObject is CharacterBody3D) {
			if (collidingObject is not Enemy enemy) return;

			// if iframe timer is not running
			if (IFrameTimer.TimeLeft == 0) IFrameTimer.Start();
			Health -= enemy.Attack;
			ApplyKnockback(enemy.Velocity, enemy.KnockbackStrength);

		} else if (collidingObject is RigidBody3D) {
			if (collidingObject is not Projectile projectile) return;

			if (IFrameTimer.TimeLeft == 0) IFrameTimer.Start();
			Health -= projectile.Damage;
			ApplyKnockback(projectile.LinearVelocity, Projectile.KnockbackStrength);
		}
	}

	private void ApplyKnockback(Vector3 movementDirection, int knockbackStrength) {
		int knockbackHeight = (int) Mathf.Min(knockbackStrength * 1.5f, 20);
		Velocity = new Vector3(Velocity.X, knockbackHeight, Velocity.Z);
		float Xcomponent = Mathf.Clamp(movementDirection.Normalized().X * knockbackStrength + knockbackVector.X, -35, 35);
		float Zcomponent = Mathf.Clamp(movementDirection.Normalized().X * knockbackStrength + knockbackVector.X, -35, 35);
		knockbackVector = new Vector3(Xcomponent, 0, Zcomponent);
	}
}
