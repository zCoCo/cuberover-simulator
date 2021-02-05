using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockPlacer : MonoBehaviour
{

    public GameObject RocksPrefab;
    public int amountMultiplier = 1;
    private GameObject RocksRoot;
    public int RandomSeed = 1;
    public Material RockMaterial;

    void Start()
    {
        //PlaceRocks();
    }


    public void PlaceRocks() {
        UnityEngine.Random.InitState(RandomSeed);

        if (RocksPrefab) {
            if (!RocksRoot)
                RocksRoot = GameObject.Instantiate(RocksPrefab);
            foreach (Transform rockTransform in RocksRoot.transform) {
                RaycastHit hit;
                float r = UnityEngine.Random.Range(0.2f, 1f);
                Vector2 position = UnityEngine.Random.insideUnitCircle * 50;
                
                Vector3 origin = new Vector3(position.x, 10000, position.y);
                Ray ray = new Ray(origin, Vector3.down);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                    rockTransform.position = hit.point + Random.Range(-0.1f, 0) * Vector3.up;
                    rockTransform.rotation = Random.rotation;
                    rockTransform.localScale = Random.Range(0.8f, 1.2f) * Vector3.one;
                }

                if (RockMaterial) {
                    MeshRenderer renderer = rockTransform.gameObject.GetComponent<MeshRenderer>();
                    if (renderer) renderer.material = RockMaterial;
                }

            }
        }
    }

    public void SetNumRocks() {
        if (!RocksPrefab) return;
        if (RocksRoot) DestroyImmediate(RocksRoot);
        RocksRoot = GameObject.Instantiate(RocksPrefab);
        int numRocksInPrefab = RocksPrefab.transform.childCount;
        for (int j = 0; j < numRocksInPrefab; j++) {
            GameObject rock = RocksPrefab.transform.GetChild(j).gameObject;
            for (int i = 0; i < amountMultiplier; i++) {
                Instantiate(rock, RocksRoot.transform);
            }
        }
    }

        // Update is called once per frame
        void Update()
    {

    }
}
