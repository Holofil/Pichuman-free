using UnityEngine;

public class ViewMotion : MonoBehaviour
{
    [SerializeField] float Smoothness = 0.1f;
    [SerializeField] Vector3 View1Position = new Vector3(0, 7.291f, -13f);
    [SerializeField] Vector3 View2Position = new Vector3(0, 7.291f, 13f);
    [SerializeField] int RoundedValue = 2;

    public Movement playerMovement; // Reference to player movement script

    private Vector3 ReachPos;
    private float xPos, yPos, zPos;
    private bool isTransitioning = false;

    private void Start()
    {
        // Initial position set to View1
        transform.position = View1Position;
        ReachPos = View1Position;
        xPos = transform.position.x;
        yPos = transform.position.y;
        zPos = transform.position.z;
    }

    private void Update()
    {
        // Determine target position based on player's Z position
        if (playerMovement != null)
        {
            if (playerMovement.transform.position.z > 2.09f && !isTransitioning)
            {
                StartTransitionToView(View2Position);
            }
            else if (playerMovement.transform.position.z <= 2.09f && !isTransitioning)
            {
                StartTransitionToView(View1Position);
            }
        }

        // Smooth movement logic
        MoveTowardsReachPos();

        // Update transform position
        transform.position = new Vector3(RoundToX(xPos), RoundToX(yPos), RoundToX(zPos));
    }

    private void StartTransitionToView(Vector3 targetView)
    {
        ReachPos = targetView;
        isTransitioning = true;
    }

    private void MoveTowardsReachPos()
    {
        // X Position
        if (xPos < ReachPos.x)
        {
            xPos += Smoothness;
        }
        else if (xPos > ReachPos.x)
        {
            xPos -= Smoothness;
        }
        xPos = RoundToX(xPos);

        // Y Position
        if (yPos < ReachPos.y)
        {
            yPos += Smoothness;
        }
        else if (yPos > ReachPos.y)
        {
            yPos -= Smoothness;
        }
        yPos = RoundToX(yPos);

        // Z Position
        if (zPos < ReachPos.z)
        {
            zPos += Smoothness;
        }
        else if (zPos > ReachPos.z)
        {
            zPos -= Smoothness;
        }
        zPos = RoundToX(zPos);

        // Check if transition is complete
        if (Mathf.Approximately(xPos, ReachPos.x) && 
            Mathf.Approximately(yPos, ReachPos.y) && 
            Mathf.Approximately(zPos, ReachPos.z))
        {
            isTransitioning = false;
        }
    }

    float RoundToX(float val)
    {
        return (float)System.Math.Round((double)val, RoundedValue);
    }
}