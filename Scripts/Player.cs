using Godot;
using System;
using System.Collections.Generic;

// TODO:
// FINISH PUTTING PLANTS IN CITY LEVEL
// FINISH CITY LEVEL WORLD ENVIRONMENT AND LIGHTING
// MAKE SPACE LEVEL WITH NEON THEME
// MAKE MENU SCREEN (NEW SCENE WITH PANNING BACKGROUND)
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
	private static readonly float frictionCoefficient = 0.4f;
	private static readonly int grappleSpeed = 1000;

	// instance variables
	public int MaxHealth { get; private set; } = 50000000;
	public int Health { get; private set; }
	public float Gravity { get; private set; } = 25;
	public int Attack { get; private set; } = 100;
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
		if (IsOnFloor()) {
			Velocity *= frictionCoefficient;
			ApplyForce(movementDirection * Speed / frictionCoefficient);
		} 
		if (!GrapplePoint.IsZeroApprox() && Grappled) {
			Velocity = Position.DirectionTo(GrapplePoint).Normalized() * grappleSpeed / frictionCoefficient * (float) delta;
		}

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
		HandleCollision();
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
		
	}

	private void HandleCollision() {
		// TODO: FIX ENEMY KNOCKBACK
		if (IFrameTimer.TimeLeft != 0) return;
		if (GetLastSlideCollision() == null) return;

		var collidingObject = GetLastSlideCollision().GetCollider();
		if (collidingObject is StaticBody3D) return;

		IFrameTimer.Start();
		if (collidingObject is Enemy enemy) {
			Health -= enemy.Attack;
			Vector3 knockbackVector = new(enemy.Velocity.X, enemy.KnockbackStrength, enemy.Velocity.Z);
			knockbackVector /= (float) GetProcessDeltaTime();
			ApplyForce(knockbackVector * enemy.KnockbackStrength);

		} else if (collidingObject is Projectile projectile) {
			Health -= projectile.Damage;
			ApplyForce(projectile.LinearVelocity * Projectile.KnockbackStrength * 10);
		}
		EmitSignal(SignalName.PlayerDamaged);

		if (Health <= 0) EmitSignal(SignalName.PlayerDied);
	}
}
