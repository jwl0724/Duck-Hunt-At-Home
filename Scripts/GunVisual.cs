using Godot;
using System;

public partial class GunVisual : Node3D {
	// exported variables
	[Export] public Player Player;
	[Export] public AnimationPlayer Animator;
	[Export] public PackedScene BulletTrailModel;
	
	public override void _Ready() {
		Player.Connect("PlayerShoot", Callable.From(() => OnPlayerShoot()));
		Player.Connect("PlayerReload", Callable.From(() => OnPlayerReload()));
	}

	public override void _Process(double delta) {
	}

	private void OnPlayerReload() {
		Animator.Play("reload");
	}

	private void OnPlayerShoot() {
		Animator.Play("fire");
		Node3D trailModel = BulletTrailModel.Instantiate<Node3D>();
		MeshInstance3D barrel = GetNode<MeshInstance3D>("Barrel");
		(Engine.GetMainLoop() as SceneTree).Root.AddChild(trailModel);
		trailModel.Position = barrel.GlobalPosition;
		// trailModel.Rotation = new Vector3(Player.GlobalRotation.X, Player.GlobalRotation.Y, Player.GlobalRotation.Z);
		Node3D rotationalHelper = Player.GetNode<Node3D>("RotationalHelper");
		trailModel.Rotation = new Vector3(rotationalHelper.Rotation.X, rotationalHelper.Rotation.Y + Mathf.DegToRad(-90), rotationalHelper.Rotation.Z);
	}
}