using Godot;
using System;

public partial class Portrait : AnimatedSprite2D {
	// exported variables
	[Export] Player Player;
	
	// timers
	private float idleTimerCD = 5;
	private float idleTimer = 0;

	public override void _Ready() {
		Player.Connect("PlayerDamaged", Callable.From(() => OnPlayerDamaged()));
	}

	public override void _Process(double delta) {
		// every 5 seconds, have a 50% chance of playing the idle animation
		if (idleTimer > idleTimerCD) {
			if (GD.Randi() % 2 == 0) {
				if (Player.Health < Player.MaxHealth / 2) Play("lowHealthIdle");
				else Play("idle");
			}
			idleTimer = 0;
		} else idleTimer += (float) delta;
	}

	private void OnPlayerDamaged() {
		if (Player.Health < Player.MaxHealth / 2) Play("lowHealthDamaged");
		else Play("damaged");
	}
}
