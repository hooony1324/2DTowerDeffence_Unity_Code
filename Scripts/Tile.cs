using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public enum TileType
{
    StrongWall = 0,
    StartPoint,
    GoalPoint,
    TowerWall,
    None,
}

public class Tile : MonoBehaviour
{
    public TileType type = TileType.None;
    public bool isTowerAttached = false;
    public GameObject tower = null;

    public void AttachTower(GameObject newTower)
    {
        tower = newTower;
        newTower.transform.position = this.gameObject.transform.position;
        isTowerAttached = true;
    }

    public void DetachTower()
    {
        if (tower)
        {
            isTowerAttached = false;
            Destroy(tower);
            tower = null;
        }
    }
}
