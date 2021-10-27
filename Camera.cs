using Godot;
using System;

public class Camera : Godot.Camera
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
	{
		
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey key)
		{
			Move(key);
			Rotate(key);

		}

		if (@event is InputEventMouseButton mouseButton)
		{
			Raycast(mouseButton);
		}
		
		base._Input(@event);
	}

	private void Raycast(InputEventMouseButton mouseButton)
	{
		// Right click?
		if ((ButtonList)mouseButton.ButtonIndex == ButtonList.Right)
		{
			var spaceState = GetWorld().DirectSpaceState;
			var viewport = GetViewport();

			var from = ProjectRayOrigin(mouseButton.Position);
			var to = from + ProjectRayNormal(mouseButton.Position) * 100f;

			var result = spaceState.IntersectRay(from, to);

			if (result.Count > 0)
			{
				var collider = (Node)result["collider"];
				Console.WriteLine(collider.Name);
			}
		}
	}

	private void Move(InputEventKey key)
	{
		var tween = (Tween) GetNode("Tween");

		if (tween.IsActive())
		{
			return;
		}
		
		var forward = Transform.basis.z.Normalized();
		
		var movementVector = (KeyList) key.Scancode switch
		{
			KeyList.Up => -forward,
			KeyList.Down => forward,
			KeyList.Left => forward.Cross(Vector3.Up),
			KeyList.Right => -forward.Cross(Vector3.Up),
			_ => Vector3.Zero
		};
		
		if(movementVector == Vector3.Zero)
			return;

		tween.InterpolateProperty(this, "translation",
			Translation,
			Translation + movementVector.Normalized() * 4,
			0.2f,
			Tween.TransitionType.Sine,
			Tween.EaseType.InOut);

		tween.Start();
	}
	
	private void Rotate(InputEventKey key)
	{
		var tween = (Tween) GetNode("Tween");

		if (tween.IsActive())
		{
			return;
		}
		
		var leftRotation = new Vector3(0, 90, 0);
		var rightRotation = -leftRotation;
		
		var rotVector = (KeyList) key.Scancode switch
		{
			KeyList.Q => leftRotation,
			KeyList.E => rightRotation,
			_ => Vector3.Zero
		};
		
		if (rotVector == Vector3.Zero)
			return;

		tween.InterpolateProperty(this, "rotation_degrees",
			RotationDegrees,
			RotationDegrees + rotVector,
			0.2f,
			Tween.TransitionType.Sine,
			Tween.EaseType.InOut);

		tween.Start();
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
