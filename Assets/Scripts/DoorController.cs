using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator animator;
    public void OpenTheDoor()
    {
        animator.SetBool("ifOpen", true);
    }

    public void CloseTheDoor()
    {
        animator.SetBool("ifOpen", false);
    }
}
