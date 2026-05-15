using UnityEngine;
using UnityEngine.UI;

public class AnonymisationUI : MonoBehaviour
{
    public Toggle UserAnonymisationToggle;
    public Toggle ServerAnonymisationToggle;

    private void Start()
    {
        if (UserAnonymisationToggle != null)
        {
            UserAnonymisationToggle.isOn = TrackerManager.UserAnonymisation;
            UserAnonymisationToggle.onValueChanged.AddListener(value =>
                TrackerManager.UserAnonymisation = value);
        }

        if (ServerAnonymisationToggle != null)
        {
            ServerAnonymisationToggle.isOn = TrackerManager.ServerAnonymisation;
            ServerAnonymisationToggle.onValueChanged.AddListener(value =>
                TrackerManager.ServerAnonymisation = value);
        }
    }
}
