using Godot;
using System;

public partial class Enemy : CharacterBody3D {
	// exported variables
	[Export] public float FallSpeed = -50f; // different from gravity, constant fall rate
	[Export] public PackedScene Projectile;
	[Export] public Node3D ProjectileSpawnPoint;
	[Export] public Area3D Hurtbox;
	[Export] public RigidBody3D Ragdoll;
	[Export] public Timer DeleteDelayTimer;
	[Export] public EnemyVisual Model;

	// signals
	[Signal] public delegate void EnemyShootEventHandler();
	[Signal] public delegate void EnemyDiedEventHandler();
	[Signal] public delegate void EnemyDamagedEventHandler();
	[Signal] public delegate void MoveStateChangeEventHandler();
	
	// static variables
	public static int EnemyCount { get; private set; } = 0;
	private static int bossScale = 2;
	private static readonly int projectileSpeed = 250;
	private static int chargeTime = 1;
	private static int chargeDelay = 2;

	// instance variables
	public int Health { get; private set; } = 500;
	public int Attack { get; private set; } = 50;
	public float Speed { get; private set; } = 400;
	public int KnockbackStrength { get; private set; } = 5;
	private Vector3 direction = new(0, 0, 0);
	private EnemyType type;
	private CharacterBody3D player;
	private bool processingAttack = false;
	public MoveState CurrentState { get; private set; } = MoveState.Idle;

	// timers
	private float attackTimer = 0;
	private float chargeTimer = 0;
	private float chargeDelayTimer = 0;
	private float attackCD = 1.5f;

