using UnityEngine;

public class ToyManager : MonoBehaviour
{
    public ChildGlow glow;
    public Toys toys;
    public SandToys sandToys;

    void Start()
    {
        toys.SpawnToys();
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			GameObject Go = gameObject.transform.GetChild(i).gameObject;
            MeshCombiner meshComb = Go.GetComponent<MeshCombiner>();
            if(meshComb != null)
            {
                meshComb.CombineMeshesFromChildren();
			}
		}
        sandToys.SpawnChildren();
		glow.CreateGlowObjects();
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
