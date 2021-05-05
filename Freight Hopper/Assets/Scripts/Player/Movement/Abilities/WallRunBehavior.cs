using UnityEngine;

public class WallRunBehavior : AbilityBehavior
{
    /// Front, Right, Back, Left
    [SerializeField, ReadOnly] private Vector3[] wallNormals = new Vector3[4];

    [SerializeField] private float wallCheckDistance = 1;

    [SerializeField] private LayerMask validWalls;
    [SerializeField] private float upwardsForce = 10;
    [SerializeField] private float rightForce = 5; // force applied towards the wall
    [SerializeField] private float climbForce = 10;
    public Timer climbTimer = new Timer(0.5f);

    [Space]
    [SerializeField] private float jumpInitialForce = 10;

    [SerializeField] private float jumpIniitalPush = 10;
    [SerializeField] private float jumpContinousForce = 10;

    [SerializeField] private float jumpContinousPush = 10;
    public Timer jumpHoldingTimer = new Timer(0.5f);
    private Vector3 jumpNormalCache;

    private bool[] UpdateWallStatus(Vector3[] walls)
    {
        bool[] nearWall = { false, false, false, false };
        for (int i = 0; i < 4; i++)
        {
            if (!walls[i].IsZero())
            {
                nearWall[i] = true;
            }
        }
        wallNormals = walls;
        return nearWall;
    }

    /// <summary>
    /// Returns if "touching" one of the 4 cardinal walls.
    /// Also updates internal wall normal storage
    /// </summary>
    public bool[] CheckWalls()
    {
        return UpdateWallStatus(playerCM.CheckWalls(wallCheckDistance, validWalls));
    }

    public override void EntryAction()
    {
    }

    public override void Action()
    {
    }

    public void WallClimb()
    {
        climbTimer.CountDownFixed();
        Vector3 upAlongWall = Vector3.Cross(playerRb.transform.right, wallNormals[0]);
        Debug.DrawLine(playerRb.position, playerRb.position + upAlongWall, Color.green, Time.fixedDeltaTime);
        playerRb.AddForce(rightForce * -wallNormals[0], ForceMode.Acceleration);
        playerRb.AddForce(climbForce * upAlongWall, ForceMode.Acceleration);
    }

    public void WallJumpInitial()
    {
        Vector3 sumNormals = Vector3.zero;
        foreach (Vector3 normal in wallNormals)
        {
            sumNormals += normal;
        }
        sumNormals.Normalize();
        jumpNormalCache = sumNormals;
        playerRb.AddForce(jumpIniitalPush * sumNormals, ForceMode.VelocityChange);
        playerRb.AddForce(jumpInitialForce * playerCM.ValidUpAxis, ForceMode.VelocityChange);
    }

    public void WallJumpContinous()
    {
        jumpHoldingTimer.CountDownFixed();
        playerRb.AddForce(jumpIniitalPush * jumpNormalCache, ForceMode.Acceleration);
        playerRb.AddForce(jumpInitialForce * playerCM.ValidUpAxis, ForceMode.Acceleration);
    }

    public void RightWallRun()
    {
        //Debug.Log("Right wall Climb");
        Vector3 upAlongWall = -Vector3.Cross(playerRb.transform.forward, wallNormals[1]);
        Debug.DrawLine(playerRb.position, playerRb.position + upAlongWall, Color.green, Time.fixedDeltaTime);
        WallRun(-wallNormals[1], upAlongWall);
    }

    public void LeftWallRun()
    {
        //Debug.Log("Left wall Climb");
        Vector3 upAlongWall = Vector3.Cross(playerRb.transform.forward, wallNormals[3]);
        Debug.DrawLine(playerRb.position, playerRb.position + upAlongWall, Color.green, Time.fixedDeltaTime);
        WallRun(-wallNormals[3], upAlongWall);
    }

    private void WallRun(Vector3 right, Vector3 up)
    {
        playerRb.AddForce(right * rightForce, ForceMode.Acceleration);
        playerRb.AddForce(up * upwardsForce, ForceMode.Acceleration);
    }

    public void WallClimbExit()
    {
        base.ExitAction();
    }

    public override void ExitAction()
    {
    }
}