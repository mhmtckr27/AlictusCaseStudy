using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private float rayDistance = 1000;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask foodLayer;
    
    public event Action<Vector3> OnMovementInput;

    public bool isInputEnabled = false;
    bool isInputReceived;
    Vector2 touchPosition;

    private Vector2 fingerDown;
    private Vector2 fingerUp;
    public bool detectSwipeOnlyAfterRelease = false;

    public float swipeThreshold = 20f;
    private Vector2 initialPos;

    public bool isControlsReversed;

    Food foodToSwitch;
    public event Action<Food> OnFoodSelected;
    public event Action<Food, Direction> OnTrySwitchFoods;
    private void Awake()
    {
        if (GameManager.Instance.currentLevelController.Level == 3)
        {
            Destroy(GetComponent<Player>());
        }
    }

    void Update()
    {
        if (!isInputEnabled)
        {
            return;
        }
        isInputReceived = IsInputReceived(GameManager.Instance.currentLevelController.Level);

        if (isInputReceived)
        {
            switch (GameManager.Instance.currentLevelController.Level)
            {
                case 1:
                case 2:
                case 4:
                    Level1_2_4Input();
                    break;
                case 3:
                    Level3Input();
                    break;
                default:
                    break;
            }
        }
    }
    
    private bool IsInputReceived(int level)
    {
        bool isInputReceived = false;
#if UNITY_EDITOR
        switch (level)
        {
            case 1:
            case 2:
                isInputReceived = Input.GetMouseButton(0);
                touchPosition = Input.mousePosition;
                break;
            case 3:
                isInputReceived = Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0);
                break;
            case 4:
                isInputReceived = Input.GetMouseButton(0);
                touchPosition = Input.mousePosition;
                break;
            default:
                break;
        }
#else
        isInputReceived = Input.touchCount > 0;
        if (isInputReceived)
        {
            touchPosition = Input.GetTouch(0).position;
        }
#endif

        return isInputReceived;
    }

    private void Level1_2_4Input()
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, groundLayer);

        if (hits.Length > 0)
        {
            if (hits.Where(hit => hit.collider.gameObject.tag == "Player").ToList().Count == 0)
            {
                Vector3 moveToPos = new Vector3(hits[0].point.x, transform.position.y, hits[0].point.z);
                if (isControlsReversed)
                {
                    moveToPos.x *= -1;
                }
                OnMovementInput?.Invoke(moveToPos);
            }
        }
    }
    void Level3Input()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            initialPos = Input.mousePosition;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            Vector2 finalPos = Input.mousePosition; 
            Ray ray = Camera.main.ScreenPointToRay(initialPos);
            RaycastHit hit;
            foodToSwitch = null;
            if (Physics.Raycast(ray, out hit, rayDistance, foodLayer))
            {
                foodToSwitch = hit.collider.GetComponentInChildren<Food>();
                if (foodToSwitch)
                {
                    MouseSwipe(finalPos);
                }
            }
        }

#else
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            fingerUp = touch.position;
            fingerDown = touch.position;

            Ray ray = Camera.main.ScreenPointToRay(fingerDown);
            RaycastHit hit;
            foodToSwitch = null;
            if (Physics.Raycast(ray, out hit, rayDistance, foodLayer))
            {
                foodToSwitch = hit.collider.GetComponentInChildren<Food>();
            }
        }

        if (touch.phase == TouchPhase.Moved)
        {
            if (!detectSwipeOnlyAfterRelease)
            {
                fingerDown = touch.position;
                CheckSwipe();
            }
        }

        if (touch.phase == TouchPhase.Ended)
        {
            fingerDown = touch.position;
            CheckSwipe();
        }
#endif
    }



    private void Level4Input()
    {

    }



    void CheckSwipe()
    {
        if (VerticalMove() > swipeThreshold && VerticalMove() > HorizontalMove())
        {
            if (fingerDown.y - fingerUp.y > 0)
            {
                OnSwipeUp();
            }
            else if (fingerDown.y - fingerUp.y < 0)
            {
                OnSwipeDown();
            }
            fingerUp = fingerDown;
        }

        else if (HorizontalMove() > swipeThreshold && HorizontalMove() > VerticalMove())
        {
            if (fingerDown.x - fingerUp.x > 0)
            {
                OnSwipeRight();
            }
            else if (fingerDown.x - fingerUp.x < 0)
            {
                OnSwipeLeft();
            }
            fingerUp = fingerDown;
        }
    }

    void MouseSwipe(Vector3 finalPos)
    {
        float disX = Mathf.Abs(initialPos.x - finalPos.x);
        float disY = Mathf.Abs(initialPos.y - finalPos.y);
        if (disX > 0 || disY > 0)
        {
            if (disX > disY)
            {
                if (initialPos.x > finalPos.x)
                {
                    //Debug.Log("LEFT");
                    OnSwipeLeft();
                }
                else
                {
                    //Debug.Log("RIGHT");
                    OnSwipeRight();
                }
            }
            else
            {
                if (initialPos.y > finalPos.y)
                {
                    //Debug.Log("Down");
                    OnSwipeDown();
                }
                else
                {
                    //Debug.Log("UP");
                    OnSwipeUp();
                }
            }
        }
    }

    float VerticalMove()
    {
        return Mathf.Abs(fingerDown.y - fingerUp.y);
    }

    float HorizontalMove()
    {
        return Mathf.Abs(fingerDown.x - fingerUp.x);
    }

    void OnSwipeUp()
    {
        if(foodToSwitch != null)
        {
            OnTrySwitchFoods?.Invoke(foodToSwitch, Direction.Up);
        }
    }

    void OnSwipeDown()
    {
        if (foodToSwitch != null)
        {
            OnTrySwitchFoods?.Invoke(foodToSwitch, Direction.Down);
        }
    }

    void OnSwipeLeft()
    {
        if (foodToSwitch != null)
        {
            OnTrySwitchFoods?.Invoke(foodToSwitch, Direction.Left);
        }
    }

    void OnSwipeRight()
    {
        if (foodToSwitch != null)
        {
            OnTrySwitchFoods?.Invoke(foodToSwitch, Direction.Right);
        }
    }
    public void EnableInput(bool enable)
    {
        isInputEnabled = enable;
    }
}
