using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour
{
    void Start()
    {
        LeanTween.value(this.gameObject, 1, 10, 3).setDelay(1f).setOnUpdate((float val) =>
        {
            Debug.LogWarning(" val1 = " + val + " " + Time.realtimeSinceStartup);
        }).setOnComplete(() =>
        {
            Debug.LogWarning(" over1 " + Time.realtimeSinceStartup);
        });
    }

    void Update()
    {
    }
}