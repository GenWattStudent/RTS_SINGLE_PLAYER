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
    [SerializeField] private GameObject popupWorldPrefab;
    [SerializeField] private GameObject popupPrefabError;
    [SerializeField] private GameObject popupPrefabInfo;
    public static List<PopupData> popups = new ();

    public Popup ShowPopup(string message, Vector3 position, float duration = 2f, bool followMouse = false, Vector2 offset = new Vector2()) {
        var popup = Instantiate(popupPrefab, position, Quaternion.identity);
        popup.transform.SetParent(transform);
        var popupScript = popup.GetComponent<Popup>();

        popupScript.Show(message, duration);
        popups.Add(new PopupData { message = message, duration = duration, followMouse = followMouse, offset = offset, popup = popupScript });

        return popupScript;
    }

    public Popup ShowPopupWorld(string message, Vector3 position, float duration = 2f, Vector3 offset = new Vector3()) {
        var popup = Instantiate(popupWorldPrefab, position + offset, Quaternion.identity);
        var popupScript = popup.GetComponent<Popup>();

        popupScript.Show(message, duration);
        popups.Add(new PopupData { message = message, duration = duration, popup = popupScript });

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

    public  void DestroyPopup(Popup popup) {
        if (popup == null) return;
        popups.Remove(popups.Find(p => p.popup == popup));
        // destroy ui element
        Destroy(popup.gameObject);
    }

    private void Update() {
        foreach (var popup in popups) {
            if (popup.followMouse) {
                var mousePosition = Input.mousePosition + new Vector3(popup.offset.x, popup.offset.y, 0);
                popup.popup.transform.position = mousePosition;
            }
        }
    }
}

