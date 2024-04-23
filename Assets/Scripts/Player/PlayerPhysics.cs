using System.Collections;
using UnityEngine;
using Utils;

public class SlugPhysics : MonoBehaviour
{
    private Collider2D collider;
    private IObserver[] observers;

    public float groundDrag = 0;     // 땅에서의 드래그
    public float airDrag = 0.998f;     // 공중에서의 드래그
    public float initialJumpVelocity = 3f;     // 초기 점프 속도
    public float maxVerticalVelocity = -3;     // 최대 수직 속도
    public float verticalDrag = 7f;     // 수직 드래그
    public float bounceFactor = 0;     // 튕김 계수

    public float groundMovementFactor = 1.1f;     // 땅에서의 이동 계수
    public float airLowVelocityMovementFactor = 0.95f;     // 공중에서의 저속 이동 계수
    public float airHighVelocityMovementFactor = 1.4f;     // 공중에서의 고속 이동 계수
    private const float rayCastRestLength = 0.03f;     // 레이캐스트의 길이
    private float movementFactor = 1.1f; //     // 이동 계수, TODO 초기화 시점의 이동 계수는 groundMovementFactor와 동일해야 함
    private const float maxSlope = 0.8f;     // 최대 경사

    private Vector2 velocity;     // 속도

    // 레이캐스트 히트 정보 배열
    private RaycastHit2D[] rayCastHit = new RaycastHit2D[1];
    private RaycastHit2D inFrontOfMe;

    public bool InTheAir { get; set; }  // 공중 여부
    private Vector2 groundSlope;     // 땅의 경사
    private float forceX;     // X 축으로의 힘

    // Y, X 변위
    private float Y;
    private float xTranslation;
    private float penteY;

    public bool debugging;     // 디버깅 모드
    public LayerMask linecastLayerMask;     // 레이캐스트에 사용될 레이어 마스크

    void Awake()
    {
        collider = GetComponent<Collider2D>();
        observers = GetComponents<IObserver>();
        velocity = new Vector2();
        InTheAir = false;
    }

    // 디버그용 출력 함수
    void myPrint(string str)
    {
        if (debugging)
        {
            print(str);
        }
    }

    void Update()
    {
        // 1 - 속도 업데이트
        CalculateVelocity();

        // 2 - 현재 경사에 따른 변위 계산
        Vector2 groundSlope;
        if (WhatIsUnderMyFeet(Vector2.zero) > 0)
        {
            groundSlope = GetSlopeFromRayCastHid2D(rayCastHit[0]);
        }
        else
        {
            groundSlope = Vector2.zero;
        }
        Vector2 transCandidate = CalculateTranslation(groundSlope);

        // 3 - 다음 프레임의 충돌에 따른 변위 조정
        // 3.1 - x 축 조정
        if (WhatIsInFrontOfMe(transCandidate) > 0)
        {
            Vector2 facingWallslope = GetSlopeFromRayCastHid2D(rayCastHit[0]);
            if (Mathf.Abs(facingWallslope.y) > maxSlope)
            {
                transCandidate.x = FixXTrans(rayCastHit[0]);
                if (!InTheAir)
                {
                    transCandidate = Vector2.zero;
                    transform.Translate(transCandidate.x, transCandidate.y, 0, Space.World);
                }
            }
        }
        // 3.2 - y 축 조정
        if (WhatIsUnderMyFeet(transCandidate) > 0)
        {
            Vector2 futurUnderslope = GetSlopeFromRayCastHid2D(rayCastHit[0]);
            if (InTheAir)
            {
                if (velocity.y < 0)
                { // 착지
                    if (Mathf.Abs(futurUnderslope.y) < maxSlope)
                    {
                        StopFalling();
                    }
                    transCandidate.y = FixYTrans(rayCastHit[0]);
                }
            }
            else
            {
                transCandidate.y = FixYTrans(rayCastHit[0]); // 땅의 곡선을 따라 이동
            }
        }
        else if (!InTheAir)
        {
            StartFalling();
        }
    }

    // 현재 경사에 따른 변위 계산
    Vector2 CalculateTranslation(Vector2 groundSlope)
    {
        Vector2 trans = new Vector2();
        if (InTheAir && groundSlope != Vector2.zero && velocity.y < 0)
        { // 가파른 경사를 타고 미끄러짐
            trans.x = 1.8f * Time.deltaTime * Mathf.Abs(groundSlope.x);
            trans.y = -1.8f * Time.deltaTime * Mathf.Abs(groundSlope.y);
        }
        else if (InTheAir)
        { // 낙하 또는 상승
            trans.x = velocity.x * movementFactor * Time.deltaTime;
            trans.y = velocity.y * Time.deltaTime;
        }
        else
        { // 땅 위에 있음
            trans.x = velocity.x * movementFactor * Time.deltaTime * Mathf.Abs(groundSlope.x);
            trans.y = Mathf.Abs(velocity.x) * movementFactor * Time.deltaTime * groundSlope.y;
        }
        myPrint(trans.x + " " + trans.y);
        return trans;
    }

