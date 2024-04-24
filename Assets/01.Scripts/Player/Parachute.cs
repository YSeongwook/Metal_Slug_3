using System.Collections;
using UnityEngine;

public class Parachute : MonoBehaviour
{
    private FlashUsingMaterial flashMaterial;
    private bool parachuteFolded = false; // 낙하산이 펼쳐졌는지 여부
    private Animator animator; // 애니메이터 컴포넌트
    private Transform playerTransform; // 플레이어의 트랜스폼

    public float speed = 1.0f; // 속도
    public float distance = 1.0f; // 날아갈 거리

    void Start()
    {
        flashMaterial = GetComponent<FlashUsingMaterial>();
        animator = GetComponent<Animator>();
        playerTransform = transform.parent; // 플레이어를 낙하산의 부모로 가정

        flashMaterial.FlashForDuration(3f);
    }

    void Update()
    {
        // A 키를 누르고 낙하산이 펼쳐져 있다면
        if (Input.GetKeyDown(KeyCode.A) && !parachuteFolded)
        {
            // 낙하산 해제
            DetachParachute();
        }
    }

    // 낙하산 해제 메서드
    public void DetachParachute()
    {
        parachuteFolded = true;
        animator.SetBool("Detached", true);
        // Coroutine 시작
        StartCoroutine(DescentCoroutine());
    }

    public void GroundedParachute()
    {
        parachuteFolded = true;
        animator.SetBool("Grounded", true);
        // Coroutine 시작
        StartCoroutine(DescentCoroutine());
    }

    // Coroutine으로 사용할 감속 메서드
    IEnumerator DescentCoroutine()
    {
        float currentDistance = 0.0f; // 현재 이동한 거리
        Vector3 initialPosition = transform.position; // 초기 위치 저장

        // descentDistance까지 이동할 때까지 반복
        while (currentDistance < distance)
        {
            // 감속 계산
            float delta = speed * Time.deltaTime;
            currentDistance += delta;

            // position.x 값을 천천히 감소하고 position.y는 초기 위치로 설정
            transform.position = new Vector3(initialPosition.x - currentDistance, initialPosition.y, initialPosition.z);

            yield return null; // 한 프레임 기다림
        }
    }
}
