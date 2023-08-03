using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class TowerBuilder : MonoBehaviour
{
    [SerializeField]
    private Image indicator;
    [SerializeField]
    private List<GameObject> indicateList;
    
    private int curIndicatedIndex = 0;
    public int CurIndicatedIndex { get { return curIndicatedIndex; } }

    [SerializeField]
    private List<GameObject> towerObjs;
    private List<GameObject> spawnedtowerList;
    private GameObject spawnedTowersObj;

    private void Awake()
    {
        indicator = transform.Find("Indicator").GetComponent<Image>();


        spawnedtowerList = new List<GameObject>();
        spawnedTowersObj = GameObject.Find("SpawnedTowers").gameObject;
    }


    public void SetIndicatorPosition(GameObject button)
    {
        Image buttonImg = button.GetComponent<Image>();
        indicator.rectTransform.position = buttonImg.rectTransform.position;
    }

    public void SetIndicateType(int type)
    {
        curIndicatedIndex = type;
    }

    public void SpawnTower(Vector3 spawnPosition, Tile baseTile)
    {
        // indicate리스트 0인덱스에 TowerBase있어서 -1
        GameObject newTower = Instantiate<GameObject>(towerObjs[curIndicatedIndex - 1]);
        newTower.transform.parent = spawnedTowersObj.transform;
        spawnedtowerList.Add(newTower);
        baseTile.AttachTower(newTower);
    }

    public void DestroyTower(Tile baseTile)
    {
        spawnedtowerList.Remove(baseTile.tower);
        baseTile.DetachTower();
    }

    public void AllTowersPlay(bool isPlay)
    {
        if (spawnedtowerList.Count == 0)
        {
            return;
        }

        foreach (GameObject towerObj in spawnedtowerList)
        {
            towerObj.GetComponent<Tower>().SetPlay(isPlay);
        }
    }
}
