using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class MapGen : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private string[] map = {@"
- - - -       - 
p + + +|     | |
  - -  
     |+|     |+|

     |+|     |+|

     |+|     |+|

     |+|     |+|

     |+|     |+|

     |+|     |+|
        - - -   
     |+ + + + +|
      - - - - -
", @"
  - - -       -  
 |+ + +|     |+|
  - -  
     |+|     |+|

     |+|     |+|

     |+|     |+|

     |+|     |+|

     |+|     |+|

     |+|     |+|
        - - -   
     |+ + + + +|
      - - - - -
"};

    public struct Floor
    {
        public Cell[] Cells;
        
        public Wall[] Walls;	
    }

    public struct Map
    {
        public Floor[] Floors { get; set; }
        
        public Player Player { get; set; }
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

    public struct Player
    {
        public Vector2 Position { get; set; }
        
        public int FloorIdx { get; set; }
    }

    public (Floor, Player?) ParseFloor(int floorIdx, string floorMap)
    {
        var lines = floorMap
            .Replace("\r\n", "\n") // Make sure we are using \n for line ending
            .Trim('\n')
            .Split("\n");

        List<Cell> cells = new List<Cell>();
        List<Wall> walls = new List<Wall>();

        int x = 0;
        int y = 0;

        int playerX = 0, 
            playerY = 0;
        var foundPlayer = false;

        for (var row = 0; row < lines.Length; row++)
        {
            var rowContent = lines[row];
            for (int col = 0; col < rowContent.Length; col++)
            {
                var current = rowContent[col];
                
                if (col % 2 == 1)
                {
                    x++;
                }

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
                }
                else // Vertical walls, player and cells
                {
                    if (current == '+')
                    {
                        cells.Add(new Cell
                        {
                            Position = new Vector2(x, y)
                        });
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
                        foundPlayer = true;

                        cells.Add(new Cell
                        {
                            Position = new Vector2(x, y)
                        });
                    }
                }
            }

            if (row % 2 == 1)
            {
                y++;
            }

            x = 0;
        }

        Player? player = foundPlayer
            ? new Player
            {
                Position = new Vector2(playerX, playerY),
                FloorIdx = floorIdx
            }
            : null;
        var floor = new Floor
        {
            Cells = cells.ToArray(),
            Walls = walls.ToArray()
        }; 

        return (floor, player);
    }

    private Map ParseMap(string[] floorMaps)
    {
        var floors = new List<Floor>();
        Player? player = null;

        for (var floorIdx = 0; floorIdx < floorMaps.Length; floorIdx++)
        {
            var floorMap = floorMaps[floorIdx];
            var (floor, playerCandidate) = ParseFloor(floorIdx, floorMap);

            floors.Add(floor);

            if (playerCandidate.HasValue)
            {
                player = playerCandidate.Value;
            }
        }

        return new Map
        {
            Floors = floors.ToArray(),
            Player = player ?? new Player {Position = new Vector2(0, 0), FloorIdx = 0}
        };
    }

    private const float xLength = 5.2f;
    private const float yLength = 5.2f;
    private const float floorHeight = 4.5f;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
        var parsed = ParseMap(map);
        
        var player = (Spatial) GetNode("Player");

        var wallResource = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/DungeonWallA.tscn");
        var floorResource = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/DungeonTileA.tscn");
        var columnResource = (PackedScene)ResourceLoader.Load("res://Scenes/Environment/DungeonWallSeparatorA.tscn");

        player.Translation = new Vector3(parsed.Player.Position.x * xLength, -parsed.Player.FloorIdx * floorHeight, parsed.Player.Position.y * yLength);

        for (var floorIdx = 0; floorIdx < parsed.Floors.Length; floorIdx++)
        {
            var floor = parsed.Floors[floorIdx];
            
            DrawCells(floor, floorIdx, floorResource);
            DrawWalls(floor, floorIdx, wallResource);
            DrawColumns(floor, floorIdx, columnResource);
        }
    }

    private void DrawColumns(Floor parsed, int floorIdx, PackedScene columnResource)
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
                        columns.Add(new Vector2(wall.FirstCellPos.x - 0.5f, wall.FirstCellPos.y + 0.5f));
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
            columnNode.Translation = new Vector3(columnPos.x * xLength, -floorIdx * floorHeight, columnPos.y * yLength);
            AddChild(columnNode);
        }
    }

    private void DrawCells(Floor floor, int floorIdx, PackedScene floorResource)
    {
        var r = new Random();
        foreach (var cell in floor.Cells)
        {
            var rot = r.Next(4) * Mathf.Pi / 2;
            rot = 0;
            var copy = (Spatial) floorResource.Instance();

            var worldX = xLength * cell.Position.x;
            var worldZ = yLength * cell.Position.y;
            copy.Translation = new Vector3(worldX, -floorIdx * floorHeight, worldZ);
            copy.Rotation = new Vector3(0, rot, 0);

            AddChild(copy);
        }
    }

    private void DrawWalls(Floor floor, int floorIdx, PackedScene wallResource)
    {
        var cellsByPosition = floor.Cells.ToDictionary(x => x.Position);
        foreach (var wall in floor.Walls)
        {
            var worldFirstX = xLength * wall.FirstCellPos.x;
            var worldFirstZ = yLength * wall.FirstCellPos.y;

            var worldSecondX = xLength * wall.SecondCellPos.x;
            var worldSecondZ = yLength * wall.SecondCellPos.y;

            var worldX = (worldFirstX + worldSecondX) / 2;
            var worldZ = (worldFirstZ + worldSecondZ) / 2;

            var translation = new Vector3(worldX, -floorIdx * floorHeight, worldZ);

            if (wall.Type == WallType.Vertical)
            {
                var top = wall.FirstCellPos;
                var bot = wall.SecondCellPos;

                var hasTop = cellsByPosition.ContainsKey(top);
                var hasBot = cellsByPosition.ContainsKey(bot);
                
                var rotationTop = new Vector3(0, 3 * Mathf.Pi / 2, 0);
                if (hasTop)
                {
                    var copy = (Spatial) wallResource.Instance();
                    copy.Translation = translation;
                    copy.Rotation = rotationTop;
                    AddChild(copy);
                }

                var rotationBot = new Vector3(0, Mathf.Pi / 2, 0);
                if (hasBot)
                {
                    var copy = (Spatial) wallResource.Instance();
                    copy.Translation = translation;
                    copy.Rotation = rotationBot;
                    AddChild(copy);
                }

                if (!hasTop && !hasBot)
                {
                    var copyBot = (Spatial) wallResource.Instance();
                    copyBot.Translation = translation;
                    copyBot.Rotation = rotationBot;
                    AddChild(copyBot);
                    
                    var copyTop = (Spatial) wallResource.Instance();
                    copyTop.Translation = translation;
                    copyTop.Rotation = rotationTop;
                    AddChild(copyTop);
                }
            }
            else
            {
                var left = wall.FirstCellPos;
                var right = wall.SecondCellPos;

                var hasLeft = cellsByPosition.ContainsKey(left);
                var hasRight = cellsByPosition.ContainsKey(right);
                
                var rotationLeft = new Vector3(0, Mathf.Pi, 0);
                var rotationRight =  new Vector3(0, 0, 0);
                
                if (hasLeft)
                {
                    var copy = (Spatial) wallResource.Instance();
                    copy.Translation = translation;
                    copy.Rotation = rotationLeft;
                    AddChild(copy);
                }
                
                if (hasRight)
                {
                    var copy = (Spatial) wallResource.Instance();
                    copy.Translation = translation;
                    copy.Rotation = rotationRight;
                    AddChild(copy);
                }
                
                if (!hasLeft && !hasRight)
                {
                    var copyLeft = (Spatial) wallResource.Instance();
                    copyLeft.Translation = translation;
                    copyLeft.Rotation = rotationLeft;
                    AddChild(copyLeft);
                    
                    var copyRight = (Spatial) wallResource.Instance();
                    copyRight.Translation = translation;
                    copyRight.Rotation = rotationRight;
                    AddChild(copyRight);
                }
            }
        }
    }
}
