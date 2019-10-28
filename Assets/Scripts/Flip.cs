using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flip : MonoBehaviour
{
   

   public void doFlip()
    {
        transform.Rotate(new Vector3(0, 0, 180));

    }
}
