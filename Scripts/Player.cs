using Godot;
using System;
using System.Collections.Generic;

// TODO:
// FINISH PUTTING PLANTS IN CITY LEVEL
// FINISH CITY LEVEL WORLD ENVIRONMENT AND LIGHTING
// MAKE SPACE LEVEL WITH NEON THEME
// MAKE MENU SCREEN (NEW SCENE WITH PANNING BACKGROUND)
// REFACTOR CODE
// IRON OUT EGREGIOUS BUGS
// MAKE GAME OVER SCREEN (REGULAR SCREEN)
// IRON OUT MINOR BUGS
public partial class Player : CharacterBody3D {	
	// exported variables
	[Export] public Timer IFrameTimer;
	[Export] public GunVisual GunModel;
	[Export] public GrappleVisual GrappleGunModel;
	[Export] public HUD HUD;
	[Export] public InputManager InputManager;

	// signals
	[Signal] public delegate void PlayerDiedEventHandler();
	[Signal] public delegate void PlayerDamagedEventHandler();

	// static variables
	private static readonly int defaultClipSize = 25;
	private static readonly int acceleration = 10;
	private static readonly int decceleration = 15;
	private static readonly int grappleSpeed = 2000;

	// instance variables
	public int MaxHealth { get; private set; } = 50000000;
	public int Health { get; private set; }
	public float Gravity { get; private set; } = -12f;
	public int Attack { get; private set; } = 100;
	public float Speed { get; private set; } = 100f;
	public float JumpSpeed { get; private set; } = 20f;
	public float MouseSensitivity { get; private set; } = 0.1f;
	public int ClipSize { get; private set; } = defaultClipSize;
	public int Bullets { get; private set; } = defaultClipSize;
	public bool Grappled { get; set; } = false;
	public bool Reloading { get; private set; } = false;
	public Vector3 GrapplePoint = new();
	private Vector3 direction = new();
	private Vector3 knockbackVector = new();
	private Node3D RotationalHelper;
	private LinkedList<Vector3> ForceList = new();

	public override void _Ready() {
		RotationalHelper = GetNode<Node3D>("RotationalHelper");
		Input.MouseMode = Input.MouseModeEnum.Captured;

		// connect signals
		GunModel.Animator.Connect("animation_finished", Callable.From((StringName name) => OnGunAnimationFinished(name)));
		InputManager.Connect("PlayerShoot", Callable.From(() => Bullets--));
		InputManager.Connect("PlayerReload", Callable.From(() => Reloading = true));

		ResetPlayerState();
	}

    public override void _PhysicsProcess(double delta) {
		if (Health <= 0) return;

		Velocity -= Vector3.Down * Gravity * (float) delta;
		// Velocity *= 0.5f;

		foreach(Vector3 force in ForceList) {
			Velocity += force * (float) delta;
		}
		ForceList.Clear();
		MoveAndSlide();
	}

	public override void _Process(double delta) {
		HandleCollision();
	}

	public void ApplyForce(Vector3 force) {
		ForceList.AddFirst(force);
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
		Bullets = ClipSize;
		Reloading = false;
	}

	// HELPER FUNCTIONS
	private void HandlePlayerDeath() {
		
	}

	// private void ProcessMovement(float delta) {
	// 	// TODO: REFACTOR MOVEMENT
	// 	float Ycomponent = delta * Gravity + Velocity.Y;

	// 	if (Grappled && !GrapplePoint.IsZeroApprox()) {
	// 		// player is grappled
	// 		Velocity = Position.DirectionTo(GrapplePoint).Normalized() * grappleSpeed * delta;

	// 	} else if (IsOnFloor()) {
	// 		// player is moving on ground
	// 		float lerpSpeed;
	// 		if (direction.Dot(Velocity) > 0) lerpSpeed = acceleration;
	// 		else lerpSpeed = decceleration;
			
	// 		Vector3 velocity = Velocity.Lerp(direction * Speed, lerpSpeed);
	// 		float Xcomponent = velocity.X * delta + knockbackVector.X;
	// 		float Zcomponent = velocity.Z * delta + knockbackVector.Z;
	// 		Velocity = new(Xcomponent, Ycomponent, Zcomponent);

	// 	} else Velocity = new(Velocity.X, Ycomponent, Velocity.Z); // player is midair
			
	// 	// handle knockback
	// 	bool collided = MoveAndSlide();
	// 	if (collided && GetLastSlideCollision().GetCollider() is not StaticBody3D) {
	// 		HandleCollision();
	// 		return;
	// 	}
	// 	if (IsOnFloor()) knockbackVector = new();
	// }

	private void HandleCollision() {
		if (IFrameTimer.TimeLeft != 0) return;
		if (GetLastSlideCollision() == null) return;

		var collidingObject = GetLastSlideCollision().GetCollider();
		if (collidingObject is StaticBody3D) return;

		IFrameTimer.Start();
		if (collidingObject is Enemy enemy) {
			Health -= enemy.Attack;
			ApplyForce(enemy.Velocity * enemy.KnockbackStrength);

		} else if (collidingObject is Projectile projectile) {
			Health -= projectile.Damage;
			ApplyForce(projectile.LinearVelocity * Projectile.KnockbackStrength);
		}
		EmitSignal(SignalName.PlayerDamaged);

		if (Health <= 0) EmitSignal(SignalName.PlayerDied);
	}
}
