using Godot;
using System;

public partial class CityLevelHandler : Node {
	// exported variables
	[Export] public Area3D Barrier;
	[Export] public SphereMesh BarrierMesh;

	// instance variables
	private Vector3 mapSize = new(400, 140, 400);
	private ShaderMaterial shader;

	public override void _Ready() {
		// MapSize = (GetParent() as GameManager).mapSize;
		shader = BarrierMesh.SurfaceGetMaterial(0) as ShaderMaterial;
		Barrier.Connect("body_exited", Callable.From((PhysicsBody3D body) => OnBarrierExit(body)));
	}

	private void OnBarrierExit(PhysicsBody3D body) {
		if (body is not CharacterBody3D characterBody) return;
		
		// calculate angle
		Vector2 XZCoords = new(characterBody.Position.X, characterBody.Position.Z);
		float angle = XZCoords.DirectionTo(Vector2.Zero).Angle();

		// move body back based on angle
		characterBody.Position = new Vector3(
			-Mathf.Cos(angle) * (BarrierMesh.Radius - 2),
			characterBody.Position.Y,
			-Mathf.Sin(angle) * (BarrierMesh.Radius - 2)
		);
	}
}