    // 바닥 아래에 있는 것이 무엇인지 확인
    int WhatIsUnderMyFeet(Vector2 trans)
    {
        Vector2 endPoint = new Vector2(collider.bounds.center.x + trans.x, collider.bounds.min.y + trans.y - 0.03f);
        Vector2 startPoint = new Vector2(endPoint.x, collider.bounds.min.y + 0.03f);
        // 한 번에 하나의 히트만 지원하므로 현재는 더 필요하지 않음
        int hitCount = Physics2D.LinecastNonAlloc(startPoint, endPoint, rayCastHit, linecastLayerMask);
        // Debug.DrawLine(startPoint, endPoint);
        return hitCount;
    }

    // 앞에 있는 것이 무엇인지 확인
    int WhatIsInFrontOfMe(Vector2 trans)
    {
        Bounds bounds = collider.bounds;
        float startX = bounds.center.x;

        Vector2 startPoint = new Vector2(startX, bounds.min.y);
        Vector2 endPoint = startPoint + new Vector2(trans.x, trans.y);

        int hitCount = Physics2D.LinecastNonAlloc(startPoint, endPoint, rayCastHit, linecastLayerMask);
        Debug.DrawLine(startPoint, endPoint);
        if (hitCount == 0)
        {
            startPoint = new Vector2(startX, bounds.max.y);
            endPoint = startPoint + new Vector2(trans.x, trans.y);
            hitCount = Physics2D.LinecastNonAlloc(startPoint, endPoint, rayCastHit, linecastLayerMask);
        }
        return hitCount;
    }

    // X 변위 조정
    float FixXTrans(RaycastHit2D hit)
    {
        if (transform.right == Vector3.left)
        {
            return rayCastHit[0].point.x - collider.bounds.center.x + 0.03f;
        }
        else
        {
            return rayCastHit[0].point.x - collider.bounds.center.x - 0.03f;
        }
    }

    // Y 변위 조정
    float FixYTrans(RaycastHit2D hit)
    {
        return hit.point.y - collider.bounds.min.y + 0.005f;
    }

    // 레이캐스트 히트로부터 경사 구하기
    Vector2 GetSlopeFromRayCastHid2D(RaycastHit2D hit)
    {
        Quaternion rotate = Quaternion.Euler(0, 0, -90 * transform.right.x);
        Vector2 slope = rotate * hit.normal;
        return slope;
    }

    // 낙하 중지
    void StopFalling()
    {
        InTheAir = false;
        movementFactor = groundMovementFactor;
        velocity.y = 0;
        NotifyObservers(SlugEvents.HitGround);
    }

    // 낙하 시작
    void StartFalling()
    {
        InTheAir = true;
        movementFactor = airLowVelocityMovementFactor;
        NotifyObservers(SlugEvents.Fall);
    }

    // 속도 계산
    void CalculateVelocity()
    {
        if (InTheAir)
        {
            velocity.x *= airDrag;
            myPrint("pre calculate " + velocity.y);
            velocity.y -= (verticalDrag * Time.deltaTime);

            myPrint("calculate " + velocity.y.ToString() + " " + Time.deltaTime + " " + verticalDrag);
            //Mathf.Clamp(absoluteVelocity.y, maxVerticalVelocity, initialJumpVelocity / 3);
        }
        else
        {
            velocity.x = velocity.x * groundDrag + forceX;
        }
    }

    // 속도 설정
    public void SetVelocity(float velX, float velY)
    {
        velocity.x = velX * transform.right.x;
        velocity.y = velY * transform.up.y;
    }

    // X 축 속도 설정
    public void SetVelocityX(float velX)
    {
        velocity.x = velX;
    }

    // Y 축 속도 설정
    public void SetVelocityY(float velY)
    {
        // WaitForPhysUpdate(() => {
        velocity.y = velY;
        //FIXME
        InTheAir = true;
        // });
    }

    // X 축 속도 가져오기
    public float GetVelocityX()
    {
        return velocity.x;
    }

    // 속도 벡터 가져오기
    public Vector2 GetVelocity()
    {
        return velocity;
    }

    // X 축 힘 설정
    public void SetForceX(float forceX)
    {
        this.forceX = forceX;
    }


    // 이동 계수 설정
    public void SetMovementFactor(float movementFactor)
    {
        this.movementFactor = movementFactor;
    }

    // 옵저버들에게 알림
    void NotifyObservers(SlugEvents ev)
    {
        if (observers == null) return;

        foreach (IObserver obs in observers)
        {
            obs.Observe(ev);
        }
    }

    // 물리 업데이트 대기
    private IEnumerator WaitForPhysUpdate(RetVoidTakeVoid cb)
    {
        yield return new WaitForFixedUpdate();
        cb();
    }
}