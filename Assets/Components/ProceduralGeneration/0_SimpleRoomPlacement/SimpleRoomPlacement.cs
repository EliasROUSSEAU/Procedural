//using System.Threading;
//using System.Collections.Generic;
//using Cysharp.Threading.Tasks;
//using UnityEngine;
//using VTools.Grid;
//using VTools.ScriptableObjectDatabase;
//using VTools.Utility;

//namespace Components.ProceduralGeneration.SimpleRoomPlacement
//{
//    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
//    public class SimpleRoomPlacement : ProceduralGenerationMethod
//    {
//        [SerializeField] private int _maxRooms = 10;
//        private List<RectInt> placedRooms = new();

//        public int MaxRooms
//        {
//            get => _maxRooms;
//            set => _maxRooms = value;
//        }

//        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
//        {
//            BuildGround();

//            for (int i = 0; i < _maxRooms; i++)
//            {
//                cancellationToken.ThrowIfCancellationRequested();

//                int x = RandomService.Range(0, Grid.Width);
//                int z = RandomService.Range(0, Grid.Lenght);
//                int width = RandomService.Range(2, 5);
//                int height = RandomService.Range(2, 5);
//                RectInt room = new(x, z, width, height);

//                if (base.CanPlaceRoom(room, spacing: 1))
//                {
//                    placedRooms.Add(room);
//                }
//            }

//            foreach (var room in placedRooms)
//            {
//                for (int rx = 0; rx < room.width; rx++)
//                {
//                    for (int rz = 0; rz < room.height; rz++)
//                    {
//                        if (Grid.TryGetCellByCoordinates(room.x + rx, room.y + rz, out var cell))
//                        {
//                            var template = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Room");
//                            GridGenerator.AddGridObjectToCell(cell, template, true);
//                        }
//                    }
//                }

//                await UniTask.Delay(GridGenerator.StepDelay);
//            }

//            for (int j = 0; j < placedRooms.Count - 1; j++)
//            {
//                cancellationToken.ThrowIfCancellationRequested();

//                Vector2Int start = placedRooms[j].GetCenter();
//                Vector2Int end = placedRooms[j + 1].GetCenter();

//                CreateDogLegCorridor(start, end);

//                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
//            }
//        }

//        private void BuildGround()
//        {
//            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");

//            for (int x = 0; x < Grid.Width; x++)
//            {
//                for (int z = 0; z < Grid.Lenght; z++)
//                {
//                    if (Grid.TryGetCellByCoordinates(x, z, out var cell))
//                    {
//                        GridGenerator.AddGridObjectToCell(cell, groundTemplate, false);
//                    }
//                }
//            }
//        }

//        private void CreateDogLegCorridor(Vector2Int start, Vector2Int end)
//        {
//            bool horizontalFirst = RandomService.Chance(0.5f);

//            if (horizontalFirst)
//            {
//                CreateHorizontalCorridor(start.x, end.x, start.y);
//                CreateVerticalCorridor(start.y, end.y, end.x);
//            }
//            else
//            {
//                CreateVerticalCorridor(start.y, end.y, start.x);
//                CreateHorizontalCorridor(start.x, end.x, end.y);
//            }
//        }

//        private void CreateHorizontalCorridor(int xStart, int xEnd, int y)
//        {
//            int from = Mathf.Min(xStart, xEnd);
//            int to = Mathf.Max(xStart, xEnd);
//            for (int x = from; x <= to; x++)
//            {
//                if (Grid.TryGetCellByCoordinates(x, y, out var cell))
//                {
//                    var template = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Corridor");
//                    GridGenerator.AddGridObjectToCell(cell, template, true);
//                }
//            }
//        }

