using Godot;
using System;

// TODO:
// HAVE HUD WITH RELEVANT INFO
public partial class Player : CharacterBody3D {	
	// exported variables
	[Export] public Timer IFrameTimer;
	[Export] public GunVisual GunModel;
	[Export] public GrappleVisual GrappleGunModel;

	// signals
	[Signal] public delegate void PlayerShootEventHandler();
	[Signal] public delegate void PlayerDiedEventHandler();
	[Signal] public delegate void PlayerDamagedEventHandler();
	[Signal] public delegate void PlayerReloadEventHandler();
	[Signal] public delegate void PlayerEmptyMagEventHandler();
	[Signal] public delegate void PlayerGrappleEventHandler();

	// static variables
	private static readonly int defaultClipSize = 25;
	private static readonly int acceleration = 10;
	private static readonly int decceleration = 15;
	private static readonly int grappleSpeed = 2000;

	// instance variables
	public int MaxHealth { get; private set; } = 500;
	public int Health { get; private set; }
	public float Gravity { get; private set; } = -12f;
	public int Attack { get; private set; } = 100;
	public float Speed { get; private set; } = 100f;
	public float JumpSpeed { get; private set; } = 5f;
	public float MouseSensitivity { get; private set; } = 0.1f;
	public int ClipSize { get; private set; } = defaultClipSize;
	public int Bullets { get; private set; } = defaultClipSize;
	public bool Grappled = false;
	public Vector3 GrapplePoint = new();
	private Vector3 direction = new();
	private Vector3 knockbackVector = new();
	private Node3D RotationalHelper;
	private bool reloading = false;

	public override void _Ready() {
		RotationalHelper = GetNode<Node3D>("RotationalHelper");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GunModel.Animator.Connect("animation_finished", Callable.From((StringName name) => OnGunAnimationFinished(name)));
		ResetPlayerState();
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

	public void ResetPlayerState() {
		// TODO, ADD MORE STUFF HERE THAT NEEDS TO BE RESET
		Health = MaxHealth;
		ClipSize = defaultClipSize;
		Bullets = defaultClipSize;
	}

	// SIGNAL HANDLERS
	private void OnGunAnimationFinished(StringName animationName) {
		if (animationName != "reload") return;
		reloading = false;
	}

	// HELPER FUNCTIONS
	private void ProcessInput() {
		// pause game (free cursor)
		if (Input.IsActionJustPressed("pause")) {
			if (Input.MouseMode == Input.MouseModeEnum.Visible)
				Input.MouseMode = Input.MouseModeEnum.Captured;
			else Input.MouseMode = Input.MouseModeEnum.Visible;
			return;
		}

		// check shooting
		if (Input.IsActionJustPressed("shoot") && !reloading) {
			if (Bullets == 0) {
				EmitSignal(SignalName.PlayerEmptyMag);
				return;
			}
			EmitSignal(SignalName.PlayerShoot);
			Bullets--;
		}

		// check reloading
		if (Input.IsActionJustPressed("reload") && !reloading) {
			if (Bullets == ClipSize) return;
			reloading = true;
			Bullets = ClipSize;
			EmitSignal(SignalName.PlayerReload);
		}

		// check grapple, everything past grapple not allowed when grappled
		if (Input.IsActionPressed("grapple")) {
			if (!Grappled) {
				Grappled = true;
				EmitSignal(SignalName.PlayerGrapple);
			}

		} else if (Input.IsActionJustReleased("grapple")) {
			Grappled = false;
			GrapplePoint = new();
			EmitSignal(SignalName.PlayerGrapple);
		}

		// check if player jumped
		if (IsOnFloor()) {
			if (Input.IsActionJustPressed("jump") && GrapplePoint.IsZeroApprox()) 
				Velocity = new Vector3(Velocity.X, JumpSpeed, Velocity.Z);
		}
		// Account for camera rotation
		// TODO: fix bug where looking up and down slows movement
		Vector2 inputVector = Input.GetVector("left", "right", "forward", "backward");
		direction = RotationalHelper.Transform.Basis * new Vector3(inputVector.X, 0, inputVector.Y);
	}

	private void ProcessMovement(float delta) {
		float Ycomponent = delta * Gravity + Velocity.Y;

		if (Grappled && !GrapplePoint.IsZeroApprox()) {
			// player is grappled
			Velocity = Position.DirectionTo(GrapplePoint).Normalized() * grappleSpeed * delta;

		} else if (IsOnFloor()) {
			// player is moving on ground
			float lerpSpeed;
			if (direction.Dot(Velocity) > 0) lerpSpeed = acceleration;
			else lerpSpeed = decceleration;
			
			Vector3 velocity = Velocity.Lerp(direction * Speed, lerpSpeed);
			float Xcomponent = velocity.X * delta + knockbackVector.X;
			float Zcomponent = velocity.Z * delta + knockbackVector.Z;
			Velocity = new(Xcomponent, Ycomponent, Zcomponent);

		} else Velocity = new(Velocity.X, Ycomponent, Velocity.Z); // player is midair
			
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
			EmitSignal(SignalName.PlayerDamaged);

		} else if (collidingObject is RigidBody3D) {
			if (collidingObject is not Projectile projectile) return;

			if (IFrameTimer.TimeLeft == 0) IFrameTimer.Start();
			Health -= projectile.Damage;
			ApplyKnockback(projectile.LinearVelocity, Projectile.KnockbackStrength);
			EmitSignal(SignalName.PlayerDamaged);
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
