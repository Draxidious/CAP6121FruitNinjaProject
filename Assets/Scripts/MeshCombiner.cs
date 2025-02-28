using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
	public bool combineOnStart = true;
	public bool deactivateOriginals = false;
	public bool generateUv2 = false;

	public void CombineMeshesFromChildren()
	{
		// 1. Gather MeshFilters from children
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

		// Check for no meshes
		if (meshFilters.Length == 0)
		{
			Debug.LogWarning("No meshes found in children to combine.");
			return;
		}
		//Check to make sure there is at least one renderer
		MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
		if (renderers.Length == 0)
		{
			Debug.LogWarning("No renderers found in child objects. Need a material to apply to the combined mesh.");
			return;
		}

		//We'll use the first object's material.  Make sure all objects use the same material for best results.
		Material combinedMaterial = renderers[0].sharedMaterial;


		// 2. Create CombineInstance array
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		for (int i = 0; i < meshFilters.Length; i++)
		{
			// Check if the current MeshFilter's GameObject is this GameObject
			if (meshFilters[i].gameObject == this.gameObject)
			{
				continue; // Skip this MeshFilter
			}

			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].subMeshIndex = 0;  // Assume single submesh
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
		}

		//Check to see if there is at least one element.
		bool hasElements = false;
		foreach (CombineInstance element in combine)
		{
			if (element.mesh != null)
			{
				hasElements = true;
				break;  // Exit the loop as soon as we find one element
			}
		}

		//If no elements, return.
		if (!hasElements)
		{
			Debug.LogWarning("No meshes found in children to combine. Skipping.");
			return;
		}


		// 3. Create a new mesh
		Mesh combinedMesh = new Mesh();

		// 4. Call CombineMeshes
		combinedMesh.CombineMeshes(combine, true, true, false);

		// 5. Create New GameObject (or modify this one)
		//    - Check if a MeshFilter already exists
		MeshFilter thisMeshFilter = this.GetComponent<MeshFilter>();
		if (thisMeshFilter == null)
		{
			thisMeshFilter = gameObject.AddComponent<MeshFilter>();
		}

		//    - Check if a MeshRenderer already exists
		MeshRenderer thisMeshRenderer = this.GetComponent<MeshRenderer>();
		if (thisMeshRenderer == null)
		{
			thisMeshRenderer = gameObject.AddComponent<MeshRenderer>();
		}


		// 6. Assign the combined mesh to this GameObject
		thisMeshFilter.mesh = combinedMesh;

		//assign material
		thisMeshRenderer.material = combinedMaterial;

		// 7. (Optional) Generate lightmap UVs
		if (generateUv2)
		{
			UnityEditor.Unwrapping.GenerateSecondaryUVSet(combinedMesh);
		}

		// 8. Deactivate original objects
		if (deactivateOriginals)
		{
			foreach (MeshFilter meshFilter in meshFilters)
			{
				//Deactivate every original object EXCEPT this object
				if (meshFilter.gameObject != this.gameObject)
				{
					meshFilter.gameObject.SetActive(false);
				}

			}
		}

		//Make this object static
		gameObject.isStatic = true;
	}
}