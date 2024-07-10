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
		if (body is not Player player) return;
		if (player.Grappled && !player.GrapplePoint.IsZeroApprox()) {
			// handle when player grapples outside barrier
			player.Position = player.Position.Lerp(Vector3.Zero, 0.1f);
			return;
		}
		Vector3 force = new(-player.Velocity.X, 4, -player.Velocity.Z);
		force = force * 2 / (float) GetProcessDeltaTime();
		player.ApplyForce(force);
	}
}
