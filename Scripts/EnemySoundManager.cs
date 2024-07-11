using Godot;
using System;

public partial class EnemySoundManager : Node3D {
	// exported variables
	[Export] Enemy Enemy;
	[Export] SoundCollection SoundCollection;

	// internal timers
	private const int QuackCD = 5;
	private float QuackTimer = 0;

	public override void _Ready() {
		Enemy.AttackHandler.Connect("EnemyShoot", Callable.From(() => SoundCollection.Play("Shoot")));
		Enemy.Connect("EnemyDied", Callable.From((int score) => SoundCollection.Play("Death")));
		Enemy.Connect("EnemyDamaged", Callable.From(() => OnDamaged()));
		Enemy.Connect("MoveStateChange", Callable.From(() => OnMoveStateChange()));
	}

	public override void _Process(double delta) {
		if (QuackTimer >= QuackCD) {
			QuackTimer = 0;
			if (GD.Randi() % 3 != 2) return; // 1 in 3 to quack
			SoundCollection.Play($"Ambience{GD.Randi() % 4 + 1}");
		} else QuackTimer += (float) delta;
	}

	private void OnDamaged() {
		if (GD.Randi() % 8 != 5) return; // 1 in 8 chance to play sound
		SoundCollection.Play("Damaged", overlap: false);
	}

	private void OnMoveStateChange() {
		if (Enemy.CurrentState == Enemy.MoveState.Charging) SoundCollection.Play("Charge");
		else SoundCollection.Stop("Charge");
	}
}
