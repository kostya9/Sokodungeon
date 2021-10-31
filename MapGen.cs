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
p + + +|o o o|+|
  - -  
o o o|+|o o o|+|

o o o|+|o o o|+|

o o o|+|o o o|+|

o o o|+|o o o|+|

o o o|+|o o o|+|

o o o|+|o o o|+|
        - - -   
o o o|+ + + + +|
- - - - - - - -
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
		public Vector2 FirstCellPos;

		public Vector2 SecondCellPos;

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
		var lines = map.Trim('\r', '\n').Split('\r', '\n');

		List<Cell> cells = new List<Cell>();
		List<Wall> walls = new List<Wall>();

		int x = 0;
		int y = 0;

		int playerX = 0, 
			playerY = 0;

		for (var row = 0; row < lines.Length; row++)
		{
			var rowContent = lines[row];
			for (int col = 0; col < rowContent.Length; col++)
			{
				var current = rowContent[col];

				if (row % 2 == 0) // Horizontal walls
				{
					if (current == '-')
					{
						walls.Add(new Wall
						{
							FirstCellPos = new Vector2(x, y - 1),
							SecondCellPos = new Vector2(x, y),

							Type = WallType.Horizontal
						});
					}
					
					if (col % 2 == 1)
					{
						x++;
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
							FirstCellPos = new Vector2(x - 1, y),
							SecondCellPos = new Vector2(x, y),

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

	private const float xLength = 4.0f;
	private const float yLength = 4.0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var parsed = ParseMap(map);
		
		var player = (Spatial) GetNode("Player");

		var wallResource = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/DungeonWallA.tscn");
		var floorResource = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/DungeonTileA.tscn");
		var columnResource = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/DungeonWallSeparatorA.tscn");

		player.Translation = new Vector3(parsed.PlayerX * xLength, 0, parsed.PlayerY * yLength);

		DrawCells(parsed, floorResource);
		DrawWalls(parsed, wallResource);
		DrawColumns(parsed, columnResource);
	}

	private void DrawColumns(Map parsed, PackedScene columnResource)
	{
		var wallsByStartPosition = parsed.Walls.ToLookup(x => x.FirstCellPos);
		var wallsByEndPosition = parsed.Walls.ToLookup(x => x.SecondCellPos);

		HashSet<Vector2> columns = new();

		foreach (var wall in parsed.Walls)
		{
			if (wall.Type == WallType.Horizontal)
			{
				// x x
				// - -
				// x x
				var rightWalls = wallsByStartPosition[new Vector2(wall.FirstCellPos.x + 1, wall.FirstCellPos.y)];
				foreach (var rightWall in rightWalls)
				{
					if (rightWall.Type == WallType.Horizontal)
					{
						columns.Add(new Vector2(wall.FirstCellPos.x + 0.5f, wall.FirstCellPos.y + 0.5f));
					}
				}

				// +
				// - 
				// + | +
				var startWallsBySecondPosition = wallsByStartPosition[wall.SecondCellPos];
				foreach (var wallBySecondPos in startWallsBySecondPosition)
				{
					if (wallBySecondPos.Type == WallType.Vertical)
					{
						columns.Add(new Vector2(wall.FirstCellPos.x + 0.5f, wall.FirstCellPos.y + 0.5f));
					}
				}
				
				//     +
				//     - 
				// + | +
				var endWallsBySecondPosition = wallsByEndPosition[wall.SecondCellPos];
				foreach (var wallBySecondPos in endWallsBySecondPosition)
				{
					if (wallBySecondPos.Type == WallType.Vertical)
					{
						columns.Add(new Vector2(wall.FirstCellPos.x + 0.5f, wall.FirstCellPos.y - 0.5f));
					}
				}
			}


			if (wall.Type == WallType.Vertical)
			{
				// + | + 
				// + | +
				var bottomWalls = wallsByStartPosition[new Vector2(wall.FirstCellPos.x, wall.FirstCellPos.y + 1)];
				foreach (var bottomWall in bottomWalls)
				{
					if (bottomWall.Type == WallType.Vertical)
					{
						columns.Add(new Vector2(wall.FirstCellPos.x + 0.5f, wall.FirstCellPos.y + 0.5f));
					}
				}
				
				
				// + | +
				//     -
				//     +
				var startWallsBySecondPosition = wallsByStartPosition[wall.SecondCellPos];
				foreach (var wallBySecondPosition in startWallsBySecondPosition)
				{
					if (wallBySecondPosition.Type == WallType.Horizontal)
					{
						columns.Add(new Vector2(wall.FirstCellPos.x + 0.5f, wall.FirstCellPos.y + 0.5f));
					}
				}
				
				// + | +
				// -   
				// +   
				var startWallsByFirstPosition = wallsByStartPosition[wall.FirstCellPos];
				foreach (var wallByFirstPos in startWallsByFirstPosition)
				{
					if (wallByFirstPos.Type == WallType.Horizontal)
					{
						columns.Add(new Vector2(wall.FirstCellPos.x + 0.5f, wall.FirstCellPos.y + 0.5f));
					}
				}
			}
		}

		foreach (var columnPos in columns)
		{
			var columnNode = (Spatial) columnResource.Instance();
			columnNode.Translation = new Vector3(columnPos.x * xLength, 0, columnPos.y * yLength);
			AddChild(columnNode);
		}
	}

	private void DrawCells(Map parsed, PackedScene floorResource)
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

	private void DrawWalls(Map parsed, PackedScene wallResource)
	{
		var cellsByPosition = parsed.Cells.ToDictionary(x => x.Position);
		foreach (var wall in parsed.Walls)
		{
			var worldFirstX = xLength * wall.FirstCellPos.x;
			var worldFirstZ = yLength * wall.FirstCellPos.y;

			var worldSecondX = xLength * wall.SecondCellPos.x;
			var worldSecondZ = yLength * wall.SecondCellPos.y;

			var worldX = (worldFirstX + worldSecondX) / 2;
			var worldZ = (worldFirstZ + worldSecondZ) / 2;

			var translation = new Vector3(worldX, 0, worldZ);

			if (wall.Type == WallType.Vertical)
			{
				var top = wall.FirstCellPos;
				if (cellsByPosition.ContainsKey(top))
				{
					var copy = (Spatial) wallResource.Instance();
					copy.Translation = translation;
					copy.Rotation = new Vector3(0, 3 * Mathf.Pi / 2, 0);
					AddChild(copy);
				}

				var bot = wall.SecondCellPos;
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
				var left = wall.FirstCellPos;
				if (cellsByPosition.ContainsKey(left))
				{
					var copy = (Spatial) wallResource.Instance();
					copy.Translation = translation;
					copy.Rotation = new Vector3(0, Mathf.Pi, 0);
					AddChild(copy);
				}

				var right = wall.SecondCellPos;
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
