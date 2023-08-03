using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KSH.Utils;
using Unity.Mathematics;
using System.Runtime.InteropServices.WindowsRuntime;

public class Grid
{
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private int[] gridArray;
    //private TextMesh[,] debugTextArray;

    private int debugTextSize = 10;
    private Color debugColor = Color.green;

    public int Width => width;
    public int Height => height;

    public ref int[] GridArray => ref gridArray;

    public Grid(int width, int height, float cellSize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new int[width * height];

        //debugTextArray = new TextMesh[width, height];
        //Vector3 offset = new Vector3(-0.5f, -0.5f);
        //for (int x = 0; x < width; x++)
        //{
        //    for (int y = 0; y < height; y++)
        //    {
        //        debugTextArray[x, y] = UtilsClass.CreateWorldText(gridArray[x + y * width].ToString(), null, GetWorldPosition(x, y), debugTextSize, debugColor, TextAnchor.MiddleCenter, TextAlignment.Center);
        //        Debug.DrawLine(GetWorldPosition(x, y) + offset, GetWorldPosition(x, y + 1) + offset, debugColor, 100.0f);
        //        Debug.DrawLine(GetWorldPosition(x, y) + offset, GetWorldPosition(x + 1, y) + offset, debugColor, 100.0f);
        //    }
        //}

        //Debug.DrawLine(GetWorldPosition(0, height) + offset, GetWorldPosition(width, height) + offset, debugColor, 100.0f);
        //Debug.DrawLine(GetWorldPosition(width, 0) + offset, GetWorldPosition(width, height) + offset, debugColor, 100.0f);

    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    public void GetIndex(Vector3 worldPosition, out int x, out int y)
    {
        // x-> -0.5 ~ 0.5 -> 0
        // y-> -0.5 ~ 0.5 -> 0

        // x-> 0.5 ~ 1.5 -> 1
        x = Mathf.RoundToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.RoundToInt((worldPosition - originPosition).y / cellSize);
    }

    private bool IsXYValid(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    public bool SetValue(int x, int y, int value)
    {
        if (IsXYValid(x, y))
        {
            gridArray[x + y * width] = value;
            //debugTextArray[x, y].text = gridArray[x + y * width].ToString();

            return true;
        }

        return false;
    }

    public bool SetValue(Vector3 worldPosition, int value)
    {
        int x, y;
        GetIndex(worldPosition, out x, out y);
        return SetValue(x, y, value);
    }

    public int GetValue(int x, int y)
    {
        if (IsXYValid(x, y))
        {
            return gridArray[x + y * width];
        }
        else
        {
            return 0;
        }
    }

    public int GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetIndex(worldPosition, out x, out y);
        return GetValue(x, y);
    }

}
