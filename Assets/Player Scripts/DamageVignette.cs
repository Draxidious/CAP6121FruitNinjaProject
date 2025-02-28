using UnityEngine;

public class DamageVignette : MonoBehaviour
{
    [Tooltip("The Mesh Renderer that displays the vignette.")]
    public MeshRenderer vignetteRenderer;

    [Tooltip("The material used for the vignette effect.  Use the VR/TunnelingVignette shader.")]
    public Material vignetteMaterial;

    [Tooltip("How quickly the vignette transitions in (seconds).")]
    public float easeInTime = 0.2f;

    [Tooltip("How quickly the vignette transitions out (seconds).")]
    public float easeOutTime = 0.3f;

    [Tooltip("The color of the vignette.")]
    public Color vignetteColor = Color.black;

    [Tooltip("The optional color to add color blending to the visual cut-off area of the vignette.")]
    public Color vignetteColorBlend = Color.black;

    [Tooltip("The degree of smoothly blending the edges between the aperture and full visual cut-off.")]
    public float featheringEffect = 0.4f;

    private float _currentApertureSize = 1f; // 1 = no vignette, 0 = fully closed
    private float _targetApertureSize = 1f;
    private MaterialPropertyBlock _propertyBlock;

    private static readonly int ApertureSize = Shader.PropertyToID("_ApertureSize");
    private static readonly int FeatheringEffect = Shader.PropertyToID("_FeatheringEffect");
    private static readonly int VignetteColor = Shader.PropertyToID("_VignetteColor");
    private static readonly int VignetteColorBlend = Shader.PropertyToID("_VignetteColorBlend");

    void Start()
    {
        // --- Input Validation ---
        if (vignetteRenderer == null)
        {
            Debug.LogError("Vignette Renderer is not assigned.", this);
            enabled = false;
            return;
        }

        if (vignetteMaterial == null)
        {
            Debug.LogError("Vignette Material is not assigned.", this);
            enabled = false;
            return;
        }

        // --- Material Setup ---
        _propertyBlock = new MaterialPropertyBlock();
        vignetteRenderer.material = vignetteMaterial; // Use the assigned material.

        // Initialize the vignette.
        UpdateVignette();
    }

    void Update()
    {
        // --- Smooth Transition (Ease In/Out) ---
        float easeTime = (_targetApertureSize < _currentApertureSize) ? easeInTime : easeOutTime;
        if (easeTime > 0)
        {
            _currentApertureSize = Mathf.MoveTowards(_currentApertureSize, _targetApertureSize, Time.deltaTime / easeTime);
        }
        else
        {
            _currentApertureSize = _targetApertureSize;
        }

        // --- Apply Vignette ---
        UpdateVignette();
    }

    // Function to trigger the vignette effect for damage
    public void TriggerVignetteEffect(float duration = 0.5f)
    {
        // Open the vignette fully (close it)
        _targetApertureSize = 0f;
        Invoke("CloseVignette", duration);
    }

    // Function to close the vignette (restore to normal)
    private void CloseVignette()
    {
        // Close the vignette fully (open it)
        _targetApertureSize = 1f;
    }

    void UpdateVignette()
    {
        vignetteRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetFloat(ApertureSize, _currentApertureSize);
        _propertyBlock.SetFloat(FeatheringEffect, featheringEffect);
        _propertyBlock.SetColor(VignetteColor, vignetteColor);
        _propertyBlock.SetColor(VignetteColorBlend, vignetteColorBlend);
        vignetteRenderer.SetPropertyBlock(_propertyBlock);
    }
}
