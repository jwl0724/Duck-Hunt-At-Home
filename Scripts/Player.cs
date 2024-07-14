using Godot;
using System;
using System.Collections.Generic;

// TODO:
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

	// instance variables
	public int MaxHealth { get; private set; } = 50000000;
	public int Health { get; private set; }
	public float Gravity { get; set; } = 25;
	public int Attack { get; private set; } = 1;
	public float Speed { get; private set; } = 400;
	public float JumpSpeed { get; set; } = 15;
	public int GrappleSpeed { get; set; } = 1000;
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
		IFrameTimer.Connect("timeout", Callable.From(() => OnIFrameTimeout()));

		ResetPlayerState();
	}

    public override void _PhysicsProcess(double delta) {
		Vector3 movementVector = movementDirection * Speed * (float) delta;
		if (IFrameTimer.TimeLeft == 0 && IsOnFloor()) {
			Velocity *= frictionCoefficient;
			ApplyForce(movementVector);
		}

		if (!GrapplePoint.IsZeroApprox() && Grappled) 
			Velocity = Position.DirectionTo(GrapplePoint).Normalized() * GrappleSpeed / frictionCoefficient * (float) delta;

		ApplyForce(Vector3.Down * Gravity * (float) delta, ignoreIFrame: true);
		foreach(Vector3 force in forceList) {
			Velocity += force;
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

	public void ApplyForce(Vector3 force, bool isInput = false, bool ignoreIFrame = false) {
		if (IFrameTimer.TimeLeft != 0 && !ignoreIFrame) return; // iframes still active
		if (isInput) movementDirection = force;
		else forceList.AddFirst(force);
	}

	public void DamagePlayer(int damage) {
		if (IFrameTimer.TimeLeft != 0) return;
		Health -= damage;
		if (Health <= 0) EmitSignal(SignalName.PlayerDied);
		else {
			EmitSignal(SignalName.PlayerDamaged);
			TogglePhysicsMasks(false);
			IFrameTimer.Start();
		}
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

	private void OnIFrameTimeout() {
		TogglePhysicsMasks(true);
	}

	// HELPER FUNCTIONS
	private void HandlePlayerDeath() {
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	private void TogglePhysicsMasks(bool state) {
		SetCollisionMaskValue(2, state);
		SetCollisionMaskValue(3, state);
	}
}
