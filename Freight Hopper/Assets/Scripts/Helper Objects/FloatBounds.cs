using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FloatBounds
{
    [SerializeField]
    private float min;
    [SerializeField]
    private float max;

    FloatBounds(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float Min => min;
    public float Max => max;

    public float Clamp(float num)
    {
        return Mathf.Clamp(num, min, max);
    }
}
