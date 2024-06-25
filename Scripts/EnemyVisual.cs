using Godot;
using System;

public partial class EnemyVisual : Node {
	[Export] public Enemy enemy;
	private AnimationPlayer animator;

	public override void _Ready() {
		animator = GetNode<AnimationPlayer>("Animation");
		enemy.Connect("EnemyShoot", Callable.From(() => animator.Play("shoot")));
		enemy.Connect("MoveStateChange", Callable.From(() => OnMoveStateChange()));
	}

	public void OnMoveStateChange() {
		// idle = don't play any animations
		animator.Stop();
		if (enemy.CurrentState == Enemy.MoveState.Walking) {
			animator.Play("walk");
		} else if (enemy.CurrentState == Enemy.MoveState.Falling) {
			animator.Play("fall");
		} else if (enemy.CurrentState == Enemy.MoveState.Charging) {
			animator.Play("charge");
		}
	}

	// This approach too specific to model, next time should use interface
	public void SetColor(Color color) {
		// get model components
		MeshInstance3D head = GetNode<MeshInstance3D>("Head");
		MeshInstance3D body = GetNode<MeshInstance3D>("Body");
		MeshInstance3D leftWing = GetNode<MeshInstance3D>("LeftWing");
		MeshInstance3D rightWing = GetNode<MeshInstance3D>("RightWing");

		// TODO: FIX WHERE CHANGING COLOR CHANGES COLOR OF ALL OBJECTS
		StandardMaterial3D material = head.Mesh.SurfaceGetMaterial(0) as StandardMaterial3D;
		StandardMaterial3D newMaterial = new(){ AlbedoColor = color };
		head.Mesh.SurfaceSetMaterial(0, newMaterial);
		body.Mesh.SurfaceSetMaterial(0, newMaterial);
		leftWing.Mesh.SurfaceSetMaterial(0, newMaterial);
		rightWing.Mesh.SurfaceSetMaterial(0, newMaterial);


		// // get material
		// StandardMaterial3D headMaterial = head.Mesh.SurfaceGetMaterial(0) as StandardMaterial3D;
		// StandardMaterial3D bodyMaterial = body.Mesh.SurfaceGetMaterial(0) as StandardMaterial3D;
		// StandardMaterial3D leftWingMaterial = leftWing.Mesh.SurfaceGetMaterial(0) as StandardMaterial3D;
		// StandardMaterial3D rightWingMaterial = rightWing.Mesh.SurfaceGetMaterial(0) as StandardMaterial3D;

		// // set color
		// headMaterial.AlbedoColor = color;
		// bodyMaterial.AlbedoColor = color;
		// leftWingMaterial.AlbedoColor = color;
		// rightWingMaterial.AlbedoColor = color;
	}

	public void ToggleGlow(bool glow) {
		foreach (var child in GetChildren()) {
			if (child is not MeshInstance3D meshInstance) return;
			if (meshInstance.Mesh.SurfaceGetMaterial(0) is not StandardMaterial3D material) return;
			Color originalColor = material.AlbedoColor;
			if (glow)
				material.AlbedoColor = new Color(originalColor.R * 2, originalColor.G * 2, originalColor.B * 2, 1);
			else 
				material.AlbedoColor = new Color(originalColor.R / 2, originalColor.G / 2, originalColor.B / 2, 1);
		}
	}
}
