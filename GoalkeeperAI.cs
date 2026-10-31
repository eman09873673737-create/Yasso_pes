
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GoalkeeperAI : MonoBehaviour
{
    public Transform goalLineCenter;
    public float movementSpeed = 7f;
    public float horizontalRange = 3f;

    Rigidbody rb;
    Rigidbody ball;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        FindBall();
    }

    void FindBall()
    {
        if (ball != null) return;

        GameObject obj = GameObject.FindGameObjectWithTag("Ball");

        if (obj != null)
            ball = obj.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        FindBall();

        if (ball == null || goalLineCenter == null) return;

        Vector3 target = new Vector3(
            Mathf.Clamp(
                ball.position.x,
                goalLineCenter.position.x - horizontalRange,
                goalLineCenter.position.x + horizontalRange
            ),
            rb.position.y,
            goalLineCenter.position.z
        );

        Vector3 next = Vector3.MoveTowards(
            rb.position, target,
            movementSpeed * Time.fixedDeltaTime);

        rb.MovePosition(next);

        Vector3 direction = ball.position - rb.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.1f)
            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                Quaternion.LookRotation(direction),
                5f * Time.fixedDeltaTime));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Ball")) return;

        Rigidbody ballBody = collision.rigidbody;

        if (ballBody == null) return;

        Vector3 clearDirection =
            (ballBody.position - rb.position).normalized;

        clearDirection.y = 0.15f;

        ballBody.AddForce(
            clearDirection * 10f,
            ForceMode.Impulse
        );

        Debug.Log("الحارس تصدى للكرة!");
    }
}
