using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    public class PopupData {
        public Popup popup;
        public string message;
        public float duration;
        public bool followMouse;
        public Vector2 offset;
        public Guid id = Guid.NewGuid();
    }

    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private GameObject popupPrefabError;
    [SerializeField] private GameObject popupPrefabInfo;
    public List<PopupData> popups = new ();

    public Popup ShowPopup(string message, float duration = 2f, bool followMouse = false, Vector2 offset = new Vector2()) {
        var popup = Instantiate(popupPrefab, transform);
        var popupScript = popup.GetComponent<Popup>();

        popupScript.Show(message, duration);
        popups.Add(new PopupData { message = message, duration = duration, followMouse = followMouse, offset = offset, popup = popupScript });

        return popupScript;
    }

    public Popup ShowPopupError(string message, float duration = 2f) {
        var popup = Instantiate(popupPrefabError, transform);
        var popupScript = popup.GetComponent<Popup>();

        popupScript.Show(message, duration);
        // popups.Add(new PopupData { message = message, duration = duration })

        return popupScript;
    }

    public Popup ShowPopupInfo(string message, float duration = 2f) {
        var popup = Instantiate(popupPrefabInfo, transform);
        var popupScript = popup.GetComponent<Popup>();

        popupScript.Show(message, duration);
        // popups.Add(popupScript);

        return popupScript;
    }

    public void DestroyPopup(Popup popup) {
        popups.Remove(popups.Find(p => p.popup == popup));
        Debug.Log("Destroy popup " + popup);
        // destroy ui element
        Destroy(popup.gameObject);
    }

    private void Update() {
        foreach (var popup in popups) {
            if (popup.followMouse) {
                var mousePosition = Input.mousePosition + (Vector3)popup.offset;
                mousePosition.z = 10;
                popup.popup.transform.position = mousePosition;
            }
        }
    }
}