//        private void CreateVerticalCorridor(int yStart, int yEnd, int x)
//        {
//            int from = Mathf.Min(yStart, yEnd);
//            int to = Mathf.Max(yStart, yEnd);
//            for (int y = from; y <= to; y++)
//            {
//                if (Grid.TryGetCellByCoordinates(x, y, out var cell))
//                {
//                    var template = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Corridor");
//                    GridGenerator.AddGridObjectToCell(cell, template, true);
//                }
//            }
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        [SerializeField] private Vector2Int _roomMinSize = new(5, 5);
        [SerializeField] private Vector2Int _roomMaxSize = new(12, 8);

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            // ROOM CREATIONS
            List<RectInt> placedRooms = new();
            int roomsPlacedCount = 0;
            int attempts = 0;

            for (int i = 0; i < _maxSteps; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                if (roomsPlacedCount >= _maxRooms)
                {
                    break;
                }

                attempts++;

                // choose a random size
                int width = RandomService.Range(_roomMinSize.x, _roomMaxSize.x + 1);
                int lenght = RandomService.Range(_roomMinSize.y, _roomMaxSize.y + 1);

                // choose random position so entire room fits into grid
                int x = RandomService.Range(0, Grid.Width - width);
                int y = RandomService.Range(0, Grid.Lenght - lenght);

                RectInt newRoom = new RectInt(x, y, width, lenght);

                if (!CanPlaceRoom(newRoom, 1))
                    continue;

                PlaceRoom(newRoom);
                placedRooms.Add(newRoom);

                roomsPlacedCount++;

                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }

            if (roomsPlacedCount < _maxRooms)
            {
                Debug.LogWarning($"RoomPlacer Only placed {roomsPlacedCount}/{_maxRooms} rooms after {attempts} attempts.");
            }

            if (placedRooms.Count < 2)
            {
                Debug.Log("Not enough rooms to connect.");
                return;
            }

            // CORRIDOR CREATIONS
            for (int i = 0; i < placedRooms.Count - 1; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                Vector2Int start = placedRooms[i].GetCenter();
                Vector2Int end = placedRooms[i + 1].GetCenter();

                CreateDogLegCorridor(start, end);

                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }

            BuildGround();
        }

        // -------------------------------------- ROOM ---------------------------------------------

        /// Marks the grid cells of the room as occupied
        private void PlaceRoom(RectInt room)
        {
            for (int ix = room.xMin; ix < room.xMax; ix++)
            {
                for (int iy = room.yMin; iy < room.yMax; iy++)
                {
                    if (!Grid.TryGetCellByCoordinates(ix, iy, out var cell))
                        continue;

                    AddTileToCell(cell, ROOM_TILE_NAME, true);
                }
            }
        }

        // -------------------------------------- CORRIDOR --------------------------------------------- 

        /// Creates an L-shaped corridor between two points, randomly choosing horizontal-first or vertical-first
        private void CreateDogLegCorridor(Vector2Int start, Vector2Int end)
        {
            bool horizontalFirst = RandomService.Chance(0.5f);

            if (horizontalFirst)
            {
                // Draw horizontal line first, then vertical
                CreateHorizontalCorridor(start.x, end.x, start.y);
                CreateVerticalCorridor(start.y, end.y, end.x);
            }
            else
            {
                // Draw vertical line first, then horizontal
                CreateVerticalCorridor(start.y, end.y, start.x);
                CreateHorizontalCorridor(start.x, end.x, end.y);
            }
        }

        /// Creates a horizontal corridor from x1 to x2 at the given y coordinate
        private void CreateHorizontalCorridor(int x1, int x2, int y)
        {
            int xMin = Mathf.Min(x1, x2);
            int xMax = Mathf.Max(x1, x2);

            for (int x = xMin; x <= xMax; x++)
            {
                if (!Grid.TryGetCellByCoordinates(x, y, out var cell))
                    continue;

                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
        }

        /// Creates a vertical corridor from y1 to y2 at the given x coordinate
        private void CreateVerticalCorridor(int y1, int y2, int x)
        {
            int yMin = Mathf.Min(y1, y2);
            int yMax = Mathf.Max(y1, y2);

            for (int y = yMin; y <= yMax; y++)
            {
                if (!Grid.TryGetCellByCoordinates(x, y, out var cell))
                    continue;

                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
        }

        // -------------------------------------- GROUND --------------------------------------------- 

        private void BuildGround()
        {
            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");

            // Instantiate ground blocks
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                    {
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                        continue;
                    }

                    GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
                }
            }
        }
    }
}