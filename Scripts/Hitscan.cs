using Godot;
using System;

public partial class Hitscan : RayCast3D {
	// exported variables
	[Export] public Player Player;

	public override void _Ready() {
		Player.Connect("PlayerShoot", Callable.From(() => OnPlayerShoot()));
	}

	private void OnPlayerShoot() {
		var body = GetCollider();
		if (body is Projectile projectile) projectile.PopBubble();
		else if (body is Enemy enemy) enemy.OnShot(Player.Attack);
	}
}
