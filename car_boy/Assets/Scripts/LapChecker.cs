using UnityEngine;

public class LapChecker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Car"))
        {
            Debug.Log("Lap completed by " + other.name);
            if(other.gameObject.transform.root.gameObject.GetComponent<MaxVerstappenDriver>() != null)
                other.gameObject.transform.root.gameObject.GetComponent<MaxVerstappenDriver>().CollideStart();
        }
    }
}
