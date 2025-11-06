//using Components.ProceduralGeneration;
//using Cysharp.Threading.Tasks;
//using System.Threading;
//using UnityEngine;
//using VTools.Grid;
//using VTools.RandomService;
//using VTools.ScriptableObjectDatabase;

//[CreateAssetMenu(menuName = "Procedural Generation Method/NewAlgo")]
//public class NewAlgo : ProceduralGenerationMethod
//{
//    private Vector2Int _roomMinSize = new(5, 5);
//    private Vector2Int _roomMaxSize = new(12, 8);

//    private RandomService _randomService = new RandomService(System.DateTime.Now.Millisecond);

//    void Start()
//    {
//        LogFact(5);
//    }

//    protected override UniTask ApplyGeneration(CancellationToken cancellationToken)
//    {
//        if (Grid == null)
//        {
//            Debug.LogError("NewAlgo: Grid est null. Assure-toi qu'il est assigné avant la génération.");
//            return UniTask.CompletedTask;
//        }

//        if (GridGenerator == null)
//        {
//            Debug.LogError("NewAlgo: GridGenerator est null. Assure-toi qu'il est assigné avant la génération.");
//            return UniTask.CompletedTask;
//        }

//        BuildGround();

//        BSPNode root = new BSPNode(new RectInt(0, 0, Grid.Width, Grid.Height));

//        root.SplitNode(root, _randomService, _roomMinSize);

//        if (root.left != null)
//            root.left.SplitNode(root.left, _randomService, _roomMinSize);

//        if (root.right != null)
//            root.right.SplitNode(root.right, _randomService, _roomMinSize);

//        var leaves = new System.Collections.Generic.List<BSPNode>();
//        root.GetLeaves(leaves);

//        if (leaves.Count == 0)
//        {
//            Debug.LogWarning("NewAlgo: aucune feuille générée par le BSP (leaves.Count == 0).");
//            return UniTask.CompletedTask;
//        }

//        for (int i = 0; i < leaves.Count; i++)
//        {
//            var currentLeaf = leaves[i];

//            int maxAllowedWidth = Mathf.Max(_roomMinSize.x, currentLeaf.area.width - 1);
//            int maxAllowedHeight = Mathf.Max(_roomMinSize.y, currentLeaf.area.height - 1);

//            if (currentLeaf.area.width <= _roomMinSize.x || currentLeaf.area.height <= _roomMinSize.y)
//            {
//                Debug.Log($"NewAlgo: feuille trop petite pour une salle : {currentLeaf.area}");
//                continue;
//            }

//            int roomWidth = _randomService.Range(_roomMinSize.x, Mathf.Min(_roomMaxSize.x, currentLeaf.area.width - 1));
//            int roomHeight = _randomService.Range(_roomMinSize.y, Mathf.Min(_roomMaxSize.y, currentLeaf.area.height - 1));

//            int maxRoomX = currentLeaf.area.xMax - roomWidth;
//            int maxRoomY = currentLeaf.area.yMax - roomHeight;
//            if (maxRoomX <= currentLeaf.area.xMin || maxRoomY <= currentLeaf.area.yMin)
//            {
//                Debug.Log($"NewAlgo: Impossible de placer une salle dans la feuille {currentLeaf.area} (maxRoomX/Y invalide)");
//                continue;
//            }

//            int roomX = _randomService.Range(currentLeaf.area.xMin, maxRoomX);
//            int roomY = _randomService.Range(currentLeaf.area.yMin, maxRoomY);

//            RectInt room = new RectInt(roomX, roomY, roomWidth, roomHeight);
//            PlaceRoom(room);
//        }

//        return UniTask.CompletedTask;
//    }

//    private void LogFact(int value)
//    {
//        Debug.Log($"The factorial of {value} is {Factorial(value)}");
//    }

//    private static long Factorial(long value)
//    {
//        if (value == 0)
//            return 1;

//        return value * Factorial(value - 1);
//    }

//    private void PlaceRoom(RectInt room)
//    {
//        for (int ix = room.xMin; ix < room.xMax; ix++)
//        {
//            for (int iy = room.yMin; iy < room.yMax; iy++)
//            {
//                if (!Grid.TryGetCellByCoordinates(ix, iy, out var cell))
//                    continue;

//                AddTileToCell(cell, ROOM_TILE_NAME, true);
//            }
//        }
//    }

//    private void BuildGround()
//    {
//        var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
//        if (groundTemplate == null)
//        {
//            Debug.LogError("NewAlgo: groundTemplate 'Grass' introuvable dans ScriptableObjectDatabase.");
//            return;
//        }

//        for (int x = 0; x < Grid.Width; x++)
//        {
//            for (int z = 0; z < Grid.Lenght; z++)
//            {
//                if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
//                {
//                    Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
//                    continue;
//                }

//                GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
//            }
//        }
//    }
//}

//public class BSPNode
//{
//    public RectInt area;
//    public BSPNode left;
//    public BSPNode right;

//    public BSPNode(RectInt area)
//    {
//        this.area = area;
//    }

//    public bool IsLeaf => left == null && right == null;

//    public void SplitNode(BSPNode node, RandomService randomService, Vector2Int minSize)
//    {
//        if (node == null || randomService == null)
//            return;

//        bool splitHorizontally = randomService.Chance(0.5f);

//        if (splitHorizontally)
//        {
//            int minSplitY = node.area.yMin + 1;
//            int maxSplitY = node.area.yMax - 1;

//            if (maxSplitY <= minSplitY)
//                return;

//            int splitY = randomService.Range(minSplitY, maxSplitY);

//            var leftArea = new RectInt(node.area.xMin, node.area.yMin, node.area.width, splitY - node.area.yMin);
//            var rightArea = new RectInt(node.area.xMin, splitY, node.area.width, node.area.yMax - splitY);

//            if (leftArea.width < minSize.x || leftArea.height < minSize.y ||
//                rightArea.width < minSize.x || rightArea.height < minSize.y)
//            {
//                return;
//            }

//            node.left = new BSPNode(leftArea);
//            node.right = new BSPNode(rightArea);
//        }
//        else
//        {
//            int minSplitX = node.area.xMin + 1;
//            int maxSplitX = node.area.xMax - 1;

//            if (maxSplitX <= minSplitX)
//                return;

//            int splitX = randomService.Range(minSplitX, maxSplitX);

//            var leftArea = new RectInt(node.area.xMin, node.area.yMin, splitX - node.area.xMin, node.area.height);
//            var rightArea = new RectInt(splitX, node.area.yMin, node.area.xMax - splitX, node.area.height);

//            if (leftArea.width < minSize.x || leftArea.height < minSize.y ||
//                rightArea.width < minSize.x || rightArea.height < minSize.y)
//            {
//                return;
//            }

//            node.left = new BSPNode(leftArea);
//            node.right = new BSPNode(rightArea);
//        }
//    }

//    public void GetLeaves(System.Collections.Generic.List<BSPNode> leaves)
//    {
//        if (IsLeaf)
//        {
//            leaves.Add(this);
//        }
//        else
//        {
//            if (left != null) left.GetLeaves(leaves);
//            if (right != null) right.GetLeaves(leaves);
//        }
//    }
//}
using System.Collections.Generic;
using System.Threading;
using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;

[CreateAssetMenu(menuName = "Procedural Generation Method/BSP 2")]
public class BSP2 : ProceduralGenerationMethod
{
    [Header("Split Parameters")]
    [Range(0, 1)] public float HorizontalSplitChance = 0.5f;
    public Vector2 SplitRatio = new(0.3f, 0.7f);
    public int MaxSplitAttempt = 5;

    [Header("Leafs Parameters")]
    public Vector2Int LeafMinSize = new(8, 8);
    public Vector2Int RoomMaxSize = new(7, 7);
    public Vector2Int RoomMinSize = new(5, 5);

    [Header("Debug")]
    public List<Node> Tree;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        Tree = new List<Node>();

        var allGrid = new RectInt(0, 0, Grid.Width, Grid.Lenght);
        var root = new Node(RandomService, GridGenerator, this, allGrid);
        Tree.Add(root);

        root.ConnectSisters();
    }
}

[System.Serializable]
public class Node
{
    [SerializeField] private RectInt _room;
    [SerializeField] private bool _isLeaf;
    private readonly RandomService _randomService;
    private readonly BaseGridGenerator _gridGenerator;
    private readonly BSP2 _bsp2;
    private Node _child1;
    private Node _child2;

    public Node(RandomService randomService, BaseGridGenerator gridGenerator, BSP2 bsp2, RectInt room)
    {
        _randomService = randomService;
        _gridGenerator = gridGenerator;
        _bsp2 = bsp2;
        _room = room;

        Split();
    }

    private void Split()
    {
        RectInt splitBoundsLeft = default;
        RectInt splitBoundsRight = default;
        bool splitFound = false;

        for (int i = 0; i < _bsp2.MaxSplitAttempt; i++)
        {
            bool horizontal = _randomService.Chance(_bsp2.HorizontalSplitChance);
            float splitRatio = _randomService.Range(_bsp2.SplitRatio.x, _bsp2.SplitRatio.y);

            if (horizontal)
            {
                if (!CanSplitHorizontally(splitRatio, out splitBoundsLeft, out splitBoundsRight))
                {
                    continue;
                }
            }
            else
            {
                if (!CanSplitVertically(splitRatio, out splitBoundsLeft, out splitBoundsRight))
                {
                    continue;
                }
            }

            splitFound = true;
            break;
        }

        if (!splitFound)
        {
            _isLeaf = true;
            PlaceRoom(_room);
            return;
        }

        _child1 = new Node(_randomService, _gridGenerator, _bsp2, splitBoundsLeft);
        _child2 = new Node(_randomService, _gridGenerator, _bsp2, splitBoundsRight);

        _bsp2.Tree.Add(_child1);
        _bsp2.Tree.Add(_child2);
    }

    private bool CanSplitHorizontally(float splitRatio, out RectInt firstSplit, out RectInt secondSplit)
    {
        int widthSplit = Mathf.RoundToInt(_room.width * splitRatio);

        var firstSplitWidth = widthSplit;
        var firstSplitHeight = _room.height;
        firstSplit = new RectInt(_room.xMin, _room.yMin, firstSplitWidth, firstSplitHeight);

        var secondSplitWidth = _room.width - widthSplit;
        var secondSplitHeight = _room.height;
        secondSplit = new RectInt(_room.xMin + widthSplit, _room.yMin, secondSplitWidth, secondSplitHeight);

        if (firstSplit.width < _bsp2.LeafMinSize.x || firstSplit.height < _bsp2.LeafMinSize.y)
            return false;

        if (secondSplit.width < _bsp2.LeafMinSize.x || secondSplit.height < _bsp2.LeafMinSize.y)
            return false;

        return true;
    }

    private bool CanSplitVertically(float splitRatio, out RectInt firstSplit, out RectInt secondSplit)
    {
        int heightSplit = Mathf.RoundToInt(_room.height * splitRatio);

        var firstSplitWidth = _room.width;
        var firstSplitHeight = heightSplit;
        firstSplit = new RectInt(_room.xMin, _room.yMin, firstSplitWidth, firstSplitHeight);

        var secondSplitWidth = _room.width;
        var secondSplitHeight = _room.height - heightSplit;
        secondSplit = new RectInt(_room.xMin, _room.yMin + heightSplit, secondSplitWidth, secondSplitHeight);

        if (firstSplit.width < _bsp2.LeafMinSize.x || firstSplit.height < _bsp2.LeafMinSize.y)
            return false;

        if (secondSplit.width < _bsp2.LeafMinSize.x || secondSplit.height < _bsp2.LeafMinSize.y)
            return false;

        return true;
    }

    private void PlaceRoom(RectInt room)
    {
        var newRoomLength = _randomService.Range(_bsp2.RoomMinSize.x, _bsp2.RoomMaxSize.x + 1);
        var newRoomWidth = _randomService.Range(_bsp2.RoomMinSize.y, _bsp2.RoomMaxSize.y + 1);

        room.width = newRoomWidth;
        room.height = newRoomLength;
        _room = room;

        for (int ix = room.xMin; ix < room.xMax; ix++)
        {
            for (int iy = room.yMin; iy < room.yMax; iy++)
            {
                if (!_gridGenerator.Grid.TryGetCellByCoordinates(ix, iy, out var cell))
                    continue;

                var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Room");
                _gridGenerator.AddGridObjectToCell(cell, groundTemplate, true);
            }
        }
    }

    private Node GetLastChild()
    {
        if (_child1 != null)
            return _child1.GetLastChild();
        return this;
    }

    public void ConnectSisters()
    {
        if (_child1 == null || _child2 == null)
            return;

        ConnectNodes(_child1, _child2);

        _child1.ConnectSisters();
        _child2.ConnectSisters();
    }

    private void ConnectNodes(Node node1, Node node2)
    {
        var center1 = node1.GetLastChild()._room.GetCenter();
        var center2 = node2.GetLastChild()._room.GetCenter();
        CreateDogLegCorridor(center1, center2);
    }

    private void CreateDogLegCorridor(Vector2Int start, Vector2Int end)
    {
        bool horizontalFirst = _randomService.Chance(0.5f);

        if (horizontalFirst)
        {
            CreateHorizontalCorridor(start.x, end.x, start.y);
            CreateVerticalCorridor(start.y, end.y, end.x);
        }
        else
        {
            CreateVerticalCorridor(start.y, end.y, start.x);
            CreateHorizontalCorridor(start.x, end.x, end.y);
        }
    }

    private void CreateHorizontalCorridor(int x1, int x2, int y)
    {
        int xMin = Mathf.Min(x1, x2);
        int xMax = Mathf.Max(x1, x2);

        for (int x = xMin; x <= xMax; x++)
        {
            if (!_gridGenerator.Grid.TryGetCellByCoordinates(x, y, out var cell))
                continue;

            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Corridor");
            _gridGenerator.AddGridObjectToCell(cell, groundTemplate, false);
        }
    }

    private void CreateVerticalCorridor(int y1, int y2, int x)
    {
        int yMin = Mathf.Min(y1, y2);
        int yMax = Mathf.Max(y1, y2);

        for (int y = yMin; y <= yMax; y++)
        {
            if (!_gridGenerator.Grid.TryGetCellByCoordinates(x, y, out var cell))
                continue;

            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Corridor");
            _gridGenerator.AddGridObjectToCell(cell, groundTemplate, false);
        }
    }
}
