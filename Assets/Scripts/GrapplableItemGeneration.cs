using System.Collections.Generic;
using UnityEngine;

public class GrapplableItemGeneration : MonoBehaviour
{
	[System.Serializable]
	public class SpawnableItem
	{
		public GameObject prefab;
		[Tooltip("Relative probability of this item being spawned.")]
		[Range(0.01f, 1f)]
		public float spawnWeight = 1f;
	}

	[Header("Sandbox Toys Parent")]
	[Tooltip("The parent GameObject containing sandbox toy prefabs as children.")]
	public GameObject sandboxToyParent;

	[Header("General Toys Parent")]
	[Tooltip("The parent GameObject containing general toy prefabs as children.")]
	public GameObject generalToyParent;


	[Header("Sandboxes")]
	public List<GameObject> sandboxes = new List<GameObject>();

	[Header("Spawn Settings")]
	[Tooltip("The overall number of toys to spawn.")]
	public int totalToysToSpawn = 20;
	[Tooltip("Percentage of toys that should be sandbox-specific.")]
	[Range(0, 1)]
	public float sandboxToyRatio = 0.3f;
	[Tooltip("The radius around sandboxes where sandbox toys can spawn.")]
	public float sandboxSpawnRadius = 2f;
	[Tooltip("The general spawn area for non-sandbox toys.")]
	public Vector3 generalSpawnAreaSize = new Vector3(20f, 1f, 20f); // x, y, z extents
	[Tooltip("The center point for general toy spawning.")]
	public Vector3 generalSpawnAreaCenter = Vector3.zero;
	[Tooltip("Minimum distance between spawned toys.")]
	public float minSpawnDistance = 0.5f;
	[Tooltip("Minimum distance to other structures.")]
	public float minStructureDistance = 2f;
	[Tooltip("Maximum number of attempts to find a valid spawn position.")]
	public int maxSpawnAttempts = 20;

	[Header("Park Boundaries")]
	[Tooltip("Maximum X coordinate of the park.")]
	public float parkMaxX = 50f;
	[Tooltip("Maximum Z coordinate of the park.")]
	public float parkMaxZ = 50f;
	[Tooltip("Minimum X coordinate of the park.")]
	public float parkMinX = -50f;
	[Tooltip("Minimum Z coordinate of the park.")]
	public float parkMinZ = -50f;

	[Header("Structures to Avoid")]
	[Tooltip("Parent GameObjects whose children should be avoided.")]
	public List<GameObject> structureParents = new List<GameObject>();

	private List<SpawnableItem> sandboxToyPrefabs = new List<SpawnableItem>();
	private List<SpawnableItem> generalToyPrefabs = new List<SpawnableItem>();


	void Start()
	{
		// Populate the prefab lists from the children of the parent GameObjects.
		InitializeToyPrefabs();
		SpawnToys();
	}
	private void InitializeToyPrefabs()
	{
		// Sandbox Toys
		if (sandboxToyParent != null)
		{
			foreach (Transform child in sandboxToyParent.transform)
			{
				// *** IMPORTANT: Ensure the child GameObject is active! ***
				if (child.gameObject.activeSelf)
				{
					sandboxToyPrefabs.Add(new SpawnableItem { prefab = child.gameObject, spawnWeight = Random.Range(0.1f, 1f) }); //random weights
				}
			}
		}
		else
		{
			Debug.LogWarning("Sandbox Toy Parent is not assigned.");
		}
		//General toys
		if (generalToyParent != null)
		{
			foreach (Transform child in generalToyParent.transform)
			{
				// *** IMPORTANT: Ensure the child GameObject is active! ***
				if (child.gameObject.activeSelf)
				{
					generalToyPrefabs.Add(new SpawnableItem { prefab = child.gameObject, spawnWeight = Random.Range(0.1f, 1f) });//random weights.
				}
			}
		}
		else
		{
			Debug.LogWarning("General Toy Parent is not assigned.");
		}
	}

	private GameObject WeightedRandomSelection(List<SpawnableItem> items)
	{
		if (items.Count == 0)
		{
			Debug.LogWarning("WeightedRandomSelection called with an empty list!");
			return null; // Or some default object if you have one
		}

		float totalWeight = 0;
		foreach (var item in items)
		{
			totalWeight += item.spawnWeight;
		}

		float randomNumber = Random.Range(0f, totalWeight);

		float cumulativeWeight = 0;
		foreach (var item in items)
		{
			cumulativeWeight += item.spawnWeight;
			if (randomNumber <= cumulativeWeight)
			{
				return item.prefab;
			}
		}

		return items[items.Count - 1].prefab;
	}

