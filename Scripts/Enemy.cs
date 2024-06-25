using Godot;
using System;

// TODO TODAY:
// CHARGER LOGIC (WAIT THEN SPIN)
// PROJECTILE THAT SHOOTS AND GLOWS
// BOSS THATS BIGGER
// ADD KNOCKBACK
// CHANGE COLOR OF DUCK DEPENDING ON ENEMY TYPE
public partial class Enemy : CharacterBody3D {
	// exported variables
	[Export] public float FallSpeed = -50f; // different from gravity, constant fall rate

	// signals
	[Signal] public delegate void EnemyShootEventHandler();
	[Signal] public delegate void EnemyDiedEventHandler();
	[Signal] public delegate void MoveStateChangeEventHandler();
	
	// instance variables
	public int Health { get; private set; } = 500;
	public int Attack { get; private set; } = 50;
	public float Speed { get; private set; } = 150;
	private Vector3 direction = new(0, 0, 0);
	private EnemyType type;
	private CharacterBody3D player;
	private bool processingAttack = false;
	public MoveState CurrentState { get; private set; } = MoveState.Idle;

	// timers
	private float attackTimer = 0;
	private float attackCD = 1.5f;

	// enemy types
	public enum EnemyType {
		Melee,
		Charger,
		Shooter,
		Boss
	}

	// possible states
	public enum MoveState {
		Walking,
		Idle,
		Falling,
		Charging
	}
	

    public override void _Ready() {
		player = GetParent().GetNode<CharacterBody3D>("Player");
    }

	public void SetEnemyProperties(EnemyType type) {
		// get model parts and change properties
		Node3D model = GetNode<Node3D>("Model");
		MeshInstance3D head = model.GetNode<MeshInstance3D>("Head");
		MeshInstance3D body = model.GetNode<MeshInstance3D>("Head");
		MeshInstance3D leftWing = model.GetNode<MeshInstance3D>("Head");
		MeshInstance3D rightWing = model.GetNode<MeshInstance3D>("Head");

		if (type == EnemyType.Melee) {
			Health = 500;
			Speed = 90;
			Attack = 50;

		} else if (type == EnemyType.Charger) {
			Health = 400;
			Speed = 70;
			Attack = 75;
			attackCD = 10f;

		} else if (type == EnemyType.Shooter) {
			Health = 300;
			Speed = 50;
			Attack = 30;
			attackCD = 2.5f;

		} else {
			// boss properties
			Health = 2000;
			Speed = 80;
			Attack = 100;
			attackCD = 5f;
		}
	}

    public override void _PhysicsProcess(double delta) {
		// check if enemy died
		if (Health <= 0) {
			ProcessDeath();
			return;
		}
		ProcessAttack((float) delta);
        ProcessMovement((float) delta);
    }

	// SIGNAL HANDLERS
	private void OnShot(int damage) {
		Health -= damage;
	}

	// HELPER FUNCTIONS
	private void ProcessAttack(float delta) {
		if (type == EnemyType.Melee) return;
		if (type == EnemyType.Shooter) {
			if (attackTimer >= attackCD) {
				EmitSignal(SignalName.EnemyShoot);
				attackTimer = 0;
			}
		} else if (type == EnemyType.Charger) {
			CurrentState = MoveState.Charging;
		} else {
			// handle boss attacks
		}
		attackTimer += delta;
	}

	private void ProcessMovement(float delta) {
		float Ycomponent;
		if (!IsOnFloor()) {
			Ycomponent = FallSpeed * delta;
			SetMoveState(MoveState.Falling);
		} else {
			Ycomponent = 0;
			SetMoveState(MoveState.Walking);
		}
		Vector3 playerPosition = player.Position;
		direction = Position.DirectionTo(playerPosition);
		
		Velocity = new(direction.X * Speed * delta, Ycomponent, direction.Z * delta * Speed);
		if (Velocity.IsZeroApprox()) SetMoveState(MoveState.Idle);
		LookAt(player.Position);
		Rotation = new Vector3(0, Rotation.Y, 0);
		MoveAndSlide();
	}

	private void ProcessDeath() {
		// determine score amount
		int score;
		if (type == EnemyType.Charger) score = 20;
		else if (type == EnemyType.Shooter) score = 25;
		else if (type == EnemyType.Boss) score = 50;
		else score = 10; // melee enemy score

		EmitSignal(SignalName.EnemyDied, score);
		QueueFree();
	}

	private void SetMoveState(MoveState state) {
		if (CurrentState != state) {
			CurrentState = state;
			EmitSignal(SignalName.MoveStateChange);
		}
	}
}
