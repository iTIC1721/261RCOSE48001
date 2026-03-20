using System;
using UnityEngine;

public class MANAGER : MonoBehaviour
{
    public static MANAGER Instance { get; private set; }

    public static StudyManager StudyManager;

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
