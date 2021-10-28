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

	private bool CanMoveInto(Vector3 direction)
	{
		var rayCast = (RayCast)GetNode("RayCast");
		var drawer = (ImmediateGeometry) GetNode("Drawer");

		rayCast.GlobalTransform = GlobalTransform; 
		rayCast.Rotation = new Vector3(0, 0, 0);

		rayCast.CastTo = direction;

		drawer.Clear();
		
		drawer.Begin(Mesh.PrimitiveType.Lines);
		drawer.SetColor(Colors.Red);
		drawer.AddVertex(new Vector3(0, 0, 0));
		drawer.AddVertex(direction);
		drawer.End();

		rayCast.ForceRaycastUpdate();
		
		Console.WriteLine(rayCast.IsColliding());

		if (rayCast.IsColliding())
		{
			Console.WriteLine(rayCast.GetCollisionPoint());
		}
		
		return !rayCast.IsColliding();
	}

	private void Move(InputEventKey key)
	{
		var tween = (Tween) GetNode("Tween");

		if (tween.IsActive())
		{
			return;
		}
		
		var forward = -Transform.basis.z.Normalized();

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
		
		var direction = Translation + movementVector;
		
		var forwardRay = 4 * Vector3.Forward;
		var movementVectorRay = (KeyList) key.Scancode switch
		{
			KeyList.Up => forwardRay,
			KeyList.Down => -forwardRay,
			KeyList.Left => -forwardRay.Cross(Vector3.Up),
			KeyList.Right => forwardRay.Cross(Vector3.Up),
			_ => Vector3.Zero
		};

		if (!CanMoveInto(movementVectorRay))
			return;

		tween.InterpolateProperty(this, "translation",
			Translation,
			direction,
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
