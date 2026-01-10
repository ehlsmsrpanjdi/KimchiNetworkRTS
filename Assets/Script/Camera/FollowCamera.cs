using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    static FollowCamera instance;
    //Player ownerPlayer;

    public static FollowCamera Instance
    {
        get { return instance; }
    }

    public Camera Camera { get; private set; }

    [Header("RTS Camera Settings")]
    [SerializeField] private float edgeScrollSize = 20f;           // 화면 가장자리 감지 범위 (픽셀)
    [SerializeField] private float scrollSpeed = 30f;              // 카메라 이동 속도
    [SerializeField] private float minX = -50f;                    // 카메라 이동 범위
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minZ = -50f;
    [SerializeField] private float maxZ = 50f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 10f;                // 줌 속도
    [SerializeField] private float minZoom = 10f;                  // 최소 높이 (가장 가까이)
    [SerializeField] private float maxZoom = 50f;                  // 최대 높이 (가장 멀리)
    [SerializeField] private float currentZoom = 30f;              // 현재 높이

    [Header("Optional: Keyboard Scroll")]
    [SerializeField] private bool useKeyboardScroll = true;        // WASD로도 이동 가능

    private void Awake()
    {
        instance = this;
        Camera = GetComponent<Camera>();

        // 초기 높이 설정
        Vector3 pos = transform.position;
        pos.y = currentZoom;
        transform.position = pos;
    }

    //public void SetOwner(Player _Player)
    //{
    //    ownerPlayer = _Player;
    //}

    private void Update()
    {
        ZoomCamera();

        if (useKeyboardScroll)
        {
            KeyboardScroll();
        }

        return;
        //EdgeScroll();


    }

    private void EdgeScroll()
    {
        Vector3 moveDirection = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;

        // 왼쪽 가장자리
        if (mousePos.x < edgeScrollSize)
        {
            moveDirection += Vector3.left;
        }
        // 오른쪽 가장자리
        else if (mousePos.x > Screen.width - edgeScrollSize)
        {
            moveDirection += Vector3.right;
        }

        // 아래쪽 가장자리
        if (mousePos.y < edgeScrollSize)
        {
            moveDirection += Vector3.back;
        }
        // 위쪽 가장자리
        else if (mousePos.y > Screen.height - edgeScrollSize)
        {
            moveDirection += Vector3.forward;
        }

        // 카메라 이동
        if (moveDirection != Vector3.zero)
        {
            Vector3 newPos = transform.position + moveDirection.normalized * scrollSpeed * Time.deltaTime;
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
            transform.position = newPos;
        }
    }

    private void ZoomCamera()
    {
        // 마우스 휠 입력
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            // 줌 인/아웃
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            // Y축 높이만 변경
            Vector3 newPos = transform.position;
            newPos.y = currentZoom;
            transform.position = newPos;
        }
    }

    private void KeyboardScroll()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveDirection += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveDirection += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveDirection += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveDirection += Vector3.right;
        }

        if (moveDirection != Vector3.zero)
        {
            Vector3 newPos = transform.position + moveDirection.normalized * scrollSpeed * Time.deltaTime;
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
            transform.position = newPos;
        }
    }
}