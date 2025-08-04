using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class CameraWaterCheck : MonoBehaviour {

    

    private List<Collider> triggers = new List<Collider> ();



    private void OnTriggerEnter (Collider other) {

        

        if (!triggers.Contains (other))

            triggers.Add (other);



    }



    private void OnTriggerExit (Collider other) {

        

        if (triggers.Contains (other))

            triggers.Remove (other);



    }



    public bool IsUnderwater ()
    {
        int i = 0;
        while (i < triggers.Count)
        {
            Collider trigger = triggers[i];

            if (trigger == null)
            {
                triggers.Remove(trigger);
                Debug.LogWarning("CameraWaterCheck: Removed a trigger because it was null");
                continue;
            }

            if (trigger.GetComponentInParent<Water>())
                return true;
            i++;
        }
        return false;
    }



}

