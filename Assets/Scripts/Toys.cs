using UnityEngine;
using System.Collections.Generic;

public class Toys : MonoBehaviour
{
	public List<GameObject> objectsToSpawn;
	public float minX = -10f; // Minimum X position for spawning.
	public float maxX = 10f;  // Maximum X position for spawning.
	public float minZ = -10f; // Minimum Z position for spawning.
	public float maxZ = 10f;  // Maximum Z position for spawning.
	public int numberOfSpawns = 10;
	public GameObject spawnedChildrenParent;
	public float fixedYPosition = 0.1f;
	public bool snapToGround = true;
	public bool makeKinematic = true;

	void Awake()
	{
		SpawnToys();
	}

	public void SpawnToys()
	{
		if (objectsToSpawn == null || objectsToSpawn.Count == 0 || spawnedChildrenParent == null)
		{
			Debug.LogError("Objects to spawn list or spawned children parent is not assigned.");
			return;
		}

		for (int i = 0; i < numberOfSpawns; i++)
		{
			GameObject objectToSpawn = objectsToSpawn[Random.Range(0, objectsToSpawn.Count)];

			// Generate random position within minX, maxX, minZ, maxZ.
			Vector3 spawnPosition = new Vector3(
				Random.Range(minX, maxX),
				fixedYPosition,
				Random.Range(minZ, maxZ)
			);

			if (snapToGround)
			{
				RaycastHit hit;
				if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, Mathf.Infinity))
				{
					spawnPosition.y = hit.point.y;
				}
				else
				{
					Debug.LogWarning("Could not find ground below spawn position.");
				}
			}

			GameObject spawnedChild = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
			spawnedChild.transform.SetParent(spawnedChildrenParent.transform);
			spawnedChild.layer = 8;

			if (makeKinematic)
			{
				Rigidbody rb = spawnedChild.GetComponent<Rigidbody>();
				if (rb != null)
				{
					rb.isKinematic = true;
				}
			}
		}
	}
}