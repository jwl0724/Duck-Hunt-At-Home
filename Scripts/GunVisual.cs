using Godot;
using System;

public partial class GunVisual : Node3D {
	// exported variables
	[Export] public Player Player;
	[Export] public AnimationPlayer Animator;
	[Export] public Hitscan HitscanLine;
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
		Node3D trailWrapper = new(); // need parent to rotate animation
		trailWrapper.AddChild(trailModel);
		(Engine.GetMainLoop() as SceneTree).Root.AddChild(trailWrapper);

		// position model
		MeshInstance3D barrel = GetNode<MeshInstance3D>("Barrel");
		trailWrapper.Position = barrel.GlobalPosition; // trail wrapper at root
		if (HitscanLine.IsColliding()) {
			trailWrapper.LookAt(HitscanLine.GetCollisionPoint());

		} else {
			Node3D lineEndPoint = HitscanLine.GetNode<Node3D>("HitscanEndPoint");
			trailWrapper.LookAt(lineEndPoint.GlobalPosition);
		}
		trailModel.Rotate(Vector3.Up, Mathf.DegToRad(-90));
	}
}