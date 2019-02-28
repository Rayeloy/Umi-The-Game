using UnityEngine;
using UnityEditor;

public abstract class EditorWithSubEditors<TEditor, TTarget> : Editor
    where TEditor : Editor
    where TTarget : Object
{
    [SerializeField]
    public TEditor[] subEditors;


    protected virtual void CheckAndCreateSubEditors(TTarget[] subEditorTargets)
    {
        //Debug.Log("CheckAndCreateSubEditors starts");
        if (subEditors != null && subEditors.Length == subEditorTargets.Length)
        {
            Debug.Log("CheckAndCreateSubEditors exits");
            return;
        }
        Debug.Log("subEditors[] =  "+subEditors);

        CleanupEditors();

        subEditors = new TEditor[subEditorTargets.Length];

        for (int i = 0; i < subEditors.Length; i++)
        {
            subEditors[i] = CreateEditor(subEditorTargets[i]) as TEditor;
            SubEditorSetup(subEditors[i]);
            Debug.Log("Editor for " + subEditorTargets[i]+" created");
        }
    }


    protected void CleanupEditors()
    {
        if (subEditors == null)
            return;

        for (int i = 0; i < subEditors.Length; i++)
        {
            DestroyImmediate(subEditors[i]);
        }

        subEditors = null;
    }


    protected abstract void SubEditorSetup(TEditor editor);
}