using Godot;
using System;

public partial class PlainsLevelHandler : Node3D {
	// exported variables
	[Export] public Player Player;
	[Export] public PackedScene Item;
	[Export] public DirectionalLight3D Sun;
	[Export] public int MaxItems;
	[Export] public Color[] ColorOptions;

	// instance variables
	private Vector3 MapSize;
	private int maxSunHeight;
	private float sunTimer;

	public override void _Ready() {
		MapSize = (GetParent() as GameManager).MapSize;
		maxSunHeight = (int) Sun.Position.Y;
		GenerateRandomTrees();
	}

	public override void _Process(double delta) {
		HandleWrapping();
		HandleSunMovement((float) delta);
	}

	private void GenerateRandomTrees() {
		(Engine.GetMainLoop() as SceneTree).CallGroup("trees", MethodName.QueueFree);
		int itemsToMake = GD.RandRange(MaxItems / 3, MaxItems);
		
		for (int i = 0; i < itemsToMake; i++) {
			TreeGenerator tree = Item.Instantiate<TreeGenerator>();
			tree.SetUpMaterial();

			// randomize tree properties
			const float maxHeight = 2, minScale = 0.5f, maxRadius = 1.5f;
			tree.SetHeight(GD.Randf() * (maxHeight - minScale) + minScale);
			tree.SetRadius(GD.Randf() * (maxRadius - minScale) + minScale);
			tree.SetColor(ColorOptions[GD.Randi() % ColorOptions.Length]);
			
			// randomize placement
			const int minSpawnRadius = 15, maxSpawnRadius = 70;
			float spawnAngle = Mathf.DegToRad(GD.Randi() % 361);
			float spawnDistance = GD.RandRange(minSpawnRadius, maxSpawnRadius);
			tree.Position = new Vector3(
				Mathf.Clamp(spawnDistance * Mathf.Cos(spawnAngle), -MapSize.X / 2, MapSize.X / 2), 3,
				Mathf.Clamp(spawnDistance * Mathf.Sin(spawnAngle), -MapSize.Z / 2, MapSize.Z / 2)
			);
			AddChild(tree);
		}
	}

	private void HandleSunMovement(float delta) {
		float Ycomponent, Zcomponent;
		sunTimer += delta;
		Ycomponent = Mathf.Sin(Mathf.DegToRad(sunTimer)) * maxSunHeight;
		Zcomponent = Mathf.Cos(Mathf.DegToRad(sunTimer)) * maxSunHeight;
		Sun.Position = Sun.Position.Lerp(new Vector3(0, Ycomponent, Zcomponent), delta * delta);
		Sun.LookAt(Vector3.Zero);
	}

	private void HandleWrapping() {
		// put player onto other side when getting too close to edge
		const float wrapFactor = 0.95f;
		bool wrapped = false;
		float Xcomponent, Zcomponent, Ycomponent = Player.Position.Y;
		if (Player.Position.X >= MapSize.X / 2 * wrapFactor) {
			Xcomponent = -Player.Position.X + Player.Velocity.X;
			wrapped = true;

		} else if (Player.Position.X <= -MapSize.X / 2 * wrapFactor) {
			Xcomponent = Player.Position.X - Player.Velocity.X;
			wrapped = true;

		} else Xcomponent = Player.Position.X;
		if (Player.Position.Z >= MapSize.Z / 2 * wrapFactor) {
			Zcomponent = -Player.Position.Z + Player.Velocity.Z;
			wrapped = true;

		} else if (Player.Position.Z <= -MapSize.Z / 2 * wrapFactor) {
			Zcomponent = Player.Position.Z - Player.Velocity.Z;
			wrapped = true;

		} else Zcomponent = Player.Position.Z;

		if (wrapped) {
			// fix bug where it can warp into a tree
			Player.Position = new Vector3(Xcomponent, Ycomponent, Zcomponent);
			Player.GetNode<Node3D>("RotationalHelper").Rotate(Vector3.Up, Mathf.Pi);
			Player.Velocity *= -1;
		} 
	}
}
