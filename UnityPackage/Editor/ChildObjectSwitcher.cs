using UnityEditor;
using UnityEngine;

/// <summary>
/// Child object switcher.
/// </summary>
public class ChildObjectSwitcher : EditorWindow
{

    GameObject holderObject;

    [MenuItem("Window/Mmzk/Child Object Switcher")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ChildObjectSwitcher window = (ChildObjectSwitcher)EditorWindow.GetWindow(typeof(ChildObjectSwitcher));
        window.Show();
    }

    void OnGUI()
    {
        if(holderObject==null)
        {
            GameObject holderObjectCheck = GameObject.Find("_holder");
            if (holderObjectCheck != null)
            {
                holderObject = holderObjectCheck;
            }
        }

        holderObject = (GameObject)EditorGUILayout.ObjectField(holderObject, typeof(GameObject), true);
        if(holderObject==null)
        {
            return;
        }

        GUILayout.BeginVertical();

        GUILayout.Label("All:");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Activate"))
        {
            for (int i = 0; i < holderObject.transform.childCount; i++)
            {
                var co = holderObject.transform.GetChild(i).gameObject;
                co.SetActive(true);
            }
        }
        if (GUILayout.Button("Deactivate"))
        {
            for (int i = 0; i < holderObject.transform.childCount; i++)
            {
                var co = holderObject.transform.GetChild(i).gameObject;
                co.SetActive(false);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Items:");
        foreach (Transform childObject in holderObject.transform)
        {
            if(GUILayout.Button(childObject.gameObject.name))
            {
                for(int i=0;i<holderObject.transform.childCount;i++)
                {
                    var co = holderObject.transform.GetChild(i).gameObject;
                    co.SetActive(co.name == childObject.gameObject.name);
                }
            }
        }
        GUILayout.EndVertical();
    }

}
