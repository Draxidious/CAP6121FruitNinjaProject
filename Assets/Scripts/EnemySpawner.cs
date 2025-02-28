using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
	[System.Serializable]
	public class SpawnObject
	{
		public GameObject prefab;
		public int quantity;
		public float minYPosition;
		public float maxYPosition;
	}

	public SpawnObject[] objectsToSpawn;

	public float minX = -10f;
	public float maxX = 10f;
	public float minZ = -10f;
	public float maxZ = 10f;

	public GameObject parentObject; // Assign the parent in the inspector.

	void Start()
	{
		SpawnAllObjects();
	}

	void SpawnAllObjects()
	{
		if (parentObject == null)
		{
			Debug.LogError("ParentObject not assigned in the inspector.");
			return;
		}

		foreach (SpawnObject spawnObject in objectsToSpawn)
		{
			for (int i = 0; i < spawnObject.quantity; i++)
			{
				SpawnObjectAtRandomPosition(spawnObject);
			}
		}
	}

	void SpawnObjectAtRandomPosition(SpawnObject spawnObject)
	{
		float randomX = Random.Range(minX, maxX);
		float randomZ = Random.Range(minZ, maxZ);
		float randomY = Random.Range(spawnObject.minYPosition, spawnObject.maxYPosition); // Random Y within range

		Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);

		GameObject spawnedObject = Instantiate(spawnObject.prefab, spawnPosition, Quaternion.identity);
		spawnedObject.transform.SetParent(parentObject.transform); // Set the parent.
	}

	//// Optional: Draw the spawn area in the Scene view for visualization.
	//void OnDrawGizmosSelected()
	//{
	//	Gizmos.color = Color.yellow;
	//	Gizmos.DrawWireCube(new Vector3((minX + maxX) / 2f, (objectsToSpawn.Length > 0 ? (objectsToSpawn.minYPosition + objectsToSpawn.maxYPosition) / 2f : 0), (minZ + maxZ) / 2f), new Vector3(maxX - minX, (objectsToSpawn.Length > 0 ? objectsToSpawn.maxYPosition - objectsToSpawn.minYPosition : 0.1f), maxZ - minZ));
	//}
}