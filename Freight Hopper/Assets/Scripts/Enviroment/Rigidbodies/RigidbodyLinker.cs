using UnityEngine;

[System.Serializable]
public class RigidbodyLinker
{
    [ReadOnly, SerializeField] private Memory<Rigidbody> connectedRb = new Memory<Rigidbody>(null, null);
    [ReadOnly, SerializeField] private Vector3 connectionWorldPosition;
    [ReadOnly, SerializeField] private Vector3 connectionLocalPosition;
    [ReadOnly, SerializeField] private Memory<Vector3> connectionVelocity;
    [ReadOnly, SerializeField] private Memory<Vector3> connectionAcceleration;

    public Memory<Rigidbody> ConnectedRb => connectedRb;
    public Memory<Vector3> ConnectionAcceleration => connectionAcceleration;
    public Memory<Vector3> ConnectionVelocity => connectionVelocity;

    /// <summary>
    /// Assigns connected rigidbody to parameter
    /// </summary>
    public void UpdateLink(Rigidbody rigidbody)
    {
        connectedRb.current = rigidbody;
    }

    /// <summary>
    /// Applies acceleration to ourRigidbody if applicable. Not applicable if mass of linked rigidbody
    /// is too small or its not kinematic
    /// </summary>
    public void UpdateConnectionState(Rigidbody ourRigidbody)
    {
        if (!connectedRb.current)
        {
            return;
        }

        if (!connectedRb.current.isKinematic && connectedRb.current.mass < ourRigidbody.mass)
        {
            return;
        }

        if (connectedRb.current == connectedRb.old)
        {
            connectionVelocity.current = (connectedRb.current.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition) / Time.fixedDeltaTime;
            connectionAcceleration.current = (connectionVelocity.current - connectionVelocity.old);
            ourRigidbody.AddForce(connectionAcceleration.current, ForceMode.VelocityChange);
        }

        connectionWorldPosition = ourRigidbody.position;
        connectionLocalPosition = connectedRb.current.transform.InverseTransformPoint(connectionWorldPosition);
    }

    public void UpdateOldValues()
    {
        connectionAcceleration.UpdateOld();
        connectionVelocity.UpdateOld();
        connectedRb.UpdateOld();
    }

    public void ClearValues()
    {
        connectionAcceleration.current = Vector3.zero;
        connectionVelocity.current = Vector3.zero;

        connectedRb.current = null;
    }
}