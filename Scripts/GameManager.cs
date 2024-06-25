using Godot;
using System;

public partial class GameManager : Node {
	// exported variables
	[Export] public PackedScene EnemyScene;
	// [Export] public PackedScene ProjectileScene;
	[Export] public Timer EnemySpawnTimer;
	[Export] public Timer BossSpawnTimer;

	// signals

	// instance variables
	private Player player;
	public bool GameRunning { get; private set; } = false;

	// internal timers

	
	public override void _Ready() {
		player = GetNode<Player>("Player");

		// connect signals
		EnemySpawnTimer.Connect("timeout", Callable.From(() => OnEnemySpawn()));
		BossSpawnTimer.Connect("timeout", Callable.From(() => OnBossSpawn()));
	}

	
	public override void _Process(double delta) {
		
	}

	private void OnEnemySpawn() {
		if (!GameRunning) return;
		Enemy enemy = EnemyScene.Instantiate<Enemy>();
		Enemy.EnemyType[] enemyTypes = (Enemy.EnemyType[]) Enum.GetValues(typeof(Enemy.EnemyType));
		Enemy.EnemyType type = enemyTypes[enemyTypes.Length - 1]; // assumes boss is at bottom of enum
		enemy.SetEnemyProperties(type);
		AddChild(enemy);
	}

	private void OnBossSpawn() {
		if (!GameRunning) return;
		Enemy enemy = EnemyScene.Instantiate<Enemy>();
		enemy.SetEnemyProperties(Enemy.EnemyType.Boss);
		AddChild(enemy);
	}

	private Vector3 GenerateSpawnPoint() {
		// will implement after shooting is done
		return new Vector3(GD.RandRange(-90,90), GD.RandRange(-90,90), GD.RandRange(-90,90));
	}

	private void StartGame() {
		EnemySpawnTimer.Start();
		BossSpawnTimer.Start();
	}

	private void EndGame() {
		EnemySpawnTimer.Stop();
		BossSpawnTimer.Stop();
	}
}
