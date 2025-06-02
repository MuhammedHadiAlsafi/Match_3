using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    public int width = 8;
    public int height = 8;
    public float spacing = 0.9f;

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject[] tilePrefabs;

    private GameObject[,] grid;  // H�cre referanslar�
    private GameObject[,] tiles; // Ta� referanslar�
    private Vector2 originOffset;
    GameObject[] newRow;
    GameObject[] newCol;

    void Start()
    {
        grid = new GameObject[width, height];
        tiles = new GameObject[width, height];
        originOffset = new Vector2(-width / 2f * spacing + spacing / 2f, -height / 2f * spacing + spacing / 2f);

        GenerateGrid();
        PlaceInitialTiles();
    }

#region Grid and Tile Generation
    void GenerateGrid()//Grid olu�turma
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x * spacing, y * spacing) + originOffset;
                GameObject cell = Instantiate(cellPrefab, pos, Quaternion.identity);
                cell.name = $"Cell ({x},{y})";
                cell.transform.parent = this.transform;
                grid[x, y] = cell;
            }
        }
    }
    void PlaceInitialTiles()// Ba�lang�� ta�lar�n� yerle�tirme
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject prefab = GetRandomTile(x, y);
                Vector2 pos = new Vector2(x * spacing, y * spacing) + originOffset;
                GameObject tile = Instantiate(prefab, pos, Quaternion.identity);
                tile.GetComponent<Tile>().x = x;
                tile.GetComponent<Tile>().y = y;
                tile.name = $"Tile ({x},{y})";
                tile.transform.parent = this.transform;
                tiles[x, y] = tile;

            }
        }
    }
    GameObject GetRandomTile(int x, int y)// Rastgele ta� se�me
    {
        int maxAttempts = 100;
        int attempts = 0;
        GameObject selected;

        do
        {
            int rand = Random.Range(0, tilePrefabs.Length);
            selected = tilePrefabs[rand];
            TileType newType = selected.GetComponent<Tile>().tileType;
            attempts++;
        }
        while (
            (x >= 2 &&
             tiles[x - 1, y]?.GetComponent<Tile>()?.tileType == selected.GetComponent<Tile>().tileType &&
             tiles[x - 2, y]?.GetComponent<Tile>()?.tileType == selected.GetComponent<Tile>().tileType) ||

            (y >= 2 &&
             tiles[x, y - 1]?.GetComponent<Tile>()?.tileType == selected.GetComponent<Tile>().tileType &&
             tiles[x, y - 2]?.GetComponent<Tile>()?.tileType == selected.GetComponent<Tile>().tileType)

            && attempts < maxAttempts
        );

        return selected;
    }
