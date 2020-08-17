using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClayxelSolids : MonoBehaviour
{
	Clayxels.ClayContainer clayxel;
	
    // Start is called before the first frame update
    void Start()
    {
        this.clayxel = this.gameObject.GetComponent<Clayxels.ClayContainer>();
        this.clayxel.init();

        List<Clayxels.Solid> solids = this.clayxel.getSolids();

        for(int i = 0; i < 128; ++i){
        	Clayxels.Solid newSolid = new Clayxels.Solid();
    		
        	newSolid.position.x = UnityEngine.Random.Range(-1.0f, 1.0f);
        	newSolid.position.y = UnityEngine.Random.Range(-1.0f, 1.0f);
        	newSolid.position.z = UnityEngine.Random.Range(-1.0f, 1.0f);

        	solids.Add(newSolid);
        }
        
        this.clayxel.updatedSolidCount();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        List<Clayxels.Solid> solids = this.clayxel.getSolids();
		
        for(int i = 0; i < solids.Count; ++i){
        	Clayxels.Solid solid = solids[i];

        	solid.position.y += 0.1f;

        	if(solid.position.y > 4.0f){
        		solid.position.y = -4.0f;
        	}

            solid.rotation.eulerAngles = solid.rotation.eulerAngles + new Vector3(0.0f, 10.0f, 0.0f);

            this.clayxel.solidUpdated(i);
        }
    }
}
