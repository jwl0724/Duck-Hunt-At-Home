using Godot;
using System;

public partial class Hitscan : RayCast3D {
	// exported variables
	[Export] public Player Player;

	// signals
	[Signal] public delegate void ShotLandedEventHandler();

	public override void _Ready() {
		Player.InputManager.Connect("PlayerShoot", Callable.From(() => OnPlayerShoot()));
		Player.InputManager.Connect("PlayerGrapple", Callable.From(() => OnGrappleFire()));
	}

	private void OnPlayerShoot() {
		var body = GetCollider();
		if (body is Projectile projectile) projectile.PopBubble();
		else if (body is Enemy enemy) {
			EmitSignal(SignalName.ShotLanded);
			enemy.OnShot(Player.Attack);
		}
	}

	private void OnGrappleFire() {
		var body = GetCollider();
		if (body is StaticBody3D) Player.GrapplePoint = GetCollisionPoint();
	}
}
