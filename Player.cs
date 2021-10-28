using Godot;
using System;

public class Player : Spatial
{
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
			Console.WriteLine("click");
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
			var camera = (Camera)GetNode("Camera");
			
			var from = camera.ProjectRayOrigin(mouseButton.Position);
			var to = from + camera.ProjectRayNormal(mouseButton.Position) * 100f;

			var result = spaceState.IntersectRay(from, to);

			if (result.Count > 0)
			{
				var collider = (Node)result["collider"];
				Console.WriteLine(collider.Name);
			}
		}
	}

	private bool CanMoveInto(Vector3 endpoint)
	{
		var spaceState = GetWorld().DirectSpaceState;
		var from = GlobalTransform.origin;

		var intersectionResult = spaceState.IntersectRay(from, endpoint, null, 2147483647, false, true);

		if (intersectionResult.Count > 0)
		{
			Console.WriteLine(intersectionResult);
			return false;
		}
		
		
		
		return true;
	}
	

	private void Move(InputEventKey key)
	{
		var tween = (Tween) GetNode("Tween");

		if (tween.IsActive())
		{
			return;
		}
		
		var forward = Transform.basis.x.Normalized();

		var movementVector = (KeyList) key.Scancode switch
		{
			KeyList.Up => forward,
			KeyList.Down => -forward,
			KeyList.Left => -forward.Cross(Vector3.Up),
			KeyList.Right => forward.Cross(Vector3.Up),
			_ => Vector3.Zero
		};
		
		if(movementVector == Vector3.Zero)
			return;
		
		movementVector = movementVector.Normalized() * 4;
		var endpoint = Translation + movementVector;

		if (!CanMoveInto(endpoint))
			return;

		tween.InterpolateProperty(this, "translation",
			Translation,
			endpoint,
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
