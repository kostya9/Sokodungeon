using Godot;
using System;
using System.Collections.Generic;
using Dungeon;

public class Player : Spatial
{
	public const float MOVE_TIME = 0.2f;
	public const float TILE_SIZE = 4;
	public const float TILE_HEIGHT = 4.5f;
	
	
	private readonly Queue<InputEvent> _unprocessedInputEvents = new();
	private readonly Dictionary<uint, List<string>> _actionsByKeycode = new();
	private Tween _tween;

	public override void _Ready()
	{
		_tween = (Tween) GetNode("Tween");

		InitializeKeyToActionMapping();
	}

	private void InitializeKeyToActionMapping()
	{
		foreach (string action in InputMap.GetActions())
		{
			foreach (InputEvent inputEvent in InputMap.GetActionList(action))
			{
				if (inputEvent is InputEventKey key)
				{
					if (!_actionsByKeycode.TryGetValue(key.Scancode, out var actionsList))
					{
						actionsList = new List<string>();

						_actionsByKeycode[key.Scancode] = actionsList;
					}

					actionsList.Add(action);
				}
			}
		}
	}

	public override void _Process(float delta)
	{
		if (!_tween.IsActive())
		{
			if (ShouldFall())
			{
				FallDown();
			}
			
			if (_unprocessedInputEvents.Count > 0)
			{
				var unprocessedEvent = _unprocessedInputEvents.Dequeue();
				
				ProcessInput(unprocessedEvent);
			}
		}
	}

	private void FallDown()
	{
		var endpoint = Translation + new Vector3(0, -TILE_HEIGHT, 0);
		
		_tween.InterpolateProperty(this, "translation",
			Translation,
			endpoint,
			MOVE_TIME,
			Tween.TransitionType.Sine,
			Tween.EaseType.InOut);

		_tween.Start();
	}

	public Spatial GetCameraHolder()
	{
		return (Spatial) GetNode("CameraHolder");
	}
	
	public Camera GetCamera()
	{
		return (Camera) GetCameraHolder().GetNode("Camera");
	}
	
	public AnimationPlayer GetCameraAnimationPlayer()
	{
		return (AnimationPlayer)GetCameraHolder().GetNode("AnimationPlayer");
	}
	
	public override void _Input(InputEvent @event)
	{
		ProcessInput(@event);
		
		base._Input(@event);
	}

	private void ProcessInput(InputEvent @event)
	{
		if (@event is InputEventKey key)
		{
			if (!_actionsByKeycode.TryGetValue(key.Scancode, out var actions))
			{
				return;
			}

			var isTweenActive = _tween.IsActive();

			foreach (var action in actions)
			{
				if (isTweenActive)
				{
					const int queueLimit = 2;
					if (Input.IsActionJustPressed(action) && _unprocessedInputEvents.Count < queueLimit)
					{
						_unprocessedInputEvents.Enqueue(key);
					}
				}
				else
				{
					Move(key, action);
					Rotate(key, action);	
				}
			}
		}

		if (@event is InputEventMouseButton mouseButton)
		{
			Console.WriteLine("click");
			Raycast(mouseButton);
		}	
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

	private bool ShouldFall()
	{
		var spaceState = GetWorld().DirectSpaceState;
		var from = GlobalTransform.origin;

		var endpoint = from + new Vector3(0, -TILE_HEIGHT * 2, 0);
		
		var intersectionResult = spaceState.IntersectRay(from, endpoint, collideWithAreas: true);

		// There is nothing under us? whaaat?
		if (intersectionResult.Count == 0)
		{
			return false;
		}

		var intersectionPoint = (Vector3)intersectionResult["position"];
		if (from.y - intersectionPoint.y > TILE_HEIGHT)
		{
			return true;
		}

		return false;
	}

	private bool CanMoveInto(Vector3 endpoint)
	{
		var spaceState = GetWorld().DirectSpaceState;
		var from = GlobalTransform.origin;
		var intersectionResult = spaceState.IntersectRay(from, endpoint, collideWithAreas: true);
		return intersectionResult.Count == 0;
	}
	

	private void Move(InputEventKey key, string action)
	{
		var forward = Transform.basis.x.Normalized();

		var movementVector = action switch
		{
			DungeonActions.Forwards => forward,
			DungeonActions.Backwards => -forward,
			DungeonActions.Left => -forward.Cross(Vector3.Up),
			DungeonActions.Right => forward.Cross(Vector3.Up),
			_ => Vector3.Zero
		};
		
		if(movementVector == Vector3.Zero)
			return;
		
		movementVector = movementVector.Normalized() * TILE_SIZE;
		var endpoint = Translation + movementVector;

		if (!CanMoveInto(endpoint))
			return;

		_tween.InterpolateProperty(this, "translation",
			Translation,
			endpoint,
			MOVE_TIME,
			Tween.TransitionType.Sine,
			Tween.EaseType.InOut);
		
		var animationTime = (float)GetCameraAnimationPlayer().GetAnimation("Step").Length;
		var customSpeed = animationTime / MOVE_TIME;
		GetCameraAnimationPlayer().Play("Step", customSpeed: customSpeed);
		_tween.Start();
	}
	
	private void Rotate(InputEventKey key, string action)
	{
		var leftRotation = new Vector3(0, 90, 0);
		var rightRotation = -leftRotation;
		
		var rotVector = action switch
		{
			DungeonActions.RotateLeft => leftRotation,
			DungeonActions.RotateRight => rightRotation,
			_ => Vector3.Zero
		};

		if (rotVector == Vector3.Zero)
			return;
		

		_tween.InterpolateProperty(this, "rotation_degrees",
			RotationDegrees,
			RotationDegrees + rotVector,
			MOVE_TIME,
			Tween.TransitionType.Sine,
			Tween.EaseType.InOut);

		_tween.Start();
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