	void SpawnToys()
	{
		int sandboxToysToSpawn = Mathf.RoundToInt(totalToysToSpawn * sandboxToyRatio);
		int generalToysToSpawn = totalToysToSpawn - sandboxToysToSpawn;

		// Spawn Sandbox Toys
		for (int i = 0; i < sandboxToysToSpawn; i++)
		{
			if (sandboxes.Count == 0)
			{
				Debug.LogWarning("No sandboxes assigned.  Cannot spawn sandbox toys.");
				break;
			}

			GameObject randomSandbox = sandboxes[Random.Range(0, sandboxes.Count)];

			Renderer sandboxRenderer = randomSandbox.GetComponent<Renderer>();
			if (sandboxRenderer == null)
			{
				Debug.LogWarning("Sandbox does not contain a renderer, and therefore bounds cannot be calculated.");
				continue;
			}

			for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
			{
				Vector3 randomOffset = Random.insideUnitSphere * sandboxSpawnRadius;
				randomOffset.y = 0;
				Vector3 potentialPosition = randomSandbox.transform.position + randomOffset;

				potentialPosition.x = Mathf.Clamp(potentialPosition.x, sandboxRenderer.bounds.min.x, sandboxRenderer.bounds.max.x);
				potentialPosition.z = Mathf.Clamp(potentialPosition.z, sandboxRenderer.bounds.min.z, sandboxRenderer.bounds.max.z);
				potentialPosition.y = sandboxRenderer.bounds.max.y;

				if (potentialPosition.x > parkMaxX || potentialPosition.x < parkMinX || potentialPosition.z > parkMaxZ || potentialPosition.z < parkMinZ)
				{
					continue;
				}

				if (!IsPositionValid(potentialPosition) || !IsFarFromStructures(potentialPosition))
				{
					continue;
				}
				//Check if we have any sandbox toys to begin with!
				if (sandboxToyPrefabs.Count > 0)
				{
					if (IsPositionValid(potentialPosition))
					{
						GameObject prefabToSpawn = WeightedRandomSelection(sandboxToyPrefabs);
						// IMPORTANT NULL CHECK
						if (prefabToSpawn != null)
						{
							Instantiate(prefabToSpawn, potentialPosition, Quaternion.identity);
						}
						else
						{
							Debug.LogError("Selected prefab to spawn is null (Sandbox Toys)!");
						}

						break;
					}
				}
				else
				{
					Debug.LogWarning("No sandbox toy prefabs available to spawn.");
					break; // Exit the loop if there are no sandbox toys
				}

				if (attempt == maxSpawnAttempts - 1)
				{
					Debug.Log("Max attempts reached, could not spawn.");
				}
			}
		}

		// Spawn General Toys
		for (int i = 0; i < generalToysToSpawn; i++)
		{
			for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
			{
				Vector3 randomPosition = new Vector3(
				Random.Range(-generalSpawnAreaSize.x / 2, generalSpawnAreaSize.x / 2),
				0,
				Random.Range(-generalSpawnAreaSize.z / 2, generalSpawnAreaSize.z / 2)
				);

				randomPosition += generalSpawnAreaCenter;

				if (randomPosition.x > parkMaxX || randomPosition.x < parkMinX || randomPosition.z > parkMaxZ || randomPosition.z < parkMinZ)
				{
					continue;
				}

				if (Physics.Raycast(randomPosition + Vector3.up * 100f, Vector3.down, out RaycastHit hit, Mathf.Infinity))
				{
					randomPosition.y = hit.point.y;
				}
				else
				{
					randomPosition.y = 0;
				}

				if (!IsPositionValid(randomPosition) || !IsFarFromStructures(randomPosition))
				{
					continue;
				}
				//Check if we have any general toys to begin with
				if (generalToyPrefabs.Count > 0)
				{
					if (IsPositionValid(randomPosition))
					{
						GameObject prefabToSpawn = WeightedRandomSelection(generalToyPrefabs);
						//IMPORTANT NULL CHECK
						if (prefabToSpawn != null)
						{
							Instantiate(prefabToSpawn, randomPosition, Quaternion.identity);
						}
						else
						{
							Debug.LogError("Selected prefab to spawn is null (General Toys)!");
						}
						break;
					}
				}
				else
				{
					Debug.LogWarning("No general toy prefabs available to spawn.");
					break;
				}


				if (attempt == maxSpawnAttempts - 1)
				{
					Debug.Log("Max attempts reached, could not spawn.");
				}
			}
		}
	}

	bool IsPositionValid(Vector3 position)
	{
		Collider[] colliders = Physics.OverlapSphere(position, minSpawnDistance);
		return colliders.Length == 0;
	}

	bool IsFarFromStructures(Vector3 position)
	{
		foreach (GameObject parent in structureParents)
		{
			// Iterate through all children of the parent
			foreach (Transform child in parent.transform)
			{
				if (Vector3.Distance(position, child.position) < minStructureDistance)
				{
					return false; // Too close to a structure
				}
			}
		}
		return true; // Far enough from all structures
	}

	void OnDrawGizmosSelected()
	{
		// General Spawn Area
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(generalSpawnAreaCenter, generalSpawnAreaSize);

		// Sandbox Spawn Radii
		Gizmos.color = Color.yellow;
		foreach (GameObject sandbox in sandboxes)
		{
			if (sandbox != null)
			{
				Gizmos.DrawWireSphere(sandbox.transform.position, sandboxSpawnRadius);
			}
		}

		// Park Boundaries
		Gizmos.color = Color.green;
		Vector3 parkCenter = new Vector3((parkMaxX + parkMinX) / 2, 0, (parkMaxZ + parkMinZ) / 2);
		Vector3 parkSize = new Vector3(parkMaxX - parkMinX, 1, parkMaxZ - parkMinZ);
		Gizmos.DrawWireCube(parkCenter, parkSize);

		// Structure Avoidance
		Gizmos.color = Color.red;
		foreach (GameObject parent in structureParents)
		{
			foreach (Transform child in parent.transform)
			{
				Gizmos.DrawWireSphere(child.position, minStructureDistance);
			}
		}
	}
}