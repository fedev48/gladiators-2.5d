using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SelectionController : MonoBehaviour
{
    [SerializeField]List<Unit> selectedUnits = new List<Unit>();
    public static SelectionController Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InputController.Instance.OnAreaSelectionAreaStarts += MouseController_OnAreaSelectionAreaStarts;
        InputController.Instance.OnAreaSelectionAreaEnds += MouseController_OnAreaSelectionAreaEnds;
    }

    private void MouseController_OnAreaSelectionAreaEnds(object sender, EventArgs e)
    {
        Rect selectionAreaRect = InputController.Instance.GetSelectionAreaRect();
        if (selectionAreaRect.width + selectionAreaRect.height < 40)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                if (hitInfo.transform.TryGetComponent<Unit>(out Unit unit))
                {
                    if (!(Input.GetKey (KeyCode.LeftShift)||Input.GetKey (KeyCode.RightShift)))
                    {
                        ClearSelectedUnits();
                    }
                    AddSelectedUnit(unit);
                }else
                {
                    ClearSelectedUnits();
                }
            }
        }
        else
        {
            ClearSelectedUnits();
            GetUnitsInSelectionArea().ForEach(unit => AddSelectedUnit(unit));
        }
        
    }

    private void MouseController_OnAreaSelectionAreaStarts(object sender, EventArgs e)
    {
        
    }

    public void AddSelectedUnit(Unit unit)
    {
        if (!unit.IsOwner) return;
        selectedUnits.Add(unit);
        unit.ActivateOrDeactivateSelectedVisual(true);
    }

    public void ClearSelectedUnits()
    {
        
        foreach (Unit unit2 in selectedUnits)
        {
            unit2.ActivateOrDeactivateSelectedVisual(false);
        }
        selectedUnits.Clear();
    }

    public void MoveUnits(Vector3 target)
    {
        foreach(Unit unit in selectedUnits)
        {
            unit.PerformRightClickAction(target);
        }
    }

    private List<Unit> GetUnitsInSelectionArea()
    {
        Rect selectionAreaRect = InputController.Instance.GetSelectionAreaRect();
        List<Unit> unitsInSelectionArea = new List<Unit>();
        foreach (Unit unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(unit.transform.position);
            if (selectionAreaRect.Contains(screenPosition))
            {
                unitsInSelectionArea.Add(unit);
            }
        }
        return unitsInSelectionArea;
    }
  

}
