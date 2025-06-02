using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
public enum TileType { cat, sheep, Spider, Unicorn }
public class Tile : MonoBehaviour
{
    public TileType tileType;
    public bool isDragging = false;
    private Board board;
    private Vector2 initialMousePos;
    private bool horizontalDrag;
    private bool directionLocked = false;

    public int x, y;
    private int row, col;
    void Start()
    {
        board = FindFirstObjectByType<Board>();
    }

    void OnMouseDown()
    {
        isDragging = true;
        directionLocked = false;
        initialMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    void OnMouseUp()
    {
        isDragging = false;
        if (horizontalDrag)
        {
            StartCoroutine(board.ValidateShiftRow(row));
        }
        else 
        {
            StartCoroutine(board.ValidateShiftColumn(col));
        }
    }

    void Update()
    {
        if (!isDragging) return;

        Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 delta = currentMousePos - initialMousePos;

        float threshold = board.spacing;

        // Y�n kilitleme sadece ilk kez yap�l�r
        if (!directionLocked && (Mathf.Abs(delta.x) > threshold || Mathf.Abs(delta.y) > threshold))
        {
            horizontalDrag = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
            directionLocked = true;
        }

        // E�er y�n kilitlenmemi�se, delta de�erine g�re y�n belirlenir ve kilitlenir
        if (directionLocked)
        {
            //Yatay y�n�nde s�r�kleme
            if (horizontalDrag && Mathf.Abs(delta.x) >= threshold)
            {
                int steps = Mathf.FloorToInt(delta.x / threshold);
                board.ShiftRow(y, steps);
                row = y; // Yatay s�r�kleme s�ras�nda sat�r bilgisi g�ncellenir
                initialMousePos.x += steps * threshold;
            }
            // Dikey y�n�nde s�r�kleme
            else if (!horizontalDrag && Mathf.Abs(delta.y) >= threshold)
            {
                int steps = Mathf.FloorToInt(delta.y / threshold);
                board.ShiftColumn(x, steps);
                col = x; // Dikey s�r�kleme s�ras�nda s�tun bilgisi g�ncellenir
                initialMousePos.y += steps * threshold;
            }
        }
    }
}
