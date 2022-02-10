using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] [Range(1, 20)] private float moveSpeed = 1;
    [HideInInspector] public InputManager inputManager;

    CapsuleCollider hotDogCollider;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
    }

    public void Init(FoodType foodType)
    {
        Destroy(GetComponentInChildren<Rigidbody>());
        switch (foodType)
        {
            case FoodType.Banana:
                //workaround to fix an error about rigidbody and non convex mesh collider.
                Invoke(nameof(LateInit), 0.1f);
                break;
            case FoodType.Hotdog:
                hotDogCollider = GetComponentInChildren<CapsuleCollider>();
                Destroy(GetComponentInChildren<Food>());
                moveSpeed = 3;
                break;
            default:
                break;
        }

        foreach (Transform childTransform in GetComponentsInChildren<Transform>())
        {
            childTransform.gameObject.layer = 9;
        }
    }

    private void LateInit()
    {
        GetComponentInChildren<MeshCollider>().convex = false;
    }

    private void OnEnable()
    {
        inputManager.OnMovementInput += OnMovementInput;
    }

    private void OnMovementInput(Vector3 moveToPos)
    {
        switch (GameManager.Instance.currentLevelController.Level)
        {
            case 1:
            case 2:
                transform.LookAt(moveToPos);
                transform.position = GameManager.Instance.currentLevelController.bordersMovableArea.ClosestPointOnBounds(transform.position + (moveToPos - transform.position).normalized * Time.deltaTime * moveSpeed);
                break;
            case 4:
                Vector3 moveToUpdated = new Vector3();

                float minX = GameManager.Instance.currentLevelController.bordersMovableArea.bounds.min.x + hotDogCollider.height / 2 /** transform.localScale.x*/;
                float maxX = GameManager.Instance.currentLevelController.bordersMovableArea.bounds.max.x - hotDogCollider.height / 2 /** transform.localScale.x*/;

                if (moveToPos.x > maxX)
                {
                    moveToUpdated.x = maxX;
                } 
                else if(moveToPos.x < minX)
                {
                    moveToUpdated.x = minX;
                }
                else
                {
                    moveToUpdated.x = moveToPos.x;
                }

                moveToUpdated.y = transform.position.y;
                moveToUpdated.z = transform.position.z;

                transform.position = transform.position + (moveToUpdated - transform.position).normalized * Time.deltaTime * moveSpeed;
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        inputManager.OnMovementInput -= OnMovementInput;
    }
    
}
