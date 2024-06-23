using Godot;
using System;

public partial class Enemy : CharacterBody3D {
	[Export] public float Gravity = -5f;
	
	// signals
	[Signal] public delegate void EnemyShootEventHandler();
	[Signal] public delegate void EnemyDiedEventHandler();
	
	// instance variables
	public int Health { get; private set; }
	public int Attack { get; private set; }
	public float Speed { get; private set; }
	private Vector3 direction = new();
	private EnemyType type;

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

    public override void _Ready() {
        // Add stuff later depending on whats needed later
    }

	public void SetEnemyProperties(EnemyType type) {
		if (type == EnemyType.Melee) {
			
		} else if (type == EnemyType.Charger) {

		} else if (type == EnemyType.Shooter) {

		} else {

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

	private void ProcessAttack(float delta) {
		if (attackTimer >= attackCD) 
			EmitSignal(SignalName.EnemyShoot);
		else attackTimer += delta;
	}

	private void ProcessMovement(float delta) {
		MoveAndSlide();
	}

	private void ProcessDeath() {
		// determine score amount
		int score;
		if (type == EnemyType.Charger) score = 20;
		else if (type == EnemyType.Shooter) score = 25;
		else if (type == EnemyType.Boss) score = 50;
		else score = 10;

		EmitSignal(SignalName.EnemyDied, score);
		QueueFree();
	}
}
