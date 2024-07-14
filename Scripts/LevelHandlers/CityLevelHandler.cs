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
		Vector3 force = player.Position.DirectionTo(Vector3.Zero) * CalculateKnockback(player.Velocity);
		player.ApplyForce(force);
	}

	private float CalculateKnockback(Vector3 velocity) {
		float highestAxis = Math.Max(Math.Abs(velocity.X), Math.Max(Math.Abs(velocity.Y), Math.Abs(velocity.Z)));
		if (highestAxis <= 30) return 30;
		else return highestAxis;
	}
}
