using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings"), SerializeField] 
    private GameObject[] items;

    [SerializeField]
    private float spawnChance;

    [Header("Raycast Settings"), SerializeField]
    private float distanceBetweenItems;
    
    [SerializeField]
    private Vector3 positiveMaxPosition;
    
    [SerializeField]
    private Vector3 negativeMaxPosition;
    
    private void Start() {
        SpawnItems();
    }

    private void SpawnItems() {
        for (float x = negativeMaxPosition.x; x < positiveMaxPosition.x; x += distanceBetweenItems) {
            for (float z = negativeMaxPosition.z; z < positiveMaxPosition.z; z += distanceBetweenItems) {
                if (Random.Range(0f, 1f) <= spawnChance) {
                    Instantiate(items[Random.Range(0, items.Length)], new Vector3(x, -1, z), Quaternion.identity);
                }
            }
        }
    }
}