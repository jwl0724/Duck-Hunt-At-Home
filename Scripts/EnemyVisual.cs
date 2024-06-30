using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class EnemyVisual : Node {
	// exported variables
	[Export] public Enemy Enemy;

	// static variables
	private readonly static Dictionary<DuckColors, Color> availableColors = new() {
		// Default = yellow, which is the original material color
		{DuckColors.Blue, new Color(0, 0.52f, 1, 1)},
		{DuckColors.Pink, new Color(0.925f, 0.412f, 0.929f, 1)},
		{DuckColors.Green, new Color(0.067f, 0.753f, 0.071f, 1)}
	};
	private readonly StringName[] ignoredComponents = {
		"RightEye", "LeftEye", "Beak"
	};
	
	// instance variables
	private AnimationPlayer animator;
	private bool glowing = false;
	private StringName previousAnimation = null;

	// enums
	public enum DuckColors {
		Blue,
		Pink,
		Green,
		Default
	}

	public override void _Ready() {
		animator = GetNode<AnimationPlayer>("Animation");

		// connect signals
		Enemy.Connect("EnemyShoot", Callable.From(() => animator.Play("shoot")));
		Enemy.Connect("MoveStateChange", Callable.From(() => OnMoveStateChange()));
		animator.Connect("animation_finished", Callable.From((StringName name) => OnAnimationFinished(name)));
	}

	private void OnAnimationFinished(StringName name) {
		if (name == "dead") Enemy.DeleteEnemy();
		else OnMoveStateChange();
	}

	private void OnMoveStateChange() {
		// idle or ragdoll = don't play any animations
		animator.Stop();
		if (Enemy.CurrentState == Enemy.MoveState.Walking) {
			animator.Play("walk");
		} else if (Enemy.CurrentState == Enemy.MoveState.Falling) {
			animator.Play("fall");
		} else if (Enemy.CurrentState == Enemy.MoveState.Charging) {
			animator.Play("charge");
		} else if (Enemy.CurrentState == Enemy.MoveState.Dead) {
			animator.Play("dead");
		}
	}

	// This approach too specific to model, next time should use interface
	public void SetColor(DuckColors color) {
		// dupe materials when spawning to prevent sharing same material
		foreach (var child in GetChildren()) {
			if (child is not MeshInstance3D meshInstance) continue;

			Resource originalMaterial = meshInstance.Mesh.SurfaceGetMaterial(0);
			StandardMaterial3D newMaterial = originalMaterial.Duplicate() as StandardMaterial3D;
			meshInstance.SetSurfaceOverrideMaterial(0, newMaterial);
			
			if (color == DuckColors.Default || ignoredComponents.Contains(meshInstance.Name)) continue;
			newMaterial.AlbedoColor = availableColors[color];
		}
	}

	public void ToggleGlow() {
		foreach (var child in GetChildren()) {
			if (child is not MeshInstance3D meshInstance) continue;
			StandardMaterial3D material = meshInstance.GetSurfaceOverrideMaterial(0) as StandardMaterial3D;
			if (glowing) material.AlbedoColor /= 2;
			else material.AlbedoColor *= 2;
		}
		// set the glowing state after editing
		if (glowing) glowing = false;
		else glowing = true;
	}
}
