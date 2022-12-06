using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;


public enum SpecialTile
{
    MoneyReduction = 0,
    Bomb = 1,
    CannotTouch = 2,
    StrikeNow = 3
}

public class SpecialTileCounter
{
    public List<Tile> sTiles;
    public SpecialTileCounter() => sTiles = new List<Tile>();

    public Tile Front => sTiles[0];

    public void Remove() => sTiles.RemoveAt(0);

    public void Clear() => sTiles.Clear();

    public int Count => sTiles.Count;

    public bool Contains(Tile tile) => sTiles.Contains(tile);

    public bool IsEmpty() => sTiles.Count == 0;
}

// 特殊方块在生成之后被扔到这里
public class SpecialTileCounters
{

    public SpecialTileCounter[] counters;
    public SpecialTileCounters(int len)
    {
        counters = new SpecialTileCounter[len];
        for (int i = 0; i < len; i++)
        {
            counters[i] = new SpecialTileCounter();
        }
    }

    // Operator overload [] to access the counters
    public SpecialTileCounter this[int index] => counters[index];

    public void Add(Tile tile)
    {
        Debug.Assert(tile.Type.isSpecial);
        int id = tile.Type.id - 100;
        Debug.Log("Adding " + id + " at col:" + tile.x + " row:" + tile.y);
        counters[id].sTiles.Add(tile);
    }

    public void Remove(int id) => counters[id].Remove();

    public void Clear()
    {
        for (int i = 0; i < counters.Length; i++)
            counters[i].Clear();
    }

    public int Count => counters.Sum(c => c.Count);

    public bool Contains(Tile tile) => counters[tile.Type.id - 100].Contains(tile);

    public bool IsEmpty() => counters.All(c => c.IsEmpty());
}

public sealed class Board : MonoBehaviour
{

