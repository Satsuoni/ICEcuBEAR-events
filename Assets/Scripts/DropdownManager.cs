using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownManager : MonoBehaviour
{
    Dropdown managed;
    // Start is called before the first frame update
    void Start()
    {
        managed = gameObject.GetComponent<Dropdown>();
        if(managed==null)
        {
            Destroy(this);
            return;
        }
        //Dropdown.OptionData opt;
        //opt.image = null;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
