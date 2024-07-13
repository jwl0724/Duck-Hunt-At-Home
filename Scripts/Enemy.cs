using Godot;
using System;

public partial class Enemy : CharacterBody3D {
	// exported variables
	[Export] public float FallSpeed = -50f; // different from gravity, constant fall rate
	[Export] public RigidBody3D Ragdoll;
	[Export] public Timer DeleteDelayTimer;
	[Export] public EnemyVisual Model;
	[Export] public EnemyAttackHandler AttackHandler;
	[Export] public VisibleOnScreenNotifier3D VisibilityNotifier;

	// signals
	[Signal] public delegate void EnemyShootEventHandler();
	[Signal] public delegate void EnemyDiedEventHandler();
	[Signal] public delegate void EnemyDamagedEventHandler();
	[Signal] public delegate void MoveStateChangeEventHandler();
	
	// static variables
	public static int EnemyCount { get; private set; } = 0;
	public static int BossScale { get; private set; } = 2;

	// instance variables
	public int Health { get; private set; } = 1;
	public int Attack { get; private set; } = 50;
	public float Speed { get; private set; } = 400;
	public int KnockbackStrength { get; private set; } = 1;
	public EnemyType Type { get; private set; }
	public MoveState CurrentState { get; set; } = MoveState.Idle;
	public Vector3 Direction { get; set; } = new();
	private CharacterBody3D player;
	private int DespawnY;
	
	// enemy types, boss needs to always be at the bottom
	public enum EnemyType {
		Melee, Charger, Shooter, Boss
	}

	// possible states
	public enum MoveState {
		Idle, Walking, Falling, Charging, WindUp, Dead, Ragdoll
	}
	
    public override void _Ready() {
		GameManager gameScene = GetParent() as GameManager;
		player = gameScene.GetNode<Player>("Player");
		DespawnY = (int) -gameScene.MapSize.Y / 2;
		AttackHandler = GetNode<EnemyAttackHandler>("AttackHandler");

		VisibilityNotifier.Connect("screen_entered", Callable.From(() => Model.Visible = true));
		VisibilityNotifier.Connect("screen_exited", Callable.From(() => Model.Visible = false));
		DeleteDelayTimer.Connect("timeout", Callable.From(() => SetMoveState(MoveState.Dead)));

		if (IsOnFloor()) SetMoveState(MoveState.Walking);
		else SetMoveState(MoveState.Falling);
    }

	public void SetEnemyProperties(EnemyType type) {
		if (type == EnemyType.Melee) {
			SetProperties(1, 90, 50, type, EnemyVisual.DuckColors.Default);

		} else if (type == EnemyType.Charger) {
			SetProperties(3, 70, 75, type, EnemyVisual.DuckColors.Pink, attackCD: 2);

		} else if (type == EnemyType.Shooter) {
			SetProperties(2, 50, 30, type, EnemyVisual.DuckColors.Blue, attackCD: 1f);

		} else {
			// boss properties
			SetProperties(10 + BossScale * 2, 80, 100, type, EnemyVisual.DuckColors.Green, attackCD: 2.5f);
			Model.ToggleGlow();
			KnockbackStrength = 2;
			
			Vector3 scale = new(BossScale, BossScale, BossScale);

			GetNode<CollisionShape3D>("Shape").Scale = scale;
			Ragdoll.GetNode<CollisionShape3D>("Shape").Scale = scale * 0.8f;
			Ragdoll.GetNode<EnemyVisual>("Model").Scale = scale;

			if (BossScale <= 4) BossScale++; // increment scaling for next boss
		}
	}

	private void SetProperties(int health, int speed, int attack, EnemyType type, EnemyVisual.DuckColors color, float attackCD = 0) {
		if (attackCD != 0) AttackHandler.SetAttackCD(attackCD);
		Health = health;
		Speed = speed;
		Attack = attack;
		Type = type;
		Model.SetColor(color);
	}

    public override void _PhysicsProcess(double delta) {
		// check if enemy died
		if (Health <= 0) {
			ProcessDeath();
			return;
		} else if (Position.Y <= DespawnY) QueueFree(); // delete if under the map
        ProcessMovement((float) delta);
    }

	public void OnShot(int damage) {
		Health -= damage;
		EmitSignal(SignalName.EnemyDamaged);
	}

	public void DeleteEnemy() {
		foreach(var child in GetChildren()) {
			child.QueueFree();
		}
		QueueFree();
	}
	
	private void FacePlayer() {
		Vector3 targetDirection = player.Position - GlobalTransform.Origin;
		if (Mathf.Abs(targetDirection.Normalized().Dot(Vector3.Up)) > 0.999f) return;

		LookAt(player.Position);
		Rotation = new Vector3(0, Rotation.Y, 0); // rotate only left or right
	}

	private void ProcessMovement(float delta) {
		if (CurrentState == MoveState.Walking || CurrentState == MoveState.Falling) {
			Direction = Position.DirectionTo(player.Position).Normalized();
			Vector3 interpolatedVelocity = Velocity.Lerp(Direction * Speed, 5);
			float Xcomponent = interpolatedVelocity.X * delta;
			float Ycomponent = FallSpeed * delta;
			float Zcomponent = interpolatedVelocity.Z * delta;
			Velocity = new(Xcomponent, Ycomponent, Zcomponent);
			FacePlayer();

		} else if (CurrentState == MoveState.Idle || CurrentState == MoveState.WindUp) {
			Velocity = Vector3.Zero;
			FacePlayer();

		} else if (CurrentState == MoveState.Charging) {
			Velocity = Direction * Speed / 2;

		} else return;

		MoveAndSlide();
	}

	private void ProcessDeath() {
		if (CurrentState == MoveState.Ragdoll || CurrentState == MoveState.Dead) return;
	
		SetCollisionLayerValue(3, false);
		SetPhysicsProcess(false); // disable physics processing once dead
		SetMoveState(MoveState.Ragdoll);
		Ragdoll.ProcessMode = ProcessModeEnum.Inherit;
		Ragdoll.LinearVelocity = Velocity;
		Ragdoll.AngularVelocity = Velocity * Vector3.Back;

		// determine score amount
		int score;
		if (Type == EnemyType.Charger) score = 20;
		else if (Type == EnemyType.Shooter) score = 25;
		else if (Type == EnemyType.Boss) score = 50;
		else score = 10; // melee enemy score
		
		EmitSignal(SignalName.EnemyDied, score);
		DeleteDelayTimer.Start();
	}

	public void SetMoveState(MoveState state) {
		if (CurrentState != state) {
			CurrentState = state;
			EmitSignal(SignalName.MoveStateChange);
		}
	}
}
