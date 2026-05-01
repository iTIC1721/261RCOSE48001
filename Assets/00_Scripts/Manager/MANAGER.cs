using System;
using Unity.VisualScripting;
using UnityEngine;

public class MANAGER : MonoBehaviour
{
    public static MANAGER Instance { get; private set; }

    public static PoolManager Pool;
    public static StudyManager StudyManager;
    public static InventoryManager Inventory;
    public static DBManager DB;
    public static GameManager Game;

    public string dataPath;
    public bool useCustomTime = false;
    public int year = DateTime.Now.Year;
    public int month = DateTime.Now.Month;
    public int day = DateTime.Now.Day;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            StudyManager = GetComponentInChildren<StudyManager>();
            Pool = GetComponentInChildren<PoolManager>();
            Inventory = GetComponentInChildren<InventoryManager>();
            DB = GetComponentInChildren<DBManager>();
            Game = GetComponentInChildren<GameManager>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CustomTime.useCustomTime = useCustomTime;
        CustomTime.customToday = new DateTime(year, month, day);
    }
}
