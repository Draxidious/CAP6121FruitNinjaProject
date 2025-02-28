using UnityEngine;

public class ChildGlow : MonoBehaviour
{
	public Material glowMaterial;
	[Range(0f, 1f)] public float glowAmount = 0.5f;
	public float maxDistance = 10f;
	public Transform player;

	private GameObject[] glowObjects;

	void Update()
	{
		if (player == null) return;

		float distance = Vector3.Distance(transform.position, player.position);
		bool withinRange = distance <= maxDistance;

		foreach (GameObject glowObject in glowObjects)
		{
			if (glowObject != null)
			{
				glowObject.SetActive(withinRange);
			}
		}

		if (withinRange)
		{
			UpdateGlowEffect();
		}
	}

	public void CreateGlowObjects()
	{
		int childCount = transform.childCount;
		glowObjects = new GameObject[childCount];

		for (int i = 0; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			glowObjects[i] = CreateGlowObject(child);
		}
	}

	public GameObject CreateGlowObject(Transform child)
	{
		GameObject glowObject = new GameObject(child.name + "_Glow");
		glowObject.transform.SetParent(child);
		glowObject.transform.localPosition = Vector3.zero;
		glowObject.transform.localRotation = Quaternion.identity;

		MeshFilter meshFilter = child.GetComponentInChildren<MeshFilter>();

		if (meshFilter != null)
		{
			MeshRenderer glowMeshRenderer = glowObject.AddComponent<MeshRenderer>();
			glowMeshRenderer.material = glowMaterial;

			MeshFilter glowMeshFilter = glowObject.AddComponent<MeshFilter>();
			glowMeshFilter.mesh = meshFilter.mesh;

			return glowObject;
		}
		else
		{
			Destroy(glowObject);
			return null;
		}
	}

	void UpdateGlowEffect()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			GameObject glowObject = glowObjects[i];

			if (glowObject != null)
			{
				Vector3 directionToPlayer = (player.position - child.position).normalized;
				Vector3 pushDirection = -directionToPlayer;

				float scaleMultiplier = 1f + glowAmount * 0.1f; // Constant scale based on glowAmount
				float pushMultiplier = glowAmount * 0.1f; // Constant push based on glowAmount

				glowObject.transform.localScale = Vector3.one * scaleMultiplier;
				glowObject.transform.position = child.position + pushDirection * pushMultiplier;
			}
		}
	}
}