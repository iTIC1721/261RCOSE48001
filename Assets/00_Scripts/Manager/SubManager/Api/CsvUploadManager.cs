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
    [SerializeField] private Button resultConfirmButton;
    [SerializeField] private Button resultCancelButton;

    [Header("로딩")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;

    private bool _isGuideMode = false;

    private void Start()
    {
        uploadButton.onClick.AddListener(OnUploadButtonClicked);
        resultConfirmButton.onClick.AddListener(OnConfirmClicked);
        resultCancelButton.onClick.AddListener(OnCancelClicked);
        resultPanel.SetActive(false);
    }

    private void OnUploadButtonClicked()
    {
        _isGuideMode = true;
        ShowResult(
            false,
            "단어 업로드",
            "csv파일의 첫 번째 줄은 반드시 칼럼명이어야 합니다.\n\n" +
            "• 단어 칼럼명: word\n" +
            "• 뜻 칼럼명: meaning (선택)\n\n" +
            "확인을 누르면 파일 선택 창이 열립니다."
        );
    }

    private void OnConfirmClicked()
    {
        resultPanel.SetActive(false);

        if (_isGuideMode)
        {
            _isGuideMode = false;
            NativeFilePicker.PickFile(OnFilePicked, new string[] {
                "text/csv",
                "text/plain",
                "text/comma-separated-values",
                "application/csv",
                "*/*"
            });
        }
    }

    private void OnCancelClicked()
    {
        _isGuideMode = false;
        resultPanel.SetActive(false);
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