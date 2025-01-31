using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollisionManagement : MonoBehaviour
{
    private Rigidbody rb;
    private Friction frictionManager;
    private RigidbodyLinker rigidbodyLinker;
    private Gravity gravity;

    [SerializeField] private float maxSlope = 60;
    [SerializeField] private float groundRaycastCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private float maxDepenetrationVelocity = 500;
    [ReadOnly, SerializeField] private Memory<bool> isGrounded;
    [ReadOnly, SerializeField] private Memory<bool> trueIsGrounded;
    [ReadOnly, SerializeField] private Memory<Vector3> contactNormal;
    [ReadOnly, SerializeField] private Memory<Vector3> velocity;
    [ReadOnly, SerializeField] private Memory<Vector3> position;
    [ReadOnly, SerializeField] private int contactCount;
    [ReadOnly, SerializeField] private int steepCount;

    [ReadOnly, SerializeField] private Vector3 validUpAxis = Vector3.up;

    public Vector3 ValidUpAxis => validUpAxis;
    public Memory<Vector3> ContactNormal => contactNormal;
    public Memory<bool> IsGrounded => isGrounded;
    public Memory<bool> TrueIsGrounded => trueIsGrounded;
    public bool LevelSurface => this.ValidUpAxis == this.ContactNormal.current && isGrounded.current;
    public Memory<Vector3> Velocity => velocity;
    public Memory<Vector3> Position => position;
    public float MaxSlope => maxSlope;

    public float MaxDepenetrationVelocity => maxDepenetrationVelocity;

    public delegate void CollisionEventHandler();

    public event CollisionEventHandler Landed;

    public event CollisionEventHandler CollisionDataCollected;

    private Vector3 upAxis;

    private void Awake()
    {
        rb = Player.Instance.modules.rigidbody;
        frictionManager = Player.Instance.modules.friction;
        rigidbodyLinker = Player.Instance.modules.rigidbodyLinker;
        gravity = Player.Instance.modules.gravity;

        upAxis = CustomGravity.GetUpAxis();
        contactNormal.current = upAxis;
        contactNormal.UpdateOld();
        Physics.defaultMaxDepenetrationVelocity = this.MaxDepenetrationVelocity;
        Player.PlayerCanMove += EnableCollision;
    }

    void EnableCollision(){
        this.GetComponent<Collider>().enabled = true;
        this.StartCoroutine(LateFixedUpdate());
    }
    private void OnDestroy()
    {
        Player.PlayerCanMove -= EnableCollision;
    }

    List<ContactPoint> contacts = new List<ContactPoint>();
    private void EvaulateCollisions(Collision collision)
    {
        contactNormal.current = Vector3.zero;
        collision.GetContacts(contacts);
        foreach (ContactPoint contactPoint in contacts)
        {
            Vector3 normal = contactPoint.normal;

            float collisionAngle = Vector3.Angle(normal, upAxis);
            if (normal != default && collisionAngle <= maxSlope)
            {
                isGrounded.current = true;
                trueIsGrounded.current = true;
                contactNormal.current += normal;
                contactCount++;
                frictionManager.EvalauteSurface(collision);
                rigidbodyLinker.UpdateLink(collision.rigidbody);
            }
            else
            {
                steepCount++;
                if (contactCount == 0)
                {
                    rigidbodyLinker.UpdateLink(collision.rigidbody);
                }
            }
        }
        if (contactCount == 0)
        {
            contactNormal.current = upAxis;
        }
        if (contactCount > 1)
        {
            contactNormal.current.Normalize();
        }
    }

    // Call using OnCollisionEnter from a monobehavior
    public void OnCollisionEnter(Collision collision)
    {
        EvaulateCollisions(collision);
        if (isGrounded.current)
        {
            Landed?.Invoke();
        }
        Player.Instance.modules.edgeCorrectionCollision.AddContacts(collision);
    }

    // Call using OnCollisionStay from a monobehavior
    public void OnCollisionStay(Collision collision)
    {
        EvaulateCollisions(collision);
        if (isGrounded.current && !isGrounded.old)
        {
            Landed?.Invoke();
        }
        Player.Instance.modules.edgeCorrectionCollision.AddContacts(collision);
    }

    private void CheckGround()
    {
        if (isGrounded.current)
        {
            return;
        }

        var transform1 = rb.transform;
        var up = transform1.up;
        Ray ray = new Ray(rb.position - (up * transform1.localScale.y), -up);
        Debug.DrawRay(ray.origin, ray.direction * groundRaycastCheckDistance, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hit, groundRaycastCheckDistance, groundMask))
        {
            if (hit.collider == null || hit.collider.isTrigger || hit.normal == default)
                return;

            if (Vector3.Angle(hit.normal, this.ValidUpAxis) <= maxSlope)
            {
                isGrounded.current = true;
                contactNormal.current = hit.normal;
                rb.AddForce(-ValidUpAxis, ForceMode.Acceleration);
            }
        }
    }

    private IEnumerator LateFixedUpdate()
    {
        var waitFixedUpdate = new WaitForFixedUpdate();
        while (true)
        {
            upAxis = CustomGravity.GetUpAxis();

            yield return waitFixedUpdate;
            if (!isGrounded.current)
            {
                gravity.GravityLoop();
            }
            CheckGround();
            rigidbodyLinker.UpdateConnectionState(rb);
            CollisionDataCollected?.Invoke();

            UpdateOldValues();
            ClearValues();
        }
    }

    private void UpdateOldValues()
    {
        isGrounded.UpdateOld();
        trueIsGrounded.UpdateOld();
        contactNormal.UpdateOld();
        rigidbodyLinker.UpdateOldValues();
        velocity.UpdateOld();
        position.UpdateOld();
    }

    private void ClearValues()
    {
        isGrounded.current = false;
        trueIsGrounded.current = false;
        contactCount = 0;
        steepCount = 0;
        velocity.current = rb.velocity;
        position.current = rb.position;

        if (upAxis.magnitude > float.Epsilon)
        {
            validUpAxis = upAxis;
        }
        contactNormal.current = validUpAxis;
        rigidbodyLinker.ClearValues();
        frictionManager.ClearValues();
    }
}