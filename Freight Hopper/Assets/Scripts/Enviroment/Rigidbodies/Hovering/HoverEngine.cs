using UnityEngine;

[System.Serializable]
public class HoverEngine : MonoBehaviour
{
    [SerializeField, ReadOnly] private Rigidbody rb;
    [SerializeField, ReadOnly] private Vector3 direction = Vector3.down;
    [SerializeField, ReadOnly] private LayerMask layerMask;
    [SerializeField, ReadOnly] private PID controller = new PID();
    [SerializeField, ReadOnly] private float targetDistance;
    [SerializeField, ReadOnly] private bool automatic;
    [SerializeField, ReadOnly] private bool firing;

    public bool Firing => firing;

    public void Initialize(Rigidbody rb, LayerMask layerMask, PID.Data data, float targetDistance, bool automatic)
    {
        this.rb = rb;
        controller.Initialize(data * rb.mass);
        this.layerMask = layerMask;
        this.targetDistance = targetDistance;
        this.automatic = automatic;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(this.transform.position, this.transform.position + (direction * targetDistance));
    }

    public void Hover(float height)
    {
        if (Physics.Raycast(this.transform.position, direction, out RaycastHit hit, height + 0.1f, layerMask))
        {
            firing = true;
            float error = height - hit.distance;
            Debug.DrawLine(this.transform.position, this.transform.position + (-direction * error), Color.white);
            // We don't want the hover engine to correct itself downwards. Hovering only applys upwards!
            if (error > -0.1f)
            {
                Vector3 force = -direction * this.controller.GetOutput(error, Time.fixedDeltaTime);

                rb.AddForceAtPosition(force, this.transform.position, ForceMode.Force);
            }
        }
        else
        {
            firing = false;
        }
    }

    public void SetDirection(Vector3 direction)
    {
        direction.Normalize();
        this.direction = direction;
    }

    private void FixedUpdate()
    {
        if (automatic)
        {
            SetDirection(-CustomGravity.GetUpAxis(this.transform.position));
            Hover(targetDistance);
        }
    }
}