using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody3D {	
	// exported variables
	[Export] public Timer IFrameTimer;
	[Export] public GunVisual GunModel;
	[Export] public GrappleVisual GrappleGunModel;
	[Export] public HUD HUD;
	[Export] public InputManager InputManager;
	[Export] public PlayerMenuManager MenuManager;

	// signals
	[Signal] public delegate void PlayerDiedEventHandler();
	[Signal] public delegate void PlayerDamagedEventHandler();

	// static variables
	private static readonly int defaultClipSize = 25;
	private static readonly float frictionCoefficient = 0.4f;

	// instance variables
	public int MaxHealth { get; private set; } = 500;
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
	private Vector3 movementDirection = new();
	private Node3D RotationalHelper;
	private readonly LinkedList<Vector3> forceList = new();

	public override void _Ready() {
		RotationalHelper = GetNode<Node3D>("RotationalHelper");
		Input.MouseMode = Input.MouseModeEnum.Captured;

		// connect signals
		GunModel.Animator.Connect("animation_finished", Callable.From((StringName name) => OnGunAnimationFinished(name)));
		InputManager.Connect("PlayerShoot", Callable.From(() => Bullets--));
		InputManager.Connect("PlayerReload", Callable.From(() => Reloading = true));
		IFrameTimer.Connect("timeout", Callable.From(() => OnIFrameTimeout()));
		MenuManager.Connect("RestartGame", Callable.From(() => ResetPlayerState()));

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
		if (Health <= 0) {
			HandlePlayerDeath();
			EmitSignal(SignalName.PlayerDied);

		} else {
			EmitSignal(SignalName.PlayerDamaged);
			TogglePhysicsMasks(false);
			IFrameTimer.Start();
		}
	}

	public void ResetPlayerState() {
		Input.MouseMode = Input.MouseModeEnum.Captured;
		Health = MaxHealth;
		ClipSize = defaultClipSize;
		Bullets = defaultClipSize;

		RotationalHelper.Rotation = Vector3.Zero;
		Position = Vector3.Zero;
		Velocity = Vector3.Zero;
		movementDirection = Vector3.Zero;
		GrapplePoint = Vector3.Zero;

		Grappled = false;
		Reloading = false;
		HUD.Visible = true;
		
		forceList.Clear();
		IFrameTimer.Stop();
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
		HUD.Visible = false;
	}

	private void TogglePhysicsMasks(bool state) {
		SetCollisionLayerValue(4, state);
		SetCollisionMaskValue(2, state);
		SetCollisionMaskValue(3, state);
	}
}
