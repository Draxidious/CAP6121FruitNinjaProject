using UnityEngine;
using System.Collections.Generic;

public class SandToys : MonoBehaviour
{
	public List<GameObject> objectsToSpawn; // List of GameObjects to spawn.
	public List<GameObject> spawnAroundObjects;
	public float minSpawnDistance = 1f;
	public float maxSpawnDistance = 5f;
	public int numberOfSpawns = 10;
	public GameObject spawnedChildrenParent;
	public float fixedYPosition = 0.1f;
	public bool snapToGround = true;
	public bool makeKinematic = true;

	void Awake()
	{
		SpawnChildren();
	}

	public void SpawnChildren()
	{
		if (objectsToSpawn == null || objectsToSpawn.Count == 0 || spawnAroundObjects == null || spawnAroundObjects.Count == 0 || spawnedChildrenParent == null)
		{
			Debug.LogError("Objects to spawn list, spawn around objects list, or spawned children parent is not assigned.");
			return;
		}

		for (int i = 0; i < numberOfSpawns; i++)
		{
			// Choose a random object to spawn.
			GameObject objectToSpawn = objectsToSpawn[Random.Range(0, objectsToSpawn.Count)];
			GameObject spawnAround = spawnAroundObjects[Random.Range(0, spawnAroundObjects.Count)];

			Vector3 randomDirection = Random.insideUnitSphere * Random.Range(minSpawnDistance, maxSpawnDistance);
			Vector3 spawnPosition = spawnAround.transform.position + randomDirection;

			spawnPosition.y = fixedYPosition;

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

	void OnDrawGizmosSelected()
	{
		if (spawnAroundObjects != null)
		{
			foreach (GameObject spawnAround in spawnAroundObjects)
			{
				if (spawnAround != null)
				{
					Gizmos.color = Color.yellow;
					Gizmos.DrawWireSphere(spawnAround.transform.position, minSpawnDistance);
					Gizmos.color = Color.red;
					Gizmos.DrawWireSphere(spawnAround.transform.position, maxSpawnDistance);
				}
			}
		}
	}
}