	// enemy types, boss needs to always be at the bottom
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
		Charging,
		Dead,
		Ragdoll
	}
	
    public override void _Ready() {
		player = GetParent().GetNode<CharacterBody3D>("Player");
		Hurtbox.Connect("body_entered", Callable.From((PhysicsBody3D body) => HandleCollisionWithPlayer(body)));
		DeleteDelayTimer.Connect("timeout", Callable.From(() => SetMoveState(MoveState.Dead)));
    }

	public void SetEnemyProperties(EnemyType type) {
		// change properties
		
		if (type == EnemyType.Melee) {
			Health = 500;
			Speed = 90;
			Attack = 50;
			this.type = type;
			Model.SetColor(EnemyVisual.DuckColors.Default);

		} else if (type == EnemyType.Charger) {
			Health = 400;
			Speed = 70;
			Attack = 75;
			attackCD = 4f;
			this.type = type;
			Model.SetColor(EnemyVisual.DuckColors.Pink);

		} else if (type == EnemyType.Shooter) {
			Health = 300;
			Speed = 50;
			Attack = 30;
			attackCD = 2.5f;
			this.type = type;
			Model.SetColor(EnemyVisual.DuckColors.Blue);

		} else {
			// boss properties
			Health = 2000;
			Speed = 80;
			Attack = 100;
			attackCD = 5f;
			KnockbackStrength = 15;
			this.type = type;
			int scale = Mathf.Min(bossScale, 10);
			Scale = new Vector3(scale, scale, scale);
			Model.SetColor(EnemyVisual.DuckColors.Green);
			Model.ToggleGlow();

			// increment scale for next boss
			bossScale++;
		}
	}

    public override void _PhysicsProcess(double delta) {
		// check if enemy died
		if (Health <= 0) {
			ProcessDeath();
			return;
		} else if (Position.Y <= -30) QueueFree(); // delete if under the map
		ProcessAttack((float) delta);
        ProcessMovement((float) delta);
    }

	public void OnShot(int damage) {
		Health -= damage;
		EmitSignal(SignalName.EnemyDamaged);
	}

	public void DeleteEnemy() {
		QueueFree();
	}

	// HELPER FUNCTIONS
	private void ProcessAttack(float delta) {
		if (type == EnemyType.Melee) return; // melee enemies have no special mechanics

		if (type == EnemyType.Shooter) {
			// spawn projectile and do shooting animation
			if (attackTimer >= attackCD) {
				ShootProjectile(delta);
				EmitSignal(SignalName.EnemyShoot);
				attackTimer = 0;
			} else attackTimer += delta;

		} else if (type == EnemyType.Charger) {
			// stop movement after a delay then rapid move forward
			if (attackTimer >= attackCD) HandleCharging(delta);
			else if (processingAttack == false && IsOnFloor()) attackTimer += delta;

		} else {
			// handle boss attacks
			if (attackTimer >= attackCD) {
				if (processingAttack == false && GD.Randi() % 2 == 0) {
					ShootProjectile(delta);
					EmitSignal(SignalName.EnemyShoot);
					attackTimer = 0;

				} else HandleCharging(delta);
			} else if (processingAttack == false) attackTimer += delta;
		}
	}

	private void HandleCharging(float delta) {
		processingAttack = true;

		// stop enemy movement
		if (chargeDelayTimer <= chargeDelay) {
			chargeDelayTimer += delta;
			SetMoveState(MoveState.Idle);
			FacePlayer();
			Velocity = new();
			return;
		}

		// begin charging
		if (chargeTimer == 0) {
			if (type != EnemyType.Boss) Model.ToggleGlow();
			Attack = 100;

			// disable collision with other enemies
			SetCollisionMaskValue(3, false);
			SetCollisionLayerValue(3, false);

			Velocity = Position.DirectionTo(player.Position).Normalized();
			Velocity *= Speed * delta * 50;
			SetMoveState(MoveState.Charging);
		}

		if (chargeTimer >= chargeTime) {
			ResetChargeState();
			return;
		}
		chargeTimer += delta;
	}

	private void ResetChargeState() {
		if (type != EnemyType.Boss) Model.ToggleGlow();

		// re-enable collision with other enemies
		SetCollisionLayerValue(3, true);
		SetCollisionMaskValue(3, true);
		
		processingAttack = false;
		chargeDelayTimer = 0;
		chargeTimer = 0;
		attackTimer = 0;
		Attack = 0;
		SetMoveState(MoveState.Idle);
	}

	private void HandleCollisionWithPlayer(PhysicsBody3D body) {
		if (body is not CharacterBody3D) return;
		if (body.Name != "Player") return;
		if (CurrentState == MoveState.Charging) ResetChargeState();
		else EmitSignal(SignalName.EnemyShoot);
	}

	private void ShootProjectile(float delta) {
		Projectile projectile = Projectile.Instantiate<Projectile>();
		projectile.Damage = Attack;
		projectile.Position = ProjectileSpawnPoint.GlobalPosition;
		if (type == EnemyType.Boss)
			projectile.LinearVelocity = Position.DirectionTo(player.Position) + Velocity * delta;
		else 
			projectile.LinearVelocity = Position.DirectionTo(player.Position) + Velocity * projectileSpeed * delta;

		// make projectile bigger if enemy is boss
		if (type == EnemyType.Boss) projectile.Scale *= bossScale;
		(Engine.GetMainLoop() as SceneTree).Root.AddChild(projectile);
	}

	private void ProcessMovement(float delta) {
		float Ycomponent;
		if (processingAttack) {
			Ycomponent = 0;
		} else if (!IsOnFloor()) {
			Ycomponent = FallSpeed * delta;
			SetMoveState(MoveState.Falling);
		} else {
			Ycomponent = 0;
			SetMoveState(MoveState.Walking);
		}

		if (!processingAttack) {
			FacePlayer();
			Vector3 playerPosition = player.Position;
			direction = Position.DirectionTo(playerPosition);
			Vector3 velocity = Velocity.Lerp(direction * Speed, 10);
			float Xcomponent = velocity.X * delta;
			float Zcomponent = velocity.Z * delta;
			Velocity = new(Xcomponent, Ycomponent, Zcomponent);
		}

		if (Velocity.IsZeroApprox() && !processingAttack) SetMoveState(MoveState.Idle);
		MoveAndSlide();
	}

	private void FacePlayer() {
		// TODO: FIX NativeCalls.cs:6354 @ void Godot.NativeCalls.godot_icall_3_706(nint, nint, Godot.Vector3*, Godot.Vector3*, Godot.NativeInterop.godot_bool): The target vector and up vector can't be parallel to each other.
		LookAt(player.Position);
		Rotation = new Vector3(0, Rotation.Y, 0); // rotate only left or right
	}

	private void ProcessDeath() {
		if (CurrentState == MoveState.Ragdoll || CurrentState == MoveState.Dead) return;
		SetMoveState(MoveState.Ragdoll);
		Ragdoll.ProcessMode = ProcessModeEnum.Inherit;
		Ragdoll.LinearVelocity = Velocity;
		Ragdoll.AngularVelocity = Velocity * Vector3.Back;

		// determine score amount
		int score;
		if (type == EnemyType.Charger) score = 20;
		else if (type == EnemyType.Shooter) score = 25;
		else if (type == EnemyType.Boss) score = 50;
		else score = 10; // melee enemy score
		
		// TODO: FIX BUG WHERE BOSS KILLED REVERTS BACK TO TINY SIZE
		EmitSignal(SignalName.EnemyDied, score);
		DeleteDelayTimer.Start();
	}

	private void SetMoveState(MoveState state) {
		if (CurrentState != state) {
			CurrentState = state;
			EmitSignal(SignalName.MoveStateChange);
		}
	}
}
