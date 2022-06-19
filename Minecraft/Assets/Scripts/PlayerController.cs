using UnityEngine;

public class PlayerController : MonoBehaviour {

    public bool IsGround;
    public bool IsMove;
    public int RotateSpeed = 30;
    
    [SerializeField] 
    private WorldSupervisor World;
    [SerializeField] 
    private FootStepsController FootStepsControllerScript = null;

    public float WalkSpeed = 3f;
    public float SprintSpeed = 6f;
    public float JumpForce = 5f;
    public float Gravity = -9.8f;

    public float PlayerWidth = 0f;

    public Transform MainCamera;
    
    private Vector3 _velocity;
    private float _moveY;
    private Animator _animator;
    private bool _isThirdPersonCameraMode;
    private bool _isMoving;
    private float _widthOffset = 0.1f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _animator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate() 
    {
        CalculateVelocity();
        CheckRotation();
    }

    private void Update()
    {
        CheckInputs();
    }

    private void CheckRotation()
    {
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * RotateSpeed);
        
        Vector3 rotateVector = Vector3.right * -Input.GetAxis("Mouse Y") * RotateSpeed;
        float camXRotation = MainCamera.transform.eulerAngles.x + rotateVector.x;
        if (camXRotation <= 361 && camXRotation >= 310 || camXRotation >= -1 &&  camXRotation < 70)
            MainCamera.Rotate(Vector3.right * -Input.GetAxis("Mouse Y") * RotateSpeed);
    }
    
    
    private void Jump () 
    {
        _moveY = JumpForce;
        IsGround = false;
    }

    private void CalculateVelocity () 
    {
        if (_moveY > Gravity)
            _moveY += Time.fixedDeltaTime * Gravity;
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (horizontal != 0.0f || vertical != 0.0f)
        {
            Walk();
            _isMoving = true;
        }
        else if (_isMoving)
        {
            Stop();
            _isMoving = false;
        }
        
        if (IsMove)
            _velocity = (transform.forward * vertical + transform.right * horizontal) * (Time.fixedDeltaTime * SprintSpeed);
        else
            _velocity = (transform.forward * vertical + transform.right * horizontal) * (Time.fixedDeltaTime * WalkSpeed);
        
        _velocity += Vector3.up * _moveY * Time.fixedDeltaTime;

        if (_velocity.z > 0 && Front || _velocity.z < 0 && Back)
            _velocity.z = 0;
        if (_velocity.x > 0 && Right || _velocity.x < 0 && Left)
            _velocity.x = 0;

        if (_velocity.y < 0)
            _velocity.y = Down(_velocity.y);
        else if (_velocity.y > 0)
            _velocity.y = Up(_velocity.y);

        transform.Translate(_velocity, Space.World);
    }

    private void CheckInputs () 
    {
        if (Input.GetButtonDown("Sprint"))
            IsMove = true;
        if (Input.GetButtonUp("Sprint"))
            IsMove = false;

        if (IsGround && Input.GetButtonDown("Jump"))
            Jump();

        if (Input.GetKeyDown(KeyCode.W))
            Walk();
        if (Input.GetKeyUp(KeyCode.W))
            Stop();

        /*if (Input.GetKeyUp(KeyCode.F3))
            SwitchCameraMode();*/            
    }

    private void SwitchCameraMode()
    {
        Vector3 changeVec;
        if (!_isThirdPersonCameraMode)
        {
            changeVec = new Vector3(MainCamera.position.x - MainCamera.forward.x * 2,
                MainCamera.position.y + 2f, MainCamera.position.z - MainCamera.forward.z * 2);
        }
        else
        {
            changeVec = new Vector3(MainCamera.position.x + MainCamera.forward.x * 2,
                MainCamera.position.y - 2f, MainCamera.position.z + MainCamera.forward.z * 2);
        }
        MainCamera.position = changeVec;

        _isThirdPersonCameraMode = !_isThirdPersonCameraMode;
    }

    private float Down (float downSpeed)
    {
        if (
            World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth, 
                transform.position.y + downSpeed, transform.position.z + PlayerWidth)) ||
            World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth, 
                transform.position.y + downSpeed, transform.position.z - PlayerWidth)) ||
            World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth, 
                transform.position.y + downSpeed, transform.position.z - PlayerWidth)) ||
            World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth, 
                transform.position.y + downSpeed, transform.position.z + PlayerWidth)) 
        )
        {
            if (!IsGround)
            {
                FootStepsControllerScript.Step();
                IsGround = true;
            }
            return 0;
        }

        IsGround = false;
        return downSpeed;
    }

    private float Up (float upSpeed)
    {
        if (
            World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth, 
                transform.position.y + 2f + upSpeed, transform.position.z - PlayerWidth)) ||
            World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth, 
                transform.position.y + 2f + upSpeed, transform.position.z - PlayerWidth)) ||
            World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth, 
                transform.position.y + 2f + upSpeed, transform.position.z + PlayerWidth)) ||
            World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth, 
                transform.position.y + 2f + upSpeed, transform.position.z + PlayerWidth))
           ) 
        {
            return 0;
        }

        return upSpeed;
    }

    private bool Front =>
        World.CheckBlockCollision(new Vector3(transform.position.x, transform.position.y, 
            transform.position.z + PlayerWidth + _widthOffset)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x, transform.position.y + 1.6f, 
            transform.position.z + PlayerWidth + _widthOffset)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth, transform.position.y, 
            transform.position.z + PlayerWidth + _widthOffset)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth, transform.position.y + 1.6f, 
            transform.position.z + PlayerWidth + _widthOffset)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth, transform.position.y, 
            transform.position.z + PlayerWidth + _widthOffset)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth, transform.position.y + 1.6f, 
            transform.position.z + PlayerWidth + _widthOffset));

    private bool Back =>
        World.CheckBlockCollision(new Vector3(transform.position.x, 
            transform.position.y, transform.position.z - PlayerWidth - _widthOffset)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x, 
            transform.position.y + 1.6f, transform.position.z - PlayerWidth - _widthOffset)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth, 
            transform.position.y + 1.6f, transform.position.z - PlayerWidth - _widthOffset)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth, 
            transform.position.y + 1.6f, transform.position.z - PlayerWidth - _widthOffset)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth, 
            transform.position.y + 1.6f, transform.position.z - PlayerWidth - _widthOffset)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth, 
            transform.position.y + 1.6f, transform.position.z - PlayerWidth - _widthOffset));

    private bool Left =>
        World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth - _widthOffset,
            transform.position.y, transform.position.z)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth - _widthOffset,
            transform.position.y + 1.6f, transform.position.z)) ||    
        World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth - _widthOffset,
            transform.position.y, transform.position.z + PlayerWidth + _widthOffset)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth - _widthOffset,
            transform.position.y + 1.6f, transform.position.z + PlayerWidth)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth - _widthOffset,
            transform.position.y, transform.position.z - PlayerWidth )) ||
        World.CheckBlockCollision(new Vector3(transform.position.x - PlayerWidth - _widthOffset,
            transform.position.y + 1.6f, transform.position.z - PlayerWidth));

    private bool Right =>
        World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth + _widthOffset,
            transform.position.y, transform.position.z)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth + _widthOffset,
            transform.position.y + 1.6f, transform.position.z)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth + _widthOffset,
            transform.position.y, transform.position.z + PlayerWidth)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth + _widthOffset,
            transform.position.y + 1.6f, transform.position.z + PlayerWidth)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth + _widthOffset,
            transform.position.y, transform.position.z - PlayerWidth)) ||
        World.CheckBlockCollision(new Vector3(transform.position.x + PlayerWidth + _widthOffset,
            transform.position.y + 1.6f, transform.position.z - PlayerWidth));


    private void Run() 
    {
        _animator.SetBool ("Walk", false);
        _animator.SetBool ("Run", true);
    }

    private void Walk() 
    {
        _animator.SetBool ("Run", false);
        _animator.SetBool ("Walk", true);
    }

    private void Stop()
    {
        _animator.SetBool ("Run", false);
        _animator.SetBool ("Walk", false);
    }
}
