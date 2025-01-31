using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class JumpingPad : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float horizontalForce = 0f;
    [SerializeField] private Vector3 jumpDirection = Vector3.up;
    
    [Header("Animation")]
    [SerializeField] private bool useVisualFeedback = true;
    [SerializeField] private float feedbackDuration = 0.2f;
    [SerializeField] private Vector3 squashScale = new Vector3(1.2f, 0.8f, 1.2f);
    [SerializeField] private Vector3 stretchScale = new Vector3(0.8f, 1.2f, 0.8f);
    
    [Header("References")]
    [SerializeField] private SOAudioEvent jumpPadSfx;
    
    private AudioSource _audioSource;
    private Vector3 _originalScale;
    private bool _isAnimating = false;
    private float _animationTime = 0f;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _originalScale = transform.localScale;
        jumpDirection = jumpDirection.normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerMovement player))
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


            jumpPadSfx.Play(_audioSource);
            if (useVisualFeedback) PlayJumpPadAnimation();
                

        }
    }

    private void PlayJumpPadAnimation()
    {
        _isAnimating = true;
        _animationTime = 0f;
    }

    private void Update()
    {
        if (_isAnimating)
        {
            _animationTime += Time.deltaTime;
            float progress = _animationTime / feedbackDuration;

            if (progress <= 0.5f)
            {
                // First half of animation - squash
                transform.localScale = Vector3.Lerp(_originalScale, squashScale, progress * 2f);
            }
            else
            {
                // Second half of animation - stretch and return to normal
                float stretchProgress = (progress - 0.5f) * 2f;
                transform.localScale = Vector3.Lerp(stretchScale, _originalScale, stretchProgress);
            }

            if (progress >= 1f)
            {
                _isAnimating = false;
                transform.localScale = _originalScale;
            }
        }
    }
}