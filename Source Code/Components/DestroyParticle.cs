using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyParticle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
     float timeTillDeath = 5;
    // Update is called once per frame
    void Update()
    {
        timeTillDeath -= Time.deltaTime;
        if(timeTillDeath <= 0)
        {
            Destroy(gameObject);
        }
    }
}
