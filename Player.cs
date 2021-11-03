using Godot;
using System;
using System.Collections.Generic;
using Dungeon;

public class Player : Spatial
{
    public const float MOVE_TIME = 0.45f;
    
    
    private readonly Queue<PlayerMovementState> _unprocessedMovement = new();
    private Tween _tween;

    public override void _Ready()
    {
        _tween = (Tween) GetNode("Tween");
    }
    
    private bool ProcessMovement(PlayerMovementState movementState)
    {
        if (Move(movementState))
        {
            return true;	
        }

        if (Rotate(movementState))
        {
            return true;
        }

        return false;
    }

    public override void _Process(float delta)
    {
        var movementState = PlayerMovement.ReadState();
        
        if (!_tween.IsActive())
        {
            // The order is important
            // Input queue should not interfere with falling
            
            if (ShouldFall())
            {
                FallDown();
                return;
            }
            
            if (_unprocessedMovement.Count > 0)
            {
                var toProcess = _unprocessedMovement.Dequeue();
                if (ProcessMovement(toProcess))
                {
                    return;
                }
            }

            if (ProcessMovement(movementState))
            {
                return;
            }

            return;
        }
        
        if (ShouldQueueMovementInput(movementState))
        {
            _unprocessedMovement.Enqueue(movementState);
        }
    }

    private bool ShouldQueueMovementInput(PlayerMovementState movementState)
    {
        const int queueLimit = 1;

        if (_unprocessedMovement.Count >= queueLimit)
        {
            return false;
        }

        // Allocate stuff on stack
        ReadOnlySpan<MovementKeyState> keys = stackalloc MovementKeyState[6]
        {
            movementState.Forwards,
            movementState.Backwards,
            movementState.Left,
            movementState.Right,

            movementState.RotateLeft,
            movementState.RotateRight,
        };

        foreach (var key in keys)
        {
            if (key.JustPressed)
            {
                return true;
            }
        }

        return false;
    }

    private void FallDown()
    {
        var endpoint = Translation + new Vector3(0, -Sizes.FloorHeight, 0);
        
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
        var playerHeight = 0.5f; // We need to include playerHeight to include the tile under us in collision testing
        var from = GlobalTransform.origin + new Vector3(0, playerHeight, 0);

        var endpoint = from + new Vector3(0, -Sizes.FloorHeight * 2, 0);
        
        var intersectionResult = spaceState.IntersectRay(from, endpoint, collideWithAreas: true);

        // There is nothing under us? whaaat?
        if (intersectionResult.Count == 0)
        {
            return false;
        }

        var intersectionPoint = (Vector3)intersectionResult["position"];
        if (from.y - intersectionPoint.y > Sizes.FloorHeight)
        {
            return true;
        }

        return false;
    }

    private bool CanMoveInto(Vector3 endpoint)
    {
        var spaceState = GetWorld().DirectSpaceState;
        var from = GlobalTransform.origin;
        from.y += Sizes.FloorHeight / 2;
        endpoint.y += Sizes.FloorHeight / 2;
        var intersectionResult = spaceState.IntersectRay(from, endpoint, collideWithAreas: true);
        
        Console.WriteLine(intersectionResult);
        
        return intersectionResult.Count == 0;
    }
    

    private bool Move(PlayerMovementState movementKeyState)
    {
        var forward = Transform.basis.x.Normalized();

        var movementVector = movementKeyState switch
        {
            { Forwards: { Pressed: true } } => forward,
            { Backwards: { Pressed: true } } => -forward,
            { Left: { Pressed: true } } => -forward.Cross(Vector3.Up),
            { Right: { Pressed: true } } => forward.Cross(Vector3.Up),
            _ => Vector3.Zero
        };
        
        if(movementVector == Vector3.Zero)
            return false;
        
        movementVector = movementVector.Normalized() * Sizes.Cell;
        var endpoint = Translation + movementVector;

        if (!CanMoveInto(endpoint))
            return false;

        _tween.InterpolateProperty(this, "translation",
            Translation,
            endpoint,
            MOVE_TIME);
        
        var animationTime = (float)GetCameraAnimationPlayer().GetAnimation("Step").Length;
        var customSpeed = animationTime / MOVE_TIME;
        GetCameraAnimationPlayer().Play("Step", customSpeed: customSpeed);
        _tween.Start();

        return true;
    }
    
    private bool Rotate(PlayerMovementState movementKeyState)
    {
        var leftRotation = new Vector3(0, 90, 0);
        var rightRotation = -leftRotation;
        
        var rotVector = movementKeyState switch
        {
            { RotateLeft: { Pressed: true } } => leftRotation,
            { RotateRight: { Pressed: true } } => rightRotation,
            _ => Vector3.Zero
        };

        if (rotVector == Vector3.Zero)
            return false;
        

        _tween.InterpolateProperty(this, "rotation_degrees",
            RotationDegrees,
            RotationDegrees + rotVector,
            0.3f,
            Tween.TransitionType.Sine,
            Tween.EaseType.InOut);

        _tween.Start();
        
        var animationTime = (float)GetCameraAnimationPlayer().GetAnimation("Rotate").Length;
        var customSpeed = animationTime / 0.3f;
        GetCameraAnimationPlayer().Play("Rotate", customSpeed: customSpeed);

        return true;
    }
}
