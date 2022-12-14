using TMPro;
using UnityEngine;

public class Debug_CanvasWorldPosition : MonoBehaviour
{
    [SerializeField] private bool isCameraPosition = false;
    [SerializeField] private GameObject parentObject;
    
    [SerializeField] private TextMeshProUGUI objectNameTextArea;
    [SerializeField] private TextMeshProUGUI positionTextArea;
    [SerializeField] private TextMeshProUGUI rotationTextArea;
    
    void Update()
    {
        if (isCameraPosition)
        {
            parentObject = Camera.main.gameObject;
        }
        objectNameTextArea.text = parentObject.name;
        positionTextArea.text =  "Position " + parentObject.transform.position;
        rotationTextArea.text =  "Rotation " + parentObject.transform.rotation;
    }
}
