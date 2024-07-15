using Godot;
using System;

public partial class GameManager : Node {
	// exported variables
	[Export] public Vector3 MapSize;
	[Export] public PackedScene EnemyScene;
	[Export] public Timer EnemySpawnTimer;
	[Export] public Timer BossSpawnTimer;
	[Export] public SoundCollection MusicCollection;

	// instance variables
	private Player player;
	public bool GameRunning { get; private set; } = false;
	public bool AlwaysGlow { get; set; } = false;
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
		if (!GameRunning) {
			ProcessGameOverScreen();
			return;

		} else if (player.Health <= 0) {
			EndGame();
			return;
		}

		TimeElapsed += (float) delta;
		player.HUD.UpdateScore(Score);
		player.HUD.UpdateTime(TimeElapsed);
	}

	private void ProcessGameOverScreen() {
		if (!MusicCollection.IsPlaying("GameOverJingle")) MusicCollection.Play("GameOver");
	}

	private void OnEnemySpawn() {
		if (!GameRunning) return;
		Enemy enemy = EnemyScene.Instantiate<Enemy>();
		Enemy.EnemyType[] enemyTypes = (Enemy.EnemyType[]) Enum.GetValues(typeof(Enemy.EnemyType));
		Enemy.EnemyType type = enemyTypes[GD.Randi() % (enemyTypes.Length - 1)]; // assumes boss is at bottom of enum
		enemy.SetEnemyProperties(type, AlwaysGlow);
		enemy.Position = GenerateSpawnPoint();
		AddChild(enemy);
		enemy.Connect("EnemyDied", Callable.From((int score) => Score += score));
	}

	private void OnBossSpawn() {
		if (!GameRunning) return;
		Enemy enemy = EnemyScene.Instantiate<Enemy>();
		enemy.SetEnemyProperties(Enemy.EnemyType.Boss, AlwaysGlow);
		enemy.Position = GenerateSpawnPoint();
		AddChild(enemy);
	}

	private Vector3 GenerateSpawnPoint() {
		const int minSpawnRadius = 25;
		float spawnAngle = Mathf.DegToRad(GD.Randi() % 361);
		float spawnDistance = GD.RandRange(minSpawnRadius, minSpawnRadius * 2);
		
		return new Vector3(
			Mathf.Clamp(spawnDistance * Mathf.Cos(spawnAngle), -MapSize.X / 2, MapSize.X / 2),
			Mathf.Clamp(GD.Randi() % (minSpawnRadius + 1), 0, MapSize.Y),
			Mathf.Clamp(spawnDistance * Mathf.Sin(spawnAngle), -MapSize.Z / 2, MapSize.Z / 2)
		);
	}

	private void StartGame() {
		EnemySpawnTimer.Start();
		BossSpawnTimer.Start();
		
		MusicCollection.Stop("GameOverJingle");
		MusicCollection.Stop("GameOver");
		MusicCollection.Play("InGame");

		GameRunning = true;
	}

	private void EndGame() {
		EnemySpawnTimer.Stop();
		BossSpawnTimer.Stop();
		
		MusicCollection.Stop("InGame");
		MusicCollection.Play("GameOverJingle");

		GameRunning = false;
	}
}
