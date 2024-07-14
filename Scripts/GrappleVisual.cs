using Godot;
using System;

public partial class GrappleVisual : Node3D {
	// exported variables
	[Export] public Player Player;
	[Export] public AnimationPlayer Animator;
	[Export] public Node3D LineSpawnPoint;
	[Export] public ImmediateMesh LineMesh;

	// signals
	[Signal] public delegate void GrappleGunLiftedEventHandler();

	// instance variables
	private StandardMaterial3D lineMaterial;
	private Node3D lineTriangleLeftSide;
	private Node3D lineTriangleRightSide;

	public override void _Ready() {
		Visible = false;
		Animator.Play("holster"); // hide grapple when first loading in

		// connect signals
		Player.InputManager.Connect("PlayerGrapple", Callable.From(() => OnPlayerGrapple()));
		Player.InputManager.Connect("PlayerGrappleReleased", Callable.From(() => OnGrappleRelease()));
		Animator.Connect("animation_finished", Callable.From((StringName name) => OnAnimationFinished(name)));
		
		// get left and right sides of line for rendering
		lineTriangleLeftSide = LineSpawnPoint.GetNode<Node3D>("LeftSide");
		lineTriangleRightSide = LineSpawnPoint.GetNode<Node3D>("RightSide");

		// create grapple line mesh instance and material
		CreateLineMaterial();
        MeshInstance3D meshInstance = new() {
            Mesh = LineMesh,
            MaterialOverride = lineMaterial,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
        };
		(Engine.GetMainLoop() as SceneTree).Root.CallDeferred(MethodName.AddChild, meshInstance);
	}

    public override void _Process(double delta) {
        if (!Player.GrapplePoint.IsZeroApprox()) RenderGrappleLine();
		else LineMesh.ClearSurfaces();
    }

    private void OnPlayerGrapple() {
		if (!Visible) Visible = true;
		Animator.Play("lift");
		
	}

	private void OnGrappleRelease() {
		Animator.Play("holster");
		LineMesh.ClearSurfaces();
	}

	private void OnAnimationFinished(StringName animationName) {
		if (animationName == "lift") EmitSignal(SignalName.GrappleGunLifted);
		else LineMesh.ClearSurfaces();
	}

	private void CreateLineMaterial() {
		lineMaterial = new () {
			AlbedoColor = new Color(0.43f, 0.43f, 0.43f, 1),
			Metallic = 1, MetallicSpecular = 1,
			RimEnabled = true, RimTint = 1,
			ClearcoatEnabled = true, ClearcoatRoughness = 1,
		};
	}

	private void RenderGrappleLine() {
		LineMesh.ClearSurfaces(); // clear previous mesh
		LineMesh.SurfaceBegin(Mesh.PrimitiveType.TriangleStrip); // start drawing

		// add vertices to form triangle
		LineMesh.SurfaceAddVertex(lineTriangleLeftSide.GlobalPosition);
		LineMesh.SurfaceAddVertex(Player.GrapplePoint);
		LineMesh.SurfaceAddVertex(lineTriangleRightSide.GlobalPosition);
		
		// end drawing
		LineMesh.SurfaceEnd();
	}
}
