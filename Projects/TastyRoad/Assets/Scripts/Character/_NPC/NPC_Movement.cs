using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Movement : MonoBehaviour
{
    [Header("")]
    [SerializeField] private NPC_Controller _controller;

    private Vector2 _targetPosition;
    public Vector2 targetPosition => _targetPosition;

    private Coroutine _moveCoroutine;

    private bool _roamActive;
    public bool roamActive => _roamActive;

    private bool _isLeaving;
    public bool isLeaving => _isLeaving;

    private SpriteRenderer _currentRoamArea;
    public SpriteRenderer currentRoamArea => _currentRoamArea;


    [Header("")]
    [SerializeField] private float _moveSpeed;

    [Header("")]
    [SerializeField] private Vector2 _intervalTimeRange;
    public Vector2 intervalTimeRange => _intervalTimeRange;

    [SerializeField][Range(0, 100)] private int _searchAttempts;
    public int searchAttempts => _searchAttempts;


    public delegate void Event();
    public Event TargetPosition_UpdateEvent;


    // UnityEngine
    private void Awake()
    {
        if (gameObject.TryGetComponent(out NPC_Controller controller)) { _controller = controller; }

        _targetPosition = transform.position;
    }

    private void Update()
    {
        _controller.basicAnim.Idle_Move(Is_Moving());
        TargetPosition_Movement();
    }


    // Get
    public float Move_Direction()
    {
        // return left
        if (transform.position.x > _targetPosition.x) return -1f;
        // return right
        else return 1f;
    }

    public float Random_IntervalTime()
    {
        return Random.Range(_intervalTimeRange.x, _intervalTimeRange.y);
    }


    // Check
    public bool Is_Moving()
    {
        if (At_TargetPosition() == false) return true;
        else return false;
    }

    public bool At_TargetPosition()
    {
        float threshold = 0.1f;
        float distance = Vector2.Distance(transform.position, _targetPosition);
        return distance < threshold;
    }
    public bool At_TargetPosition(Vector2 targetPosition)
    {
        float threshold = 0.1f;
        float distance = Vector2.Distance(transform.position, targetPosition);
        return distance < threshold;
    }

    public bool At_CurrentRoamArea()
    {
        return currentRoamArea.bounds.Contains(transform.position);
    }


    // Movement Update
    private void TargetPosition_Movement()
    {
        if (At_TargetPosition() == false)
        {
            transform.position = Vector2.MoveTowards(transform.position, _targetPosition, _moveSpeed * 0.1f * Time.deltaTime);
        }
    }


    /// <summary>
    /// updates the current roam area variable
    /// </summary>
    public void Update_RoamArea(SpriteRenderer assignArea)
    {
        _currentRoamArea = assignArea;
    }


    /// <summary>
    /// Moves to assign position
    /// </summary>
    public void Assign_TargetPosition(Vector2 assignPosition)
    {
        _targetPosition = assignPosition;
        _controller.basicAnim.Flip_Sprite(Move_Direction());

        TargetPosition_UpdateEvent?.Invoke();
    }

    /// <summary>
    /// Attempts to find path cleared position
    /// </summary>
    public void Assign_TargetPosition(SpriteRenderer searchArea)
    {
        int attemptCount = _searchAttempts;
        Vector2 targetPosition;

        do
        {
            targetPosition = Main_Controller.Random_AreaPoint(searchArea);
            bool stationPlaced = _controller.mainController.Is_StationArea(targetPosition);

            if (stationPlaced == false)
            {
                Assign_TargetPosition(targetPosition);
                return;
            }
            attemptCount--;
        }
        while (attemptCount > 0);

        Leave(Random_IntervalTime());
    }

    /// <summary>
    /// Moves to assign position, after roamReturnTime pass by, returns back to Free roam
    /// </summary>
    public void Assign_TargetPosition(Vector2 assignPosition, float roamReturnTime, SpriteRenderer roamReturnArea)
    {
        _moveCoroutine = StartCoroutine(Assign_TargetPosition_Coroutine(assignPosition, roamReturnTime, roamReturnArea));
    }
    private IEnumerator Assign_TargetPosition_Coroutine(Vector2 assignPosition, float roamReturnTime, SpriteRenderer roamReturnArea)
    {
        Assign_TargetPosition(assignPosition);

        while (At_TargetPosition() == false)
        {
            yield return null;
        }

        Free_Roam(roamReturnArea, roamReturnTime);
    }


    /// <summary>
    /// Updates and moves to a random target position inside roam area on every interval time
    /// </summary>
    public void Free_Roam(SpriteRenderer roamArea)
    {
        Free_Roam(roamArea, Random_IntervalTime());
    }
    public void Free_Roam(SpriteRenderer roamArea, float startDelayTime)
    {
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);

        _roamActive = true;
        _currentRoamArea = roamArea;
        _moveCoroutine = StartCoroutine(Free_Roam_Coroutine(roamArea, startDelayTime));
    }
    private IEnumerator Free_Roam_Coroutine(SpriteRenderer roamArea, float startDelayTime)
    {
        yield return new WaitForSeconds(startDelayTime);
        // Assign_TargetPosition(Main_Controller.Random_AreaPoint(roamArea));
        Assign_TargetPosition(roamArea);

        // repeat until free roam deactivates
        while (_roamActive == true)
        {
            // wait until NPC reaches target position
            while (At_TargetPosition() == false)
            {
                yield return null;
            }

            float randIntervalTime = Random.Range(_intervalTimeRange.x, _intervalTimeRange.y);

            yield return new WaitForSeconds(randIntervalTime);
            // Assign_TargetPosition(Main_Controller.Random_AreaPoint(roamArea));
            Assign_TargetPosition(roamArea);
        }
    }

    public void Stop_FreeRoam()
    {
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);

        _roamActive = false;
        _targetPosition = transform.position;
    }


    public void Leave(float startDelayTime)
    {
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);

        _isLeaving = true;
        _moveCoroutine = StartCoroutine(Leave_Coroutine(Random.Range(0, 2), startDelayTime));
    }

    /// <summary>
    /// 0 for left, 1 for right
    /// </summary>
    public void Leave(int direction, float startDelayTime)
    {
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);

        _isLeaving = true;
        _moveCoroutine = StartCoroutine(Leave_Coroutine(direction, startDelayTime));
    }
    private IEnumerator Leave_Coroutine(int direction, float startDelayTime)
    {
        yield return new WaitForSeconds(startDelayTime);

        Main_Controller main = _controller.mainController;

        // random left or right side of camera
        Vector2 targetPosition = main.currentLocation.OuterLocation_Position(direction);

        // assign target position
        Assign_TargetPosition(targetPosition);

        // wait until npc reaches outer camera position
        while (At_TargetPosition(targetPosition) == false)
        {
            yield return null;
        }

        // untrack, destroy
        main.UnTrack_CurrentCharacter(gameObject);
        Destroy(gameObject);
    }
}