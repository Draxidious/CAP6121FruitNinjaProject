using UnityEngine;

public class CustomVignette : MonoBehaviour
{
	[Tooltip("The Mesh Renderer that displays the vignette.")]
	public MeshRenderer vignetteRenderer;

	[Tooltip("The material used for the vignette effect.  Use the VR/TunnelingVignette shader.")]
	public Material vignetteMaterial;

	[Tooltip("The script that provides the speed value.")]
	public UnicycleMovement speedProvider; // Change MonoBehaviour to the actual type

    [Tooltip("The name of the property or field on the speed provider that holds the speed (float).")]
	public string speedPropertyName = "CurrentSpeed";

	[Tooltip("The speed at which the vignette starts to appear.")]
	public float minSpeedThreshold = 1f;

	[Tooltip("The speed at which the vignette is fully closed.")]
	public float maxSpeedThreshold = 5f;

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
	private System.Reflection.PropertyInfo _speedProperty;
	private System.Reflection.FieldInfo _speedField;
	private MaterialPropertyBlock _propertyBlock;

	private static readonly int ApertureSize = Shader.PropertyToID("_ApertureSize");
	private static readonly int FeatheringEffect = Shader.PropertyToID("_FeatheringEffect");
	private static readonly int VignetteColor = Shader.PropertyToID("_VignetteColor");
	private static readonly int VignetteColorBlend = Shader.PropertyToID("_VignetteColorBlend");

	void Start()
	{
		maxSpeedThreshold = speedProvider.lean.maxSpeed;

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

		if (speedProvider == null)
		{
			Debug.LogError("Speed Provider is not assigned.", this);
			enabled = false;
			return;
		}

		// --- Reflection Setup ---
		// Try to get as property.
		_speedProperty = speedProvider.GetType().GetProperty(speedPropertyName);

		// Try to get as field, if property not found.
		if (_speedProperty == null)
		{
			_speedField = speedProvider.GetType().GetField(speedPropertyName);

			if (_speedField == null)
			{
				Debug.LogError($"Speed property/field '{speedPropertyName}' not found on {speedProvider.GetType().Name}.", this);
				enabled = false;
				return;
			}
		}


		// --- Material Setup ---
		_propertyBlock = new MaterialPropertyBlock();
		vignetteRenderer.material = vignetteMaterial; // Use the assigned material.

		// Initialize the vignette.
		UpdateVignette();
	}

	void Update()
	{
		// --- Get Speed ---
		float currentSpeed;

		if (_speedProperty != null)
		{
			currentSpeed = (float)_speedProperty.GetValue(speedProvider);
		}
		else // It's a field.
		{
			currentSpeed = (float)_speedField.GetValue(speedProvider);
		}


		// --- Calculate Target Aperture Size ---
		float speedRatio = Mathf.Clamp01((currentSpeed - minSpeedThreshold) / (maxSpeedThreshold - minSpeedThreshold));
		_targetApertureSize = Mathf.Lerp(1f, 0.5f, speedRatio);  // 1 = no vignette, 0 = fully closed based on speed


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