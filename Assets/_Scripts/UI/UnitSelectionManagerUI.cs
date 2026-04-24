using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManagerUI : MonoBehaviour
{
    [SerializeField] private  RectTransform selectionAreaRectTransform;
    [SerializeField] private Canvas canvas;

    

    void Start()
    {
        InputController.Instance.OnAreaSelectionAreaStarts += MouseController_OnAreaSelectionAreaStarts;
        InputController.Instance.OnAreaSelectionAreaEnds += MouseController_OnAreaSelectionAreaEnds;
    }

    void Update()
    {
        if (selectionAreaRectTransform.gameObject.activeSelf)
        {
            UpdateVisual();
        }
    }
    private void MouseController_OnAreaSelectionAreaStarts(object sender, EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(true);
        UpdateVisual();
    }

    private void MouseController_OnAreaSelectionAreaEnds(object sender, EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void UpdateVisual()
    {
        Rect selectionAreaRect = InputController.Instance.GetSelectionAreaRect();
        float canvasScaleFactor = canvas.scaleFactor;
        selectionAreaRectTransform.anchoredPosition = new Vector2(
            selectionAreaRect.x,
            selectionAreaRect.y
        )/canvasScaleFactor;
        selectionAreaRectTransform.sizeDelta = new Vector2(
            selectionAreaRect.width,
            selectionAreaRect.height
        )/canvasScaleFactor;
    }

    
}
