using UnityEngine;

[RequireComponent(typeof(Animator), typeof(MovementController))]
public class PlayerAnimationController : MonoBehaviour
{
    private Animator _animator;
    private MovementController _movement;

    // For velocity-based animation when Rigidbody2D is kinematic
    private Vector2 _lastPosition;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<MovementController>();
        _lastPosition = transform.position;
    }

    void Update()
    {
        // Compute local velocity (world delta) over the frame
        Vector2 currentPosition = transform.position;
        Vector2 velocity = (currentPosition - _lastPosition) / Time.deltaTime;
        _lastPosition = currentPosition;

        float speed = velocity.magnitude;

        // Tell the Blend Tree how fast and in which direction to play
        _animator.SetFloat("Speed", speed);

        if (speed > 0.1f)
        {
            // Normalize direction for Blend Tree parameters
            Vector2 dir = velocity.normalized;
            _animator.SetFloat("MoveX", dir.x);
            _animator.SetFloat("MoveY", dir.y);
        }
    }
}