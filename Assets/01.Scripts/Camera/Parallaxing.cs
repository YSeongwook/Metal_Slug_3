using UnityEngine;

public class Parallaxing : MonoBehaviour
{
    private Transform background;
    public float parallaxScaleX = -20;
    public float parallaxScaleY = 0;
    public float smoothing = 1f; 
    public bool isActive = true;

    private Transform cam;
    private Vector3 previousCamPos;

    void Awake()
    {
        cam = Camera.main.transform;
    }

    void Start()
    {
        background = GetComponent<Transform>();
        previousCamPos = cam.position;
    }

    void FixedUpdate()
    {
        if (isActive)
        {
            float parallaxX = (previousCamPos.x - cam.position.x) * parallaxScaleX;
            float parallaxY = (previousCamPos.y - cam.position.y) * parallaxScaleY;

            float backgroundTargetPosX = background.position.x + parallaxX;
            float backgroundTargetPosY = background.position.y + parallaxY;

            Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, backgroundTargetPosY, background.position.z);

            background.position = Vector3.Lerp(background.position, backgroundTargetPos, smoothing * Time.deltaTime);
        }

        previousCamPos = cam.position;
    }

    public void setActive(bool isActive)
    {
        this.isActive = isActive;
    }
}