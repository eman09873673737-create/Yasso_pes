using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerAI : MonoBehaviour
{
    public Vector3 homePosition;
    public float aiSpeed = 5f;
    public float chaseDistance = 12f;
    public float formationInfluence = 0.6f;

    Rigidbody rb;
    Rigidbody ball;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        homePosition = transform.position;
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
        if (ball == null) return;

        Vector3 target = homePosition;

        float distance = Vector3.Distance(
            transform.position, ball.position);

        if (distance < chaseDistance)
        {
            target = Vector3.Lerp(
                homePosition,
                ball.position,
                1f - formationInfluence);
        }

        target.y = rb.position.y;

        Vector3 next = Vector3.MoveTowards(
            rb.position, target,
            aiSpeed * Time.fixedDeltaTime);

        rb.MovePosition(next);

        Vector3 direction = target - rb.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.1f)
        {
            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                Quaternion.LookRotation(direction),
                5f * Time.fixedDeltaTime));
        }
    }
}
