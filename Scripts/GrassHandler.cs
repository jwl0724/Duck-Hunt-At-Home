using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GrassHandler : Node3D {
	[Export] Player Player;
	ShaderMaterial GrassMaterial = null;
	public override void _Ready() {
		foreach(MeshInstance3D chunk in GetChildren().Cast<MeshInstance3D>()) {
			// stop rendering grass if not on screen
			MultiMeshInstance3D grass = chunk.GetNode<MultiMeshInstance3D>("Grass");
			VisibleOnScreenNotifier3D visibility = chunk.GetNode<VisibleOnScreenNotifier3D>("VisibilityNotifier");
	
			visibility.Connect("screen_entered", Callable.From(() => grass.Visible = true));
			visibility.Connect("screen_exited", Callable.From(() => grass.Visible = false));

			// get the material to set player coordinates
			GrassMaterial ??= grass.MaterialOverride as ShaderMaterial;
		}
	}

	public override void _Process(double delta) {
		GrassMaterial.SetShaderParameter("player_position", Player.Position + new Vector3(0, -1.5f, 0));
	}
}
