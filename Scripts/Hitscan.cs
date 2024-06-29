using Godot;
using System;

public partial class Hitscan : RayCast3D {
	// exported variables
	[Export] public Player Player;

	// static variables
	private static readonly int grappleSpeed = 2000;

	public override void _Ready() {
		Player.Connect("PlayerShoot", Callable.From(() => OnPlayerShoot()));
		Player.GrappleGunModel.Connect("GrappleGunLifted", Callable.From(() => OnGrappleGunLifted()));
	}

	private void OnPlayerShoot() {
		var body = GetCollider();
		if (body is Projectile projectile) projectile.PopBubble();
		else if (body is Enemy enemy) enemy.OnShot(Player.Attack);
	}

	private void OnGrappleGunLifted() {
		var body = GetCollider();
		if (body is StaticBody3D) 
			Player.Velocity = Player.Position.DirectionTo(GetCollisionPoint()) * grappleSpeed * (float) GetPhysicsProcessDeltaTime();
	}
}
