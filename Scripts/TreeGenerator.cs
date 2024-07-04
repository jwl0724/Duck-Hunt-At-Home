using Godot;
using System;

public partial class TreeGenerator : StaticBody3D {
	// exported variables
	[Export] private StandardMaterial3D LeavesMaterial;

	private StandardMaterial3D leavesMaterialCopy;

	public void SetUpMaterial() {
		// duplicate material so they dont share the same reference
		StandardMaterial3D copy = LeavesMaterial.Duplicate() as StandardMaterial3D;
		MeshInstance3D topMesh = GetNode<CollisionShape3D>("TopLeaves").GetChild(0) as MeshInstance3D;
		MeshInstance3D botMesh = GetNode<CollisionShape3D>("BottomLeaves").GetChild(0) as MeshInstance3D;
		topMesh.SetSurfaceOverrideMaterial(0, copy);
		botMesh.SetSurfaceOverrideMaterial(0, copy);
		leavesMaterialCopy = copy;
	}

	public void SetColor(Color color) {
		leavesMaterialCopy.AlbedoColor = color;
	}

	public void SetHeight(float height) {
		Scale = new Vector3(Scale.X, height, Scale.Z);
	}

	public void SetRadius(float radius) {
		Scale = new Vector3(radius, Scale.Y, radius);
	}
}
