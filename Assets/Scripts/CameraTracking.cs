// Usage: this script is meant to be placed on a Camera.
// The Camera must be a child of a Player.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; // Transform used to rotate around y-axis

    private bool isMovingCamera = false;
	public AnimationCurve motionCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    // Just for testing
    public Transform lookAt;

    private void Awake()
    {
        if (playerTransform == null)
            Debug.LogError(GetType() + ": The player transform is not initialized.");
    }

    private void Update()
    {
        // Example usage of LookAtAndTrack
        if (InputManager.GetKeyDown("Attack1"))
            StartCoroutine(LookAtAndTrack(lookAt, 1f, 10f));

        // Example usage of Track
        if (InputManager.GetKeyDown("Attack2"))
            StartCoroutine(Track(lookAt, 5f));
    }

    public IEnumerator LookAt(Transform tf, float time)
    {
        if (!this.isMovingCamera)
        {
            this.isMovingCamera = true;
            yield return StartCoroutine(LookAtCoro(tf, time));
            this.isMovingCamera = false;
        }
    }

    public IEnumerator Track(Transform tf, float time)
    {
        if (!this.isMovingCamera)
        {
            this.isMovingCamera = true;
            yield return StartCoroutine(TrackCoro(tf, time));
            this.isMovingCamera = false;
        }
    }

    public IEnumerator LookAtAndTrack(Transform tf, float timeToLookAt, float timeToTrack)
    {
        if (!this.isMovingCamera)
        {
            this.isMovingCamera = true;
            yield return StartCoroutine(LookAtAndTrackCoro(tf, timeToLookAt, timeToTrack));
            this.isMovingCamera = false;
        }
    }

    // Coroutine for smoothly interpolating toward facing a given transform's position.
    // Note: Transforms are pass-by-reference, so this allows for dynamic positions.
    private IEnumerator LookAtCoro(Transform tf, float time)
    {
        float speed = 1 / time;

        float interpVal = 0f;
        float startTime = Time.time;
        float startXRot = Normalize(this.transform.localRotation.eulerAngles.x);
        float startYRot = Normalize(this.playerTransform.localRotation.eulerAngles.y);
        
        while (interpVal < 1f)
        {
            yield return null;

            Vector3 relativePos = tf.position - this.transform.position;
            Quaternion quat = Quaternion.LookRotation(relativePos);
            float endXRot = Normalize(quat.eulerAngles.x);
            float endYRot = Normalize(quat.eulerAngles.y);
            
            // Forces the interpolation to take the shortest path.
            FixRotation(out endXRot, out startXRot, endXRot, startXRot);
            FixRotation(out endYRot, out startYRot, endYRot, startYRot);

            interpVal = motionCurve.Evaluate((Time.time - startTime) * speed);
            float xRot = Mathf.Lerp(startXRot, endXRot, interpVal);
            float yRot = Mathf.Lerp(startYRot, endYRot, interpVal);

            // Set player rotation around the y-axis, camera rotation around the x-axis
            this.playerTransform.localRotation = Quaternion.Euler(0f, yRot, 0f);
            this.transform.localRotation = Quaternion.Euler(xRot, 0f, this.transform.localRotation.eulerAngles.z);
        }
    }

    // Coroutine for tracking a given transform's position for a certain amount of time.
    private IEnumerator TrackCoro(Transform tf, float time)
    {
        float startTime = Time.time;

        while (Time.time - startTime < time)
        {
            yield return null;
            Vector3 relativePos = tf.position - this.transform.position;
            Quaternion quat = Quaternion.LookRotation(relativePos);
            float xRot = Normalize(quat.eulerAngles.x);
            float yRot = Normalize(quat.eulerAngles.y);

            // Set player rotation around the y-axis, camera rotation around the x-axis
            this.playerTransform.localRotation = Quaternion.Euler(0f, yRot, 0f);
            this.transform.localRotation = Quaternion.Euler(xRot, 0f, this.transform.localRotation.eulerAngles.z);
        }
    }

    // Coroutine for tracking a given transform's position indefinitely.
    private IEnumerator TrackCoro(Transform tf)
    {
        while (true)
        {
            yield return null;
            Vector3 relativePos = tf.position - this.transform.position;
            Quaternion quat = Quaternion.LookRotation(relativePos);
            float xRot = Normalize(quat.eulerAngles.x);
            float yRot = Normalize(quat.eulerAngles.y);

            // Set player rotation around the y-axis, camera rotation around the x-axis
            this.playerTransform.localRotation = Quaternion.Euler(0f, yRot, 0f);
            this.transform.localRotation = Quaternion.Euler(xRot, 0f, this.transform.localRotation.eulerAngles.z);
        }
    }

    private IEnumerator LookAtAndTrackCoro(Transform tf, float timeToLookAt, float timeToTrack)
    {
        yield return StartCoroutine(LookAtCoro(tf, timeToLookAt));
        yield return StartCoroutine(TrackCoro(tf, timeToTrack));
    }
    
    private float Normalize(float f)
    {
        if (f >= 180f)
            return f - 360f;
        else if (f < -180f)
            return f + 360f;
        else
            return f;
    }
    
    // Checks if the start and end values differ in sign.
    // If it would be a shorter path, change the negative value to its positive equivalent.
    private void FixRotation(out float newEndRot, out float newStartRot, float endRot, float startRot)
    {
        newEndRot = endRot;
        newStartRot = startRot;

        if (endRot < 0f && startRot > 0f)
        {
            if (Mathf.Abs(endRot - startRot) > Mathf.Abs(startRot - (endRot + 360f)))
                newEndRot = endRot + 360f;
        }
        else if (startRot < 0f && endRot > 0f)
        {
            if (Mathf.Abs(endRot - startRot) > Mathf.Abs((startRot + 360f) - endRot))
                newStartRot = startRot + 360f;
        }
    }
}