    public AudioSource clock;
    public AudioSource fail;
    public AudioSource explode;
    public static Board Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        specialTileCounters = new SpecialTileCounters(specialTileNum);
    }


    [SerializeField] public TileTypeAsset[] tileTypes;

    // alllow other query the length of normal tileTypes
    public int normalTileTypesLength => tileTypes.Length - specialTileNum;

    [SerializeField] public int specialTileNum;

    public SpecialTileCounters specialTileCounters;

    [SerializeField] private Row[] rows;

    [SerializeField] private AudioClip matchSound;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private float tweenDuration;

    [SerializeField] private Transform swappingOverlay;

    [SerializeField] private bool ensureNoStartingMatches;

    private readonly List<Tile> _selection = new List<Tile>();

    private bool _isSwapping;
    private bool _isMatching;
    private bool _isShuffling;

    public event Action<TileTypeAsset, int> OnMatch;

    private TileData[,] Matrix
    {
        get
        {
            var width = rows.Max(row => row.tiles.Length);
            var height = rows.Length;

            var data = new TileData[width, height];

            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    data[x, y] = GetTile(x, y).Data;

            return data;
        }
    }

    private void Start()
    {
        for (var y = 0; y < rows.Length; y++)
        {
            for (var x = 0; x < rows.Max(row => row.tiles.Length); x++)
            {
                Tile tile = GetTile(x, y);

                tile.x = x;
                tile.y = y;

                tile.Type = tileTypes[Random.Range(0, normalTileTypesLength)];

                tile.button.onClick.AddListener(() => Select(tile));
            }
        }

        if (ensureNoStartingMatches) StartCoroutine(EnsureNoStartingMatches());

        OnMatch += (type, count) =>
        {
            // Debug.Log($"Matched {count}x tile_{type.name}.");
            double money = count * count * count * type.value;

            if (!specialTileCounters[(int)SpecialTile.MoneyReduction].IsEmpty())
            {
                for (int i = 0; i < specialTileCounters[0].Count; i++)
                    money *= 0.75;
            }


            MoneyBoard.Instance.Money += (int)money;

            Risk.Instance.RiskValue += count;
        };

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var bestMove = TileDataMatrixUtility.FindBestMove(Matrix);

            if (bestMove != null)
            {
                Select(GetTile(bestMove.X1, bestMove.Y1));
                Select(GetTile(bestMove.X2, bestMove.Y2));
            }
        }
    }

    private IEnumerator EnsureNoStartingMatches()
    {
        var wait = new WaitForEndOfFrame();

        while (TileDataMatrixUtility.FindBestMatch(Matrix) != null)
        {
            Shuffle();

            yield return wait;
        }
    }

    private Tile GetTile(int x, int y) => rows[y].tiles[x];

    private Tile[] GetTiles(IList<TileData> tileData)
    {
        var length = tileData.Count;

        var tiles = new Tile[length];

        for (var i = 0; i < length; i++)
            tiles[i] = GetTile(tileData[i].X, tileData[i].Y);

        return tiles;
    }

    public async void Select(Tile tile)
    {
        if (_isSwapping || _isMatching || _isShuffling) return;

        if (tile.Type.canBeSelected == false)
        {
            // TODO: Play a sound
            fail.Play();
            return;
        }

        // 这里可以试着让特殊方块不能被选中
        // if (tile.Type.isSpecial) return;

        if (!_selection.Contains(tile))
        {
            if (_selection.Count > 0)
            {
                if (Math.Abs(tile.x - _selection[0].x) == 1 && Math.Abs(tile.y - _selection[0].y) == 0
                    || Math.Abs(tile.y - _selection[0].y) == 1 && Math.Abs(tile.x - _selection[0].x) == 0)
                    _selection.Add(tile);
                else
                {
                    _selection.Clear();
                    _selection.Add(tile);
                }
            }
            else
            {
                _selection.Add(tile);
            }
        }

        if (_selection.Count < 2) return;

        await SwapAsync(_selection[0], _selection[1]);

        if (!await TryMatchAsync()) await SwapAsync(_selection[0], _selection[1]);

        var matrix = Matrix;

        while (TileDataMatrixUtility.FindBestMove(matrix) == null || TileDataMatrixUtility.FindBestMatch(matrix) != null)
        {
            // Shuffle();
            // matrix = Matrix;
            Debug.Log("No more moves!");
        }

        _selection.Clear();
    }

    private async Task SwapAsync(Tile tile1, Tile tile2)
    {
        _isSwapping = true;

        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        icon1Transform.SetParent(swappingOverlay);
        icon2Transform.SetParent(swappingOverlay);

        icon1Transform.SetAsLastSibling();
        icon2Transform.SetAsLastSibling();

        var sequence = DOTween.Sequence();

        sequence.Join(icon1Transform.DOMove(icon2Transform.position, tweenDuration).SetEase(Ease.OutBack))
                .Join(icon2Transform.DOMove(icon1Transform.position, tweenDuration).SetEase(Ease.OutBack));

        await sequence.Play().AsyncWaitForCompletion();

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1Item = tile1.Type;

        tile1.Type = tile2.Type;

        tile2.Type = tile1Item;

        _isSwapping = false;
    }

    // TODO: 
    // CountDown and then make the position of the tile to be inactivated
    private IEnumerator CountDownAndExplode(Tile tile, int times)
    {
        while (times > 0)
        {
            // 需要音效 倒计时
            clock.Play();
            Debug.Log("CountDown: " + times);
            yield return new WaitForSeconds(1);
            times--;
            if (times == 0)
            {

                explode.Play();

            }
        }
        clock.Stop();

        // 需要音效 爆炸


        tile.button.interactable = false;
        // 更改图标为爆炸之后的样子, 需要在Board中添加一个爆炸的sprite存这个图
        // tile.icon.sprite = ;
        // 永远的禁用这个方块, 需要在Board里面添加一个记录表, 让洗牌不可以修复这个位置?
        // 感觉禁用按钮没有意义, 禁用交换即可



        List<TileData> neighbors = TileDataMatrixUtility.GetNeighbors(Matrix, tile.x, tile.y);
        foreach (var data in neighbors)
        {
            Tile t = GetTile(data.X, data.Y);
            t.button.interactable = false;
        }
    }

    private async Task<bool> TryMatchAsync()
    {
        var didMatch = false;

        _isMatching = true;

        var match = TileDataMatrixUtility.FindBestMatch(Matrix);

        while (match != null)
        {
            didMatch = true;


            var tiles = GetTiles(match.Tiles);

            var deflateSequence = DOTween.Sequence();

            foreach (var tile in tiles)
                deflateSequence.Join(tile.icon.transform.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InBack));

            audioSource.PlayOneShot(matchSound);

            await deflateSequence.Play().AsyncWaitForCompletion();

            var inflateSequence = DOTween.Sequence();

            // 在这生成新的方块
            foreach (var tile in tiles)
            {
                tile.Type = tileTypes[Random.Range(0, normalTileTypesLength)];
                // 恢复可点击
                tile.button.interactable = true;

                // Debug.Log("RiskValue: " + Risk.Instance.RiskValue);
                if (Risk.Instance.RiskValue > Random.Range(5, 100))
                {
                    tile.Type = tileTypes[Random.Range(normalTileTypesLength, tileTypes.Length)];
                    SpecialTile specialTileType = (SpecialTile)(tile.Type.id - 100);
                    Debug.Log("Generate Special Tile " + specialTileType);

                    if (specialTileType == SpecialTile.CannotTouch)
                    {
                        // 禁止点击特殊方块
                        // tile.button.interactable = false;
                    }

                    if (specialTileType == SpecialTile.Bomb)
                    {
                        StartCoroutine(CountDownAndExplode(tile, 10));
                    }



                    // 添加方块到counter
                    specialTileCounters.Add(tile);

                    Risk.Instance.RiskValue = 0;
                }

                inflateSequence.Join(tile.icon.transform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutBack));
            }

            await inflateSequence.Play().AsyncWaitForCompletion();

            OnMatch?.Invoke(Array.Find(tileTypes, tileType => tileType.id == match.TypeId), match.Tiles.Length);

            match = TileDataMatrixUtility.FindBestMatch(Matrix);
        }

        _isMatching = false;

        return didMatch;
    }

    public void Shuffle()
    {
        _isShuffling = true;

        foreach (var row in rows)
            foreach (var tile in row.tiles)
            {
                tile.Type = tileTypes[Random.Range(0, normalTileTypesLength)];
                tile.button.interactable = true;
            }

        _isShuffling = false;
    }
}

