using Godot;
using System;
using System.Collections.Generic;

// TODO:
// FINISH PUTTING PLANTS IN CITY LEVEL
// FINISH CITY LEVEL WORLD ENVIRONMENT AND LIGHTING
// IMPLEMENT SHIELD BOUNCE EFFECT
// MAKE SPACE LEVEL WITH NEON THEME
// MAKE MENU SCREEN (NEW SCENE WITH PANNING BACKGROUND)
// MAKE GAME OVER SCREEN (REGULAR SCREEN)
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
	private static readonly float frictionCoefficient = 0.4f;
	private static readonly int grappleSpeed = 1000;

	// instance variables
	public int MaxHealth { get; private set; } = 50000000;
	public int Health { get; private set; }
	public float Gravity { get; private set; } = 25;
	public int Attack { get; private set; } = 1000;
	public float Speed { get; private set; } = 200;
	public float JumpSpeed { get; private set; } = 15;
	public float MouseSensitivity { get; private set; } = 0.1f;
	public int ClipSize { get; private set; } = defaultClipSize;
	public int Bullets { get; private set; } = defaultClipSize;
	public bool Grappled { get; set; } = false;
	public bool Reloading { get; private set; } = false;
	public Vector3 GrapplePoint = new();
	private Node3D RotationalHelper;
	private readonly LinkedList<Vector3> forceList = new();
	private Vector3 movementDirection = new();

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
		Vector3 movementVector = movementDirection * Speed / frictionCoefficient;

		if (IFrameTimer.TimeLeft != 0) movementVector = Vector3.Zero;
		if (IsOnFloor() && IFrameTimer.TimeLeft == 0) Velocity *= frictionCoefficient;
		else movementVector *= (float) delta * 1.3f;
		ApplyForce(movementVector);

		if (!GrapplePoint.IsZeroApprox() && Grappled) 
			Velocity = Position.DirectionTo(GrapplePoint).Normalized() * grappleSpeed / frictionCoefficient * (float) delta;

		HandleCollision();
		ApplyForce(Vector3.Down * Gravity);
		foreach(Vector3 force in forceList) {
			Velocity += force * (float) delta;
		}
		forceList.Clear();
		MoveAndSlide();
	}

	public override void _Process(double delta) {
		if (Position.Y < -(GetParent() as GameManager).MapSize.Y) {
			if (Health <= 0) return;
			Health = 0;
			SetPhysicsProcess(false);
			EmitSignal(SignalName.PlayerDied);
		}
	}

	public void ApplyForce(Vector3 force, bool isInput = false) {
		if (isInput) movementDirection = force;
		else forceList.AddFirst(force);
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
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	private void HandleCollision() {
		// TODO: FIX ENEMY KNOCKBACK, FIX ENEMY DEAD BODY HURTING PLAYER
		if (IFrameTimer.TimeLeft != 0) return;
		if (GetLastSlideCollision() == null) return;

		var collidingObject = GetLastSlideCollision().GetCollider();
		if (collidingObject is StaticBody3D) return;

		IFrameTimer.Start();
		if (collidingObject is Enemy enemy) {
			Health -= enemy.Attack;
			// Vector3 knockbackVector = new(enemy.Velocity.X, enemy.KnockbackStrength, enemy.Velocity.Z);
			// ApplyForce(knockbackVector * enemy.KnockbackStrength);
			ApplyForce(enemy.Velocity * enemy.KnockbackStrength / (float) GetProcessDeltaTime());

		} else if (collidingObject is Projectile projectile) {
			Health -= projectile.Damage;
			// ApplyForce(projectile.LinearVelocity / (float) GetProcessDeltaTime() / 2);
			Vector3 force = projectile.LinearVelocity;
			ApplyForce(force / (float) GetProcessDeltaTime());
		}
		EmitSignal(SignalName.PlayerDamaged);

		if (Health <= 0) EmitSignal(SignalName.PlayerDied);
	}
}
