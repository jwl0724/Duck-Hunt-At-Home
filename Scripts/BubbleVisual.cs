using Godot;
using System;

public partial class BubbleVisual : MeshInstance3D {
	// exported variables
	[Export] public RigidBody3D Body;

	// static variables
	private static float originalHeight = 0;

	// instance variables
	private SphereMesh model;
	private float lastFrameVelocity = 0;
	public override void _Ready() {		
		// duplicate mesh so bubbles dont share same reference
		Mesh = Mesh.Duplicate() as SphereMesh;
		model = Mesh as SphereMesh;
		if (originalHeight == 0) originalHeight = model.Height;
	}

	public override void _Process(double delta) {
		if (!Visible) return;
		model.Height = originalHeight * Mathf.Clamp(1 - Body.LinearVelocity.Y * 0.1f, 0.9f, 1.5f);
		lastFrameVelocity = Body.LinearVelocity.Y;
	}
}