#endregion Grid and Tile Generation
#region Board Manipulation
    public void ShiftRow(int row, int steps)// Sat�r kayd�rma
    {
        steps = ((steps % width) + width) % width;
        if (steps == 0) return;

        newRow = new GameObject[width];
        for (int x = 0; x < width; x++)
        {
            int newX = (x + steps) % width;
            newRow[newX] = tiles[x, row];
        }
        for (int x = 0; x < width; x++)
        {
            tiles[x, row] = newRow[x];
            Vector2 newPos = new Vector2(x * spacing, row * spacing) + originOffset;
            StartCoroutine(SmoothMove(tiles[x, row], newPos));

            Tile t = tiles[x, row].GetComponent<Tile>();
            t.x = x;
            t.y = row;
        }
        //StartCoroutine(ValidateShiftRow(row)); // Kayd�rma sonras� kontrol

    }
    public void ShiftColumn(int col, int steps)// S�tun kayd�rma
    {
        steps = ((steps % height) + height) % height;
        if (steps == 0) return;

        newCol = new GameObject[height];
        for (int y = 0; y < height; y++)
        {
            int newY = (y + steps) % height;
            newCol[newY] = tiles[col, y];
        }
        for (int y = 0; y < height; y++)
        {
            tiles[col, y] = newCol[y];
            Vector2 newPos = new Vector2(col * spacing, y * spacing) + originOffset;
            StartCoroutine(SmoothMove(tiles[col, y], newPos));

            Tile t = tiles[col, y].GetComponent<Tile>();
            t.x = col;
            t.y = y;
        }
    }
    IEnumerator SmoothMove(GameObject obj, Vector2 targetPos)// Ta��ma animasyonu
    {
        if (obj == null) yield break;

        float t = 0;
        float duration = 0.3f;
        Vector2 startPos = obj.transform.position;

        while (t < 1f)
        {
            if (obj == null) yield break; // Obje silinmi�se ��k
            t += Time.deltaTime / duration;
            obj.transform.position = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        if (obj != null)
            obj.transform.position = targetPos;
    }
#endregion Board Manipulation
#region Match Checking
    public void CheckMatches()// E�le�meleri kontrol etme
    {
        HashSet<GameObject> matches = new HashSet<GameObject>();

        // Yatay e�le�me kontrol�
        for (int y = 0; y < height; y++)
        {
            int matchLength = 1;
            for (int x = 0; x < width; x++)
            {
                bool checkMatch = false;

                if (x == width - 1)
                {
                    checkMatch = true;
                }
                else
                {
                    Tile current = tiles[x, y]?.GetComponent<Tile>();
                    Tile next = tiles[x + 1, y]?.GetComponent<Tile>();
                    
                    if (current != null && next != null && current.tileType == next.tileType)
                    {
                        matchLength++;
                    }
                    else
                    {
                        checkMatch = true;
                    }
                }

                if (checkMatch)
                {
                    if (matchLength >= 3)
                    {
                        for (int i = 0; i < matchLength; i++)
                        {
                            matches.Add(tiles[x - i, y]);
                        }
                    }
                    matchLength = 1;
                }
            }
        }

        // Dikey e�le�me kontrol�
        for (int x = 0; x < width; x++)
        {
            int matchLength = 1;
            for (int y = 0; y < height; y++)
            {
                bool checkMatch = false;

                if (y == height - 1)
                {
                    checkMatch = true;
                }
                else
                {
                    Tile current = tiles[x, y]?.GetComponent<Tile>();
                    Tile next = tiles[x, y + 1]?.GetComponent<Tile>();

                    if (current != null && next != null && current.tileType == next.tileType)
                    {
                        matchLength++;
                    }
                    else
                    {
                        checkMatch = true;
                    }
                }

                if (checkMatch)
                {
                    if (matchLength >= 3)
                    {
                        for (int i = 0; i < matchLength; i++)
                        {
                            matches.Add(tiles[x, y - i]);
                        }
                    }
                    matchLength = 1;
                }
            }
        }

        // E�le�me varsa yok etme i�lemi ba�lat
        if (matches.Count > 0)
        {
            StartCoroutine(DestroyMatches(matches));
        }
    }
    bool HasMatch()// E�le�me var m� kontrol�
    {
        // �ok basit ve genel e�le�me kontrol� (h�zl� �al��s�n diye ilk buldu�unda true d�ner)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile current = tiles[x, y].GetComponent<Tile>();

                // Sa�a bak
                if (x < width - 2)
                {
                    Tile t1 = tiles[x + 1, y].GetComponent<Tile>();
                    Tile t2 = tiles[x + 2, y].GetComponent<Tile>();
                    if (current.tileType == t1.tileType && current.tileType == t2.tileType)
                        return true;
                }
                // Yukar� bak
                if (y < height - 2)
                {
                    Tile t1 = tiles[x, y + 1].GetComponent<Tile>();
                    Tile t2 = tiles[x, y + 2].GetComponent<Tile>();
                    if (current.tileType == t1.tileType && current.tileType == t2.tileType)
                        return true;
                }
            }
        }

        return false;
    }
    IEnumerator DestroyMatches(HashSet<GameObject> matchedTiles)// E�le�en ta�lar� yok etme
    {
        foreach (var tile in matchedTiles)
        {
            Tile t = tile.GetComponent<Tile>();
            tiles[t.x, t.y] = null; // Grid'den temizle
            Destroy(tile);          // Objeyi yok et
        }

        yield return new WaitForSeconds(0.2f);

        //otomatik olarak bo�luklar� doldur
        yield return StartCoroutine(FillEmptySpaces());

        //doldurduktan sonra 1saniye bekle
        yield return new WaitForSeconds(1f);

        // Sonraki e�le�meleri kontrol et (zincirleme)
        CheckMatches();
    }
    IEnumerator FillEmptySpaces()// Bo�luklar� doldurma
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] == null)
                {
                    // Yukar�dan bo� olmayan� bul
                    for (int k = y + 1; k < height; k++)
                    {
                        if (tiles[x, k] != null)
                        {
                            tiles[x, y] = tiles[x, k];
                            tiles[x, k] = null;

                            tiles[x, y].transform.position = new Vector2(x * spacing, k * spacing) + originOffset;
                            StartCoroutine(SmoothMove(tiles[x, y], new Vector2(x * spacing, y * spacing) + originOffset));

                            Tile tileScript = tiles[x, y].GetComponent<Tile>();
                            tileScript.x = x;
                            tileScript.y = y;

                            break;
                        }
                    }

                    // E�er hala bo�sa, yeni ta� olu�tur
                    if (tiles[x, y] == null)
                    {
                        GameObject newTile = Instantiate(GetRandomTile(x, y),
                            new Vector2(x * spacing, height * spacing) + originOffset,
                            Quaternion.identity);

                        tiles[x, y] = newTile;
                        newTile.transform.parent = transform;

                        Tile tileScript = newTile.GetComponent<Tile>();
                        tileScript.x = x;
                        tileScript.y = y;

                        StartCoroutine(SmoothMove(newTile, new Vector2(x * spacing, y * spacing) + originOffset));
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
    }
    public IEnumerator ValidateShiftRow(int row)// Sat�r kayd�rma sonras� kontrol
    {
        yield return new WaitForSeconds(0.35f); // animasyon s�resi kadar bekle

        if (!HasMatch() && newRow != null)// e�le�me yoksa geri al
        {
            for (int x = 0; x < width; x++)
            {
                tiles[x, row] = newRow[x];
                Vector2 pos = new Vector2(x * spacing, row * spacing) + originOffset;
                StartCoroutine(SmoothMove(tiles[x, row], pos));
                Debug.Log($"Geri y�kl�yoruz: sat�r {row} - x={x}, hedef pos={pos}");
                Tile t = tiles[x, row].GetComponent<Tile>();
                t.x = x;
                t.y = row;
            }
        }
        else
        {
            CheckMatches(); // e�le�me varsa yok et
        }
    }
    public IEnumerator ValidateShiftColumn(int col)// S�tun kayd�rma sonras� kontrol
    {
        yield return new WaitForSeconds(0.35f);

        if (!HasMatch() && newCol != null)// e�le�me yoksa geri al
        {
            for (int y = 0; y < height; y++)
            {
                tiles[col, y] = newCol[y];
                Vector2 pos = new Vector2(col * spacing, y * spacing) + originOffset;
                StartCoroutine(SmoothMove(tiles[col, y], pos));
                Debug.Log($"Geri y�kl�yoruz: s�tun {col} - y={y}, hedef pos={pos}");

                Tile t = tiles[col, y].GetComponent<Tile>();
                t.x = col;
                t.y = y;
            }
        }
        else
        {
            CheckMatches();
        }
    }
#endregion Match Checking
}