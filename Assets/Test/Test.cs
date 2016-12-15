using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour
{
    public RectTransform button;

    // Use this for initialization
    void Start()
    {
        LeanTween.value(button.gameObject, button.anchoredPosition, new Vector2(200f, 200f), 1f).setOnUpdate((Vector2 val) =>
        {
            //button.anchoredPosition = val;
        }).setDelay(4f).setEaseInBack();

        Debug.LogWarning(" time " + Time.realtimeSinceStartup);
        LeanTween.value(this.gameObject, 1, 10, 3).setDelay(1f).setOnUpdate((float val) =>
        {
            Debug.LogWarning(" val1 = " + val + " " + Time.realtimeSinceStartup);
        }).setOnComplete(() =>
        {
            Debug.LogWarning(" over1 " + Time.realtimeSinceStartup);
        });
    }

    // Update is called once per frame
    void Update()
    {
    }
}