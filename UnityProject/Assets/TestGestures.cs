using UnityEngine;

public class TestGestures : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }

    public void PrintGesture(string gesture)
    {
        Debug.Log("function called");
        if (gesture == "Thumbs Up")
        {
            Debug.Log("Thumbs Up");
        } else
        {
            Debug.Log("Thumbs Down");
        }
    }
}
