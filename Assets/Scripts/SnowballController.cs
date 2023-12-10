using System.Collections;
using UnityEngine;

public class SnowballController : MonoBehaviour
{
    public GameObject snowballPrefab;
    public float snowballGrowthRate = 0.1f;
    public LayerMask groundLayer;
    public LayerMask snowballTagLayer;
    public string snowballTag = "Snowball";
    public float maxDistanceToStack = 2f;

    private Camera mainCamera;
    private GameObject currentSnowball;
    private bool isGrowingSnowball = false;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleInput();

        // Check if the snowball is being grown
        if (isGrowingSnowball)
        {
            GrowSnowball();
        }
    }

    void HandleInput()
    {
        // Check if left mouse button is pressed down
        if (Input.GetMouseButtonDown(0))
        {
            // Try to roll an existing snowball
            if (TryRollExistingSnowball())
            {
                return;
            }

            // If no existing snowball is found, instantiate a new one
            Vector3 instantiatePosition = GetMouseRaycastHitPosition();
            GameObject newSnowball = Instantiate(snowballPrefab, instantiatePosition, Quaternion.identity);
            currentSnowball = newSnowball;
            isGrowingSnowball = true;
        }

        // Check if left mouse button is released
        if (Input.GetMouseButtonUp(0))
        {
            isGrowingSnowball = false;
            currentSnowball = null;
        }
    }

    bool TryRollExistingSnowball()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if the ray hits an existing snowball
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, snowballTagLayer) && hit.collider.CompareTag(snowballTag))
        {
            // If a snowball with the specified tag is hit, grow that snowball
            currentSnowball = hit.collider.gameObject;
            isGrowingSnowball = true;
            return true;
        }

        return false;
    }

    void GrowSnowball()
    {
        if (currentSnowball != null)
        {
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                Vector3 newPosition = hit.point;

                // Adjust the snowball's position based on the ground height
                Bounds bounds = currentSnowball.GetComponent<Collider>().bounds;
                newPosition.y = hit.point.y + bounds.extents.y;

                currentSnowball.transform.position = newPosition;

                float snowballScaleDelta = Mathf.Abs(Input.GetAxis("Mouse X")) + Mathf.Abs(Input.GetAxis("Mouse Y"));
                snowballScaleDelta *= snowballGrowthRate;
                Vector3 newScale = currentSnowball.transform.localScale + new Vector3(snowballScaleDelta, snowballScaleDelta, snowballScaleDelta);
                currentSnowball.transform.localScale = newScale;
            }

            // Check for collision with other snowballs
            Collider[] colliders = Physics.OverlapSphere(currentSnowball.transform.position, currentSnowball.transform.localScale.x, snowballTagLayer);

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject != currentSnowball)
                {
                    // Check size comparison to determine the stacking behavior
                    float currentSnowballSize = currentSnowball.transform.localScale.x;
                    float otherSnowballSize = collider.transform.localScale.x;

                    if (currentSnowballSize > otherSnowballSize)
                    {
                        // Absorb the smaller snowball and grow the current one
                        Destroy(collider.gameObject);
                        float absorbedScale = Mathf.Sqrt(currentSnowballSize * currentSnowballSize + otherSnowballSize * otherSnowballSize);
                        currentSnowball.transform.localScale = new Vector3(absorbedScale, absorbedScale, absorbedScale);
                    }
                    else
                    {
                        // Stack the smaller snowball on top of the larger one
                        float newYPosition = collider.transform.position.y + collider.bounds.extents.y + currentSnowball.transform.localScale.y;
                        currentSnowball.transform.position = new Vector3(currentSnowball.transform.position.x, newYPosition, currentSnowball.transform.position.z);
                    }
                }
            }
        }
    }

    Vector3 GetMouseRaycastHitPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            return hit.point;
        }

        return Vector3.zero;
    }
}