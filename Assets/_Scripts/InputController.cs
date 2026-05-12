using System;
using UnityEngine;

public class InputController : MonoBehaviour
{

    private Vector2 startingPosition;
    public static InputController Instance; 
    public event EventHandler OnAreaSelectionAreaStarts;
    public event EventHandler OnAreaSelectionAreaEnds;
    public event EventHandler OnMouseRightClickDown;
    public event EventHandler OnMouseRightClickUp;
    public event EventHandler<NumDownInfo> OnNumKeyDown;
    public class NumDownInfo: EventArgs
    {
        public int numKey;
        public NumDownInfo(int numKey)
        {
            this.numKey = numKey;
        }
    }
   
   
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

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startingPosition = Input.mousePosition;
            OnAreaSelectionAreaStarts?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endingPosition = Input.mousePosition;
            OnAreaSelectionAreaEnds?.Invoke(this, EventArgs.Empty);
        }


        if (Input.GetMouseButtonDown(1))
        {

            OnMouseRightClickDown?.Invoke(this, EventArgs.Empty);
            SelectionController.Instance.MoveUnits(GetMouseWorldPosition(false));
        }

        if (Input.GetMouseButtonUp(1))
        {
           OnMouseRightClickUp?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Alpha5))
        {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    OnNumKeyDown?.Invoke(this, new NumDownInfo(1));
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    OnNumKeyDown?.Invoke(this, new NumDownInfo(2)); 
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    OnNumKeyDown?.Invoke(this, new NumDownInfo(3));
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    OnNumKeyDown?.Invoke(this, new NumDownInfo(4));
                }
                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    OnNumKeyDown?.Invoke(this, new NumDownInfo(5));
                }
        }

    }

    public Rect GetSelectionAreaRect()
    {
        Vector2 endMousePosition = Input.mousePosition;

        Vector2 lowerLeftCorner = new Vector2(
            Mathf.Min(endMousePosition.x, startingPosition.x),
            Mathf.Min(endMousePosition.y, startingPosition.y)
        );
        Vector2 upperRightCorner = new Vector2(
            Mathf.Max(endMousePosition.x, startingPosition.x),
            Mathf.Max(endMousePosition.y, startingPosition.y)
        );
        
        
        return new Rect(lowerLeftCorner.x, lowerLeftCorner.y, upperRightCorner.x - lowerLeftCorner.x, upperRightCorner.y - lowerLeftCorner.y);
    }

    public Vector3 GetMouseWorldPosition(bool colideOnlyWithGround = true)
    {
        if (colideOnlyWithGround){
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                return hitInfo.point;
            }
            return Vector3.zero;
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                
                return hitInfo.point;
            }
            return Vector3.zero;
        }
    }
}
