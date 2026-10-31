using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float sprintMultiplier = 1.5f;
    public float passPower = 12f;
    public float shootPower = 22f;
    public float ballControlDistance = 2f;

    Rigidbody rb;
    Vector3 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints |= RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (GameManager.Instance == null ||
            !GameManager.Instance.matchRunning) return;

        movement = new Vector3(
            Input.GetAxisRaw("Horizontal"), 0,
            Input.GetAxisRaw("Vertical")
        );

        movement = Vector3.ClampMagnitude(movement, 1f);

        if (Input.GetKeyDown(KeyCode.Space))
            PassBall();

        if (Input.GetKeyDown(KeyCode.J))
            ShootBall();
    }

    void FixedUpdate()
    {
        float speed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
            speed *= sprintMultiplier;

        Vector3 nextPosition =
            rb.position + movement * speed * Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);

        if (movement.sqrMagnitude > 0.01f)
        {
            Quaternion rotation = Quaternion.LookRotation(movement);

            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation, rotation,
                12f * Time.fixedDeltaTime));
        }
    }

    bool CanPlayBall()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.ball == null) return false;

        return Vector3.Distance(
            transform.position,
            GameManager.Instance.ball.position
        ) <= ballControlDistance;
    }

    void PassBall()
    {
        if (!CanPlayBall()) return;

        GameManager gm = GameManager.Instance;
        GameObject receiver = gm.GetNearestTeammate(gameObject);

        if (receiver == null) return;

        Rigidbody ball = gm.ball;

        Vector3 direction =
            receiver.transform.position - ball.position;

        direction.y = 0.08f;
        direction.Normalize();

        ball.linearVelocity = direction * passPower;

        gm.SwitchControlTo(receiver);
    }

    void ShootBall()
    {
        if (!CanPlayBall()) return;

        Rigidbody ball = GameManager.Instance.ball;

        Vector3 direction = transform.forward;
        direction.y = 0.12f;
        direction.Normalize();

        ball.linearVelocity = direction * shootPower;
    }
}
