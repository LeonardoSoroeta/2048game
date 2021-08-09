using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[DefaultExecutionOrder(-1)]
public class SwipeDetector : MonoBehaviour
{
    public static SwipeDetector instance;

    public PlayerControls playerControls;

    private Vector3 startPosition;
    private Vector3 currentPosition;

    private bool firstTouch = true;
    private bool detectingSwipe = false;

    private bool swipedUp = false;
    private bool swipedDown = false;
    private bool swipedLeft = false;
    private bool swipedRight = false;

    [SerializeField] private float minimumDistance = .2f;
    [SerializeField, Range(0f, 1f)] private float directionThreshold = .9f;

    private void Awake()
    {
        instance = this;
        playerControls = new PlayerControls();

    }

    private void Update()
    {
        if (detectingSwipe)
        {
            currentPosition = PrimaryPosition();

            DetectSwipe();
        }
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {
        playerControls.Touch.PrimaryContact.started += ctx => StartTouchPrimary(ctx);
        playerControls.Touch.PrimaryContact.canceled += ctx => EndTouchPrimary(ctx);
    }

    private void StartTouchPrimary(InputAction.CallbackContext context)
    {
        // to avoid a bug on first touch
        if (firstTouch)
        {
            firstTouch = false;
        }
        else
        {
            startPosition = PrimaryPosition();
            detectingSwipe = true;
        }

    }

    private void EndTouchPrimary(InputAction.CallbackContext context)
    {
        detectingSwipe = false;

    }

    private void DetectSwipe()
    {
        if (Vector3.Distance(startPosition, currentPosition) >= minimumDistance)
        {
            Debug.Log("SwipeDetected");
            Debug.Log("Start: " + startPosition);
            Debug.Log("Current: " + currentPosition);
            Vector3 direction = currentPosition - startPosition;
            Vector2 direction2D = new Vector2(direction.x, direction.y).normalized;
            detectingSwipe = false;
            SwipeDirection(direction2D);
        }
    }

    public Vector2 PrimaryPosition()
    {
        return ScreenToWorld(Camera.main, playerControls.Touch.PrimaryPosition.ReadValue<Vector2>());
        
    }

    public static Vector3 ScreenToWorld(Camera camera, Vector3 position)
    {
        position.z = camera.nearClipPlane;

        return camera.ScreenToWorldPoint(position);
    }

    private void SwipeDirection(Vector2 direction)
    {
        if (Vector2.Dot(Vector2.up, direction) > directionThreshold)
        {
            swipedUp = true;
        }
        else if (Vector2.Dot(Vector2.down, direction) > directionThreshold)
        {
            swipedDown = true;
        }
        else if (Vector2.Dot(Vector2.left, direction) > directionThreshold)
        {
            swipedLeft = true;
        }
        else if (Vector2.Dot(Vector2.right, direction) > directionThreshold)
        {
            swipedRight = true;
        }
    }

    public bool SwipedUp()
    {
        if (swipedUp == true)
        {
            swipedUp = false;
            return true;
        }
        return false;
    }

    public bool SwipedDown()
    {
        if (swipedDown == true)
        {
            swipedDown = false;
            return true;
        }
        return false;
    }

    public bool SwipedLeft()
    {
        if (swipedLeft == true)
        {
            swipedLeft = false;
            return true;
        }
        return false;
    }

    public bool SwipedRight()
    {
        if (swipedRight == true)
        {
            swipedRight = false;
            return true;
        }
        return false;
    }

}
