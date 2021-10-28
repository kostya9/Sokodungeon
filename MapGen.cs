using Godot;
using System;

public class MapGen : Node
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var floor = (Spatial) GetParent().GetNode("floor");


		for (var i = 1; i < 100; i++)
		{
			var floor1 = (Spatial)floor.Duplicate();
			floor1.Translate(new Vector3(0, i, 0));
			floor1.Name = $"floor{i}";
			
			GetParent().CallDeferred("add_child", floor1);
			
			
			var floorLeft = (Spatial)floor.Duplicate();
			floorLeft.Translate(new Vector3(-1, i, 0));
			floorLeft.Rotate(Vector3.Back, Mathf.Pi / 2);
			floorLeft.Name = $"floor{i}_left";
			
			GetParent().CallDeferred("add_child", floorLeft);
			
			
			var floorRight = (Spatial)floor.Duplicate();
			floorRight.Translate(new Vector3(0f, i - 1, 0));
			floorRight.Rotate(Vector3.Forward, Mathf.Pi / 2);
			floorRight.Name = $"floor{i}_right";
			
			GetParent().CallDeferred("add_child", floorRight);
		}
		
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
