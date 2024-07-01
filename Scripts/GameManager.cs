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
	public int Score { get; private set; } = 0;

	// internal timers
	public float TimeElapsed { get; private set; } = 0;

	
	public override void _Ready() {
		player = GetNode<Player>("Player");

		// connect signals
		EnemySpawnTimer.Connect("timeout", Callable.From(() => OnEnemySpawn()));
		BossSpawnTimer.Connect("timeout", Callable.From(() => OnBossSpawn()));

		StartGame();
	}

	
	public override void _Process(double delta) {
		if (player.Health <= 0) return;
		TimeElapsed += (float) delta;
		player.HUD.UpdateScore(Score);
		player.HUD.UpdateTime(TimeElapsed);
	}

	

	private void OnEnemySpawn() {
		if (!GameRunning) return;
		Enemy enemy = EnemyScene.Instantiate<Enemy>();
		Enemy.EnemyType[] enemyTypes = (Enemy.EnemyType[]) Enum.GetValues(typeof(Enemy.EnemyType));
		Enemy.EnemyType type = enemyTypes[GD.Randi() % (enemyTypes.Length - 1)]; // assumes boss is at bottom of enum
		enemy.SetEnemyProperties(type);
		enemy.Position = GenerateSpawnPoint();
		AddChild(enemy);
		enemy.Connect("EnemyDied", Callable.From((int score) => Score += score));
	}

	private void OnBossSpawn() {
		if (!GameRunning) return;
		Enemy enemy = EnemyScene.Instantiate<Enemy>();
		enemy.SetEnemyProperties(Enemy.EnemyType.Boss);
		enemy.Position = GenerateSpawnPoint();
		AddChild(enemy);
	}

	private Vector3 GenerateSpawnPoint() {
		// will implement after shooting is done
		return new Vector3(GD.RandRange(-25, 25), GD.RandRange(0, 15), GD.RandRange(-25, 25));
	}

	private void StartGame() {
		EnemySpawnTimer.Start();
		BossSpawnTimer.Start();

		GameRunning = true;
	}

	private void EndGame() {
		EnemySpawnTimer.Stop();
		BossSpawnTimer.Stop();

		GameRunning = false;
	}
}
