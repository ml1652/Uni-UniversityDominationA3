﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmark : MonoBehaviour
{
    #region Private Fields

    [SerializeField]
    ResourceType resourceType;
    [SerializeField]
    int amount = 2;

    #endregion

    #region Public Properties

    public ResourceType ResourceType
    {
        get { return resourceType; }
        set { resourceType = value; }
    }

    public int Amount
    {
        get { return amount; }
        set { amount = value; }
    }

    #endregion
}
