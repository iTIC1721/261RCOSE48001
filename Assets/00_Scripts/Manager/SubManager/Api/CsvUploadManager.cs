using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CsvUploadPanel : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Button uploadButton;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI resultDetailText;
    [SerializeField] private Button resultCloseButton;

    [Header("로딩")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Start()
    {
        uploadButton.onClick.AddListener(OnUploadButtonClicked);
        resultCloseButton.onClick.AddListener(() => resultPanel.SetActive(false));
        resultPanel.SetActive(false);
    }

    private void OnUploadButtonClicked()
    {
        NativeFilePicker.PickFile(OnFilePicked, new string[] { "text/csv", "text/plain" });
    }

    private void OnFilePicked(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        StartCoroutine(UploadRoutine(path));
    }

    private IEnumerator UploadRoutine(string path)
    {
        uploadButton.interactable = false;
        ShowLoading("단어 업로드 중...");

        int wordCount = 0;
        try
        {
            string[] lines = File.ReadAllLines(path);
            wordCount = Mathf.Max(0, lines.Length - 1);
        }
        catch (Exception e)
        {
            HideLoading();
            ShowResult(false, "파일 읽기 실패", e.Message);
            uploadButton.interactable = true;
            yield break;
        }

        yield return ApiManager.Instance.UploadCsv(
            filePath: path,
            onSuccess: _ => {
                HideLoading();
                ShowResult(true, "단어 추가 완료", $"{wordCount}개의 단어가 추가되었습니다.");
            },
            onError: err => {
                HideLoading();
                ShowResult(false, "업로드 실패", err);
            }
        );

        uploadButton.interactable = true;
    }

    private void ShowResult(bool success, string title, string detail)
    {
        resultTitleText.text = title;
        resultDetailText.text = detail;
        resultPanel.SetActive(true);
    }

    private void ShowLoading(string msg)
    {
        if (loadingPanel) loadingPanel.SetActive(true);
        if (loadingText) loadingText.text = msg;
    }

    private void HideLoading()
    {
        if (loadingPanel) loadingPanel.SetActive(false);
    }
}