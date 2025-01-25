using UnityEngine;

public class JumpingPad : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float horizontalForce = 0f;
    [SerializeField] private Vector3 jumpDirection = Vector3.up;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool useVisualFeedback = true;
    [SerializeField] private float feedbackDuration = 0.2f;
    [SerializeField] private Vector3 squashScale = new Vector3(1.2f, 0.8f, 1.2f);
    [SerializeField] private Vector3 stretchScale = new Vector3(0.8f, 1.2f, 0.8f);
    
    private Vector3 originalScale;
    private bool isAnimating = false;
    private float animationTime = 0f;

    private void Start()
    {
        originalScale = transform.localScale;
        jumpDirection = jumpDirection.normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerMovement>(out PlayerMovement player))
        {
            // Calculate the jump velocity using the same formula as the player's jump
            float verticalVelocity = Mathf.Sqrt(jumpForce * -2f * player.Gravity());
            
            // Create the final velocity by combining vertical and horizontal forces
            Vector3 launchVelocity = jumpDirection * verticalVelocity;
            if (horizontalForce > 0)
            {
                // Add horizontal force in the direction the pad is facing
                launchVelocity += transform.forward * horizontalForce;
            }
            
            // Apply the velocity directly to the player's movement script
            player.SetVelocity(launchVelocity);

            if (useVisualFeedback)
            {
                PlayJumpPadAnimation();
            }
        }
    }

    private void PlayJumpPadAnimation()
    {
        isAnimating = true;
        animationTime = 0f;
    }

    private void Update()
    {
        if (isAnimating)
        {
            animationTime += Time.deltaTime;
            float progress = animationTime / feedbackDuration;

            if (progress <= 0.5f)
            {
                // First half of animation - squash
                transform.localScale = Vector3.Lerp(originalScale, squashScale, progress * 2f);
            }
            else
            {
                // Second half of animation - stretch and return to normal
                float stretchProgress = (progress - 0.5f) * 2f;
                transform.localScale = Vector3.Lerp(stretchScale, originalScale, stretchProgress);
            }

            if (progress >= 1f)
            {
                isAnimating = false;
                transform.localScale = originalScale;
            }
        }
    }
}