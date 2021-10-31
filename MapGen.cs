using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class MapGen : Node
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	private string map = @"
   - - - 
|p + + +|
 - - -   
|o o o|+|
         
|o o o|+|
         
|o o o|+|
         
|o o o|+|
         
|o o o|+|
 - - -   
 + + + +|
 - - - -
";

	public struct Map
	{
		public Cell[] Cells;
		
		public Wall[] Walls;
		public int PlayerX { get; set; }
		public int PlayerY { get; set; }
	}

	public struct Wall
	{
		public Vector2 FirstPosition;

		public Vector2 SecondPosition;

		public WallType Type;
	}

	public enum WallType
	{
		Vertical, Horizontal
	}

	public struct Cell
	{
		public Vector2 Position;
	}

	public Map ParseMap(string map)
	{
		map = map.Replace("\r\n", "\n");
		
		var mapSpan = map.AsSpan().Trim('\n');

		var width = mapSpan.IndexOf('\n') + 1;
		var height = 1 + (int)Math.Ceiling((mapSpan.LastIndexOf('\n')) / (double)width);

		List<Cell> cells = new List<Cell>();
		List<Wall> walls = new List<Wall>();

		int x = 0;
		int y = 0;

		int playerX = 0, 
			playerY = 0;

		for (int row = 0; row < height; row++)
		{
			for (int col = 0; col < width; col++)
			{
				var idx = col + row * width;
				if (idx >= mapSpan.Length)
					break;
				
				var current = mapSpan[col + row * width];

				if (row % 2 == 0) // Horizontal walls
				{
					if (current == '-')
					{
						walls.Add(new Wall
						{
							FirstPosition = new Vector2(x, y - 1),
							SecondPosition = new Vector2(x, y),

							Type = WallType.Horizontal
						});

						x++;
					}
					else if (current == ' ')
					{
						if (col % 2 == 1)
						{
							x++;
						}
					}
				}
				else // Vertical walls, player and cells
				{
					if (current == '+')
					{
						cells.Add(new Cell
						{
							Position = new Vector2(x, y)
						});

						x++;
					}
					else if (current == '|')
					{
						walls.Add(new Wall
						{
							FirstPosition = new Vector2(x - 1, y),
							SecondPosition = new Vector2(x, y),
							
							Type = WallType.Vertical
						});
					}
					else if (current == 'p')
					{
						playerX = x;
						playerY = y;
					
						cells.Add(new Cell
						{
							Position = new Vector2(x, y)
						});

						x++;
					}
					else if (current == 'o')
					{
						x++;
					}
				}
			}

			if (row % 2 == 1)
			{
				y++;	
			}
			
			x = 0;
		}

		return new Map
		{
			Cells = cells.ToArray(),
			Walls = walls.ToArray(),
			PlayerX = playerX,
			PlayerY = playerY
		};
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var parsed = ParseMap(map);
		
		var player = (Spatial) GetParent().GetNode("Player");

		var wallResource = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/DungeonWallA.tscn");
		var floorResource = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/DungeonTileA.tscn");


		float xLength = 4f;
		float yLength = 4f;

		DrawCells(parsed, floorResource, xLength, yLength);
		DrawWalls(parsed, xLength, yLength, wallResource);
	}

	private void DrawCells(Map parsed, PackedScene floorResource, float xLength, float yLength)
	{
		foreach (var cell in parsed.Cells)
		{
			var copy = (Spatial) floorResource.Instance();

			var worldX = xLength * cell.Position.x;
			var worldZ = yLength * cell.Position.y;
			copy.Translation = new Vector3(worldX, 0f, worldZ);

			AddChild(copy);
		}
	}

	private void DrawWalls(Map parsed, float xLength, float yLength, PackedScene wallResource)
	{
		var cellsByPosition = parsed.Cells.ToDictionary(x => x.Position);
		foreach (var wall in parsed.Walls)
		{
			var worldFirstX = xLength * wall.FirstPosition.x;
			var worldFirstZ = yLength * wall.FirstPosition.y;

			var worldSecondX = xLength * wall.SecondPosition.x;
			var worldSecondZ = yLength * wall.SecondPosition.y;

			var worldX = (worldFirstX + worldSecondX) / 2;
			var worldZ = (worldFirstZ + worldSecondZ) / 2;

			var translation = new Vector3(worldX, 0, worldZ);

			if (wall.Type == WallType.Vertical)
			{
				var top = wall.FirstPosition;
				if (cellsByPosition.ContainsKey(top))
				{
					var copy = (Spatial) wallResource.Instance();
					copy.Translation = translation;
					copy.Rotation = new Vector3(0, 3 * Mathf.Pi / 2, 0);
					AddChild(copy);
				}

				var bot = wall.SecondPosition;
				if (cellsByPosition.ContainsKey(bot))
				{
					var copy = (Spatial) wallResource.Instance();
					copy.Translation = translation;
					copy.Rotation = new Vector3(0, Mathf.Pi / 2, 0);
					AddChild(copy);
				}
			}
			else
			{
				var left = wall.FirstPosition;
				if (cellsByPosition.ContainsKey(left))
				{
					var copy = (Spatial) wallResource.Instance();
					copy.Translation = translation;
					copy.Rotation = new Vector3(0, Mathf.Pi, 0);
					AddChild(copy);
				}

				var right = wall.SecondPosition;
				if (cellsByPosition.ContainsKey(right))
				{
					var copy = (Spatial) wallResource.Instance();
					copy.Translation = translation;
					AddChild(copy);
				}
			}
		}
	}
}
