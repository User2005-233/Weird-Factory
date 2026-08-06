using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoSingleton<CameraController>
{
    [Header("Angle")]
    [SerializeField, Range(30f, 80f)] float tiltAngle = 55f;

    [Header("Zoom")]
    [SerializeField] float zoomMin = 5f;
    [SerializeField] float zoomMax = 30f;
    [SerializeField] float zoomDefault = 15f;
    [SerializeField] float zoomSpeed = 0.1f;

    [Header("Pan")]
    [SerializeField] float panSpeed = 10f;
    [SerializeField] float panSpeedFast = 25f;
    [SerializeField, Range(0f, 1f)] float edgePanThreshold = 0.05f;

    [Header("Bounds")]
    [SerializeField] int gridWidth = 20;
    [SerializeField] int gridHeight = 20;
    [SerializeField] float mapPadding = 5f;

    GameControls gameControls;

    Camera cam;
    Vector2 moveInput;
    float zoomInput;
    bool isMiddleDrag;
    bool isFastPan;
    Vector3 pivot;
    float currentZoom;

    float Aspect => (float)Screen.width / Screen.height;

    protected override void Awake()
    {
        base.Awake();
        cam = GetComponent<Camera>();
        if (cam == null) cam = gameObject.AddComponent<Camera>();
        cam.orthographic = true;
    }

    void OnEnable()
    {
        gameControls = new GameControls();
        gameControls.Enable();
        gameControls.CameraActions.Move.performed += OnMove;
        gameControls.CameraActions.Move.canceled += OnMove;
        gameControls.CameraActions.Zoom.performed += OnZoom;
        gameControls.CameraActions.MiddleDrag.performed += OnMiddleDrag;
        gameControls.CameraActions.MiddleDrag.canceled += OnMiddleDrag;
        gameControls.CameraActions.FastPan.performed += OnFastPan;
        gameControls.CameraActions.FastPan.canceled += OnFastPan;
    }

    void OnDisable()
    {
        gameControls?.Disable();
    }

    void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    void OnZoom(InputAction.CallbackContext ctx) => zoomInput = ctx.ReadValue<float>();
    void OnMiddleDrag(InputAction.CallbackContext ctx) => isMiddleDrag = ctx.performed;
    void OnFastPan(InputAction.CallbackContext ctx) => isFastPan = ctx.performed;

    void Start()
    {
        pivot = new Vector3(gridWidth * 0.5f, 0f, gridHeight * 0.5f);
        currentZoom = zoomDefault;
        ApplyPositionAndZoom();
    }

    void LateUpdate()
    {
        HandleKeyboardPan();
        HandleMiddleDrag();
        HandleEdgePan();
        HandleZoom();
        ClampPivot();
        ApplyPositionAndZoom();
    }

    void HandleKeyboardPan()
    {
        if (moveInput == Vector2.zero) return;
        float speed = isFastPan ? panSpeedFast : panSpeed;
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y) * speed * Time.deltaTime;
        pivot += move;
    }

    void HandleMiddleDrag()
    {
        if (!isMiddleDrag) return;
        Vector2 delta = Mouse.current.delta.ReadValue();
        Vector3 move = new Vector3(-delta.x, 0f, -delta.y) * panSpeed * 0.1f * Time.deltaTime;
        pivot += move;
    }

    void HandleEdgePan()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 move = Vector3.zero;
        float speed = isFastPan ? panSpeedFast : panSpeed;

        if (mousePos.x <= Screen.width * edgePanThreshold)
            move += Vector3.left;
        if (mousePos.x >= Screen.width * (1f - edgePanThreshold))
            move += Vector3.right;
        if (mousePos.y <= Screen.height * edgePanThreshold)
            move += Vector3.back;
        if (mousePos.y >= Screen.height * (1f - edgePanThreshold))
            move += Vector3.forward;

        if (move != Vector3.zero)
            pivot += move.normalized * speed * Time.deltaTime;
    }

    void HandleZoom()
    {
        currentZoom -= zoomInput * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, zoomMin, zoomMax);
        zoomInput = 0f;
    }

    void ClampPivot()
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * Aspect;
        float pad = mapPadding;
        pivot.x = Mathf.Clamp(pivot.x, -pad, gridWidth + pad);
        pivot.z = Mathf.Clamp(pivot.z, -pad, gridHeight + pad);
    }

    void ApplyPositionAndZoom()
    {
        Vector3 dir = Quaternion.Euler(tiltAngle, 0f, 0f) * Vector3.back;
        cam.transform.position = pivot + dir * currentZoom;
        cam.transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);
        cam.orthographicSize = currentZoom;
    }

    public Vector3 Pivot => pivot;
    public float Zoom => currentZoom;
}
