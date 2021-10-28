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

		var player = (Spatial) GetParent().GetNode("Player");

		floor.Translation = player.Translation;

		var floorMeshInstance = floor.GetChild<MeshInstance>(0);
		var aabb = floorMeshInstance.Mesh.GetAabb();

		var initX = player.Translation.x;
		var initZ = player.Translation.z;

		var scaledSize = aabb.Size * floorMeshInstance.Scale;
		
		var width = scaledSize.x;
		var height = scaledSize.y;

		var columns = 100;
		var rows = 100;

		var centerCol = columns / 2;
		var centerRow = columns / 2;
		
		for (int col = 0; col < columns; col++)
		{
			for (int row = 0; row < rows; row++)
			{
				var posX = initX + col * width - centerCol * width;
				var posZ = initZ + row * height - centerRow * height;

				var floorCopy = (Spatial) floor.Duplicate();
				floorCopy.Translation = new Vector3(posX, player.Translation.y, posZ);
				floorCopy.Name = $"floor_{col}_{row}";
				
				GetParent().CallDeferred("add_child", floorCopy);
			}
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
