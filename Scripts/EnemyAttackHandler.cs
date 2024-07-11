using Godot;
using System;

public partial class EnemyAttackHandler : Node3D {
	// exported variables
	[Export] public PackedScene Projectile;
	[Export] public Node3D ProjectileSpawnPoint;

	// signals
	[Signal] public delegate void EnemyShootEventHandler();

	// class constants
	private static readonly int projectileSpeed = 2500;

	// instance variables
	private Enemy enemy;
	private Player player;
	private Timer attackTimer;
	private Timer meleeTimer;
	private float attackCD = 0;

	// internal timer
	private float movementPauseTimer = 0;

	public override void _Ready() {
		enemy = GetParent() as Enemy;
		player = enemy.GetParent().GetNode<Player>("Player");

		attackTimer = GetNode<Timer>("AttackTimer");
		if (attackCD != 0) attackTimer.WaitTime = attackCD;
		meleeTimer = GetNode<Timer>("MeleeTimer");

		attackTimer.Connect("timeout", Callable.From(() => OnAttackTimeout()));
		meleeTimer.Connect("timeout", Callable.From(() => OnMeleeTimeout()));

		attackTimer.Start();
	}

	public override void _PhysicsProcess(double delta) {
		KinematicCollision3D lastCollision = enemy.GetLastSlideCollision();
		if (lastCollision == null) return;
		if (lastCollision.GetCollider() is Player player) HandleCollisionWithPlayer(player);
	}

	private void HandleCollisionWithPlayer(Player player) {
		if (meleeTimer.TimeLeft != 0) return;
		attackTimer.Stop();
		meleeTimer.Start();
		if ((enemy.Type == Enemy.EnemyType.Charger || enemy.Type == Enemy.EnemyType.Boss) && enemy.CurrentState == Enemy.MoveState.Charging) 
			ResetChargeState();

		enemy.SetMoveState(Enemy.MoveState.Idle);
		EmitSignal(SignalName.EnemyShoot);
	}

	public void SetAttackCD(float time) {
		attackCD = time;
	}

	private void OnMeleeTimeout() {
		if (enemy.Type != Enemy.EnemyType.Melee) attackTimer.Start();
		SetMovement();
	}

	private void OnAttackTimeout() {
		if (enemy.Health <= 0) {
			attackTimer.Stop();
			meleeTimer.Stop();
			return;
		}
		if (enemy.Type == Enemy.EnemyType.Melee) return;

		if (enemy.Type == Enemy.EnemyType.Shooter) ProcessShoot();
		else if (enemy.Type == Enemy.EnemyType.Charger) ProcessCharge();
		else {
			uint randomNumber;
			if (enemy.CurrentState == Enemy.MoveState.Walking || enemy.CurrentState == Enemy.MoveState.Falling)
				randomNumber = GD.Randi() % 2;
			else randomNumber = 0;
			
			// 1 = shoot, 0 = charge
			if (randomNumber == 1) ProcessShoot();
			else ProcessCharge();
		}
	}
	
	private void ProcessShoot() {
		ShootProjectile();
		EmitSignal(SignalName.EnemyShoot);
	}

	private void ShootProjectile() {
		Projectile projectile = Projectile.Instantiate<Projectile>();
		projectile.Damage = enemy.Attack;
		projectile.Position = ProjectileSpawnPoint.GlobalPosition;
		projectile.LinearVelocity = enemy.Position.DirectionTo(player.Position) * (float) GetProcessDeltaTime() * projectileSpeed;
		if (enemy.Type == Enemy.EnemyType.Boss) projectile.LinearVelocity /= 3;

		// make projectile bigger if enemy is boss
		if (enemy.Type == Enemy.EnemyType.Boss) {
			Vector3 enemyScale = enemy.GetNode<CollisionShape3D>("Shape").Scale;
			projectile.GetNode<CollisionShape3D>("Shape").Scale = enemyScale * 1.2f;
			projectile.GetNode<MeshInstance3D>("Model").Scale = enemyScale * 1.2f;
			projectile.GetNode<Timer>("Lifespan").WaitTime = 2.5f;
		}
		(Engine.GetMainLoop() as SceneTree).Root.AddChild(projectile);
	}

	private void ProcessCharge() {
		if (enemy.CurrentState == Enemy.MoveState.Walking || enemy.CurrentState == Enemy.MoveState.Falling) {
			enemy.SetMoveState(Enemy.MoveState.WindUp);

		} else if (enemy.CurrentState == Enemy.MoveState.WindUp) {
			SetChargeState();
			enemy.SetMoveState(Enemy.MoveState.Charging);

		} else if (enemy.CurrentState == Enemy.MoveState.Charging) {
			ResetChargeState();
			SetMovement();
		}
	}

	private void SetChargeState() {
		if (enemy.Type == Enemy.EnemyType.Charger) enemy.Model.ToggleGlow();
		enemy.Direction = enemy.Position.DirectionTo(player.Position).Normalized();
		enemy.SetCollisionMaskValue(3, false); // disable collision with other enemies
	}

	private void ResetChargeState() {
		if (enemy.Type == Enemy.EnemyType.Charger) enemy.Model.ToggleGlow();
		enemy.SetCollisionMaskValue(3, true);
	}

	private void SetMovement() {
		if (enemy.Health <= 0) return;
		if (enemy.IsOnFloor()) enemy.SetMoveState(Enemy.MoveState.Walking);
		else enemy.SetMoveState(Enemy.MoveState.Falling);
	}
}