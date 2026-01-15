using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(Transform))]
public class InteractableObjectHelper : Editor
{
    private static readonly string[] InteractableComponents = new string[]
    {
        "MaquinaDePalomitas",
        "MaquinaDeBebidas", 
        "MaquinaDePerritos",
        "MaquinaDeItems",
        "Papelera",
        "CampanaInteractiva",
        "GestorPedidos",
        "TabletManager",
        "ItemData"
    };

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Transform transform = (Transform)target;
        
        // Verificar si el objeto tiene algún componente interactuable
        bool hasInteractableComponent = false;
        foreach (string componentName in InteractableComponents)
        {
            if (transform.GetComponent(componentName) != null)
            {
                hasInteractableComponent = true;
                break;
            }
        }

        if (hasInteractableComponent)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Este objeto es interactuable", MessageType.Info);
            
            // Verificar si tiene Outline
            Outline outline = transform.GetComponent<Outline>();
            if (outline == null)
            {
                EditorGUILayout.HelpBox("¡Falta el componente Outline!", MessageType.Warning);
                if (GUILayout.Button("Añadir Outline"))
                {
                    Undo.AddComponent<Outline>(transform.gameObject);
                    outline = transform.GetComponent<Outline>();
                    outline.OutlineColor = Color.white;
                    outline.OutlineWidth = 3f;
                    outline.enabled = false;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Outline configurado correctamente", MessageType.None);
            }

            // Verificar si tiene Collider
            Collider collider = transform.GetComponent<Collider>();
            if (collider == null)
            {
                EditorGUILayout.HelpBox("¡Falta el componente Collider!", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Collider configurado correctamente", MessageType.None);
            }
        }
    }
}
#endif
