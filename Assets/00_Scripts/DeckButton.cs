using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [HideInInspector] public string deckId;

    [Header("UI")]
    public RectTransform buttonImage;
    public TextMeshProUGUI deckNameText;
    public Button deleteButton;

    [Header("Setting")]
    [SerializeField] private float maxDragDistance = 200;
    [SerializeField] private float dragBackSpeed = 10f;

    private RectTransform rectTransform;
    private RectTransform parentRect;
    private Canvas canvas;

    private float dragStartX = 0;
    private float currentX = 0;     // -maxDragDistance ~ 0

    private bool isDragging = false;

    private UnityAction onClickEvent;

    private Coroutine dragBackCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = rectTransform.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();
    }

    public void AddEvent(UnityAction action)
    {
        onClickEvent = action;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dragBackCoroutine != null)
        {
            StopCoroutine(dragBackCoroutine);
            dragBackCoroutine = null;
        }

        Vector2 localPoint;

        // 현재 마우스 위치를 부모 기준 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        dragStartX = localPoint.x;

        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float scale = canvas.scaleFactor;
        Vector2 delta = eventData.delta / scale;

        // 이미지 이동
        float tmp = currentX + delta.x;
        currentX = Mathf.Clamp(tmp, -maxDragDistance, 0);

        buttonImage.anchoredPosition = new Vector2(currentX, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 localPoint;

        // 현재 마우스 위치를 부모 기준 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        float dragDistance = localPoint.x - dragStartX;
        if (dragDistance <= -maxDragDistance)
        {
            // 드래그된 채로 고정
            currentX = -maxDragDistance;
            buttonImage.anchoredPosition = new Vector2(currentX, 0);

            isDragging = false;
        }
        else
        {
            // 드래그 해제
            dragBackCoroutine = StartCoroutine(DragBackCoroutine());
        }
    }

    private IEnumerator DragBackCoroutine()
    {
        while (currentX < 0)
        {
            yield return null;

            currentX += dragBackSpeed * Time.deltaTime;
            buttonImage.anchoredPosition = new Vector2(currentX, 0);
        }

        currentX = 0;
        buttonImage.anchoredPosition = new Vector2(currentX, 0);

        isDragging = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return;

        onClickEvent?.Invoke();
    }
}
