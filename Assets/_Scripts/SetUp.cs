using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetUp : MonoBehaviour
{
    private void Awake()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Agent"), LayerMask.NameToLayer("Agent"), true);
    }
}