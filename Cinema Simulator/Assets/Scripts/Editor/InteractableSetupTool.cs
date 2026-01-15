using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
public class InteractableSetupTool : EditorWindow
{
    [MenuItem("Herramientas/Configurar Objetos Interactuables")]
    public static void ShowWindow()
    {
        GetWindow<InteractableSetupTool>("Setup Interactuables");
    }

    private Vector2 scrollPosition;
    
    void OnGUI()
    {
        GUILayout.Label("Configuración Masiva de Objetos Interactuables", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Esta herramienta añade automáticamente el componente Outline a todos los objetos interactuables en la escena.",
            MessageType.Info
        );
        
        EditorGUILayout.Space();

        if (GUILayout.Button("Buscar y Configurar Todos los Objetos", GUILayout.Height(40)))
        {
            ConfigurarTodosLosObjetos();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        GUILayout.Label("Configuración Individual", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        if (GUILayout.Button("Configurar Máquinas de Palomitas"))
            ConfigurarTipo<MaquinaDePalomitas>("Máquinas de Palomitas");
        
        if (GUILayout.Button("Configurar Máquinas de Bebidas"))
            ConfigurarTipo<MaquinaDeBebidas>("Máquinas de Bebidas");
        
        if (GUILayout.Button("Configurar Máquinas de Perritos"))
            ConfigurarTipo<MaquinaDePerritos>("Máquinas de Perritos");
        
        if (GUILayout.Button("Configurar Máquinas de Items/Envases"))
            ConfigurarTipo<MaquinaDeItems>("Máquinas de Items");
        
        if (GUILayout.Button("Configurar Papeleras"))
            ConfigurarTipo<Papelera>("Papeleras");
        
        if (GUILayout.Button("Configurar Campanas"))
            ConfigurarTipo<CampanaInteractiva>("Campanas");

        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Tip: También puedes seleccionar objetos manualmente y usar el botón 'Añadir Outline' que aparece en el Inspector.",
            MessageType.Info
        );
    }

    void ConfigurarTodosLosObjetos()
    {
        int configurados = 0;
        
        configurados += ConfigurarTipo<MaquinaDePalomitas>("Máquinas de Palomitas", false);
        configurados += ConfigurarTipo<MaquinaDeBebidas>("Máquinas de Bebidas", false);
        configurados += ConfigurarTipo<MaquinaDePerritos>("Máquinas de Perritos", false);
        configurados += ConfigurarTipo<MaquinaDeItems>("Máquinas de Items", false);
        configurados += ConfigurarTipo<Papelera>("Papeleras", false);
        configurados += ConfigurarTipo<CampanaInteractiva>("Campanas", false);
        
        EditorUtility.DisplayDialog(
            "Configuración Completada",
            $"Se configuraron {configurados} objetos interactuables con éxito.",
            "OK"
        );
    }

    int ConfigurarTipo<T>(string nombre, bool mostrarDialogo = true) where T : Component
    {
        T[] objetos = FindObjectsByType<T>(FindObjectsSortMode.None);
        int configurados = 0;
        int yaConfigurados = 0;
        List<string> errores = new List<string>();

        foreach (T obj in objetos)
        {
            GameObject go = obj.gameObject;
            
            // Verificar Collider
            if (go.GetComponent<Collider>() == null)
            {
                errores.Add($"{go.name}: Sin Collider");
            }
            
            // Configurar Outline
            Outline outline = go.GetComponent<Outline>();
            if (outline == null)
            {
                Undo.AddComponent<Outline>(go);
                outline = go.GetComponent<Outline>();
                outline.OutlineColor = Color.white;
                outline.OutlineWidth = 3f;
                outline.enabled = false;
                configurados++;
                EditorUtility.SetDirty(go);
            }
            else
            {
                // Verificar configuración
                if (outline.OutlineColor != Color.white || outline.OutlineWidth != 3f)
                {
                    Undo.RecordObject(outline, "Configurar Outline");
                    outline.OutlineColor = Color.white;
                    outline.OutlineWidth = 3f;
                    EditorUtility.SetDirty(outline);
                    configurados++;
                }
                else
                {
                    yaConfigurados++;
                }
            }
        }

        if (mostrarDialogo)
        {
            string mensaje = $"Tipo: {nombre}\n\n";
            mensaje += $"✓ Configurados: {configurados}\n";
            mensaje += $"○ Ya configurados: {yaConfigurados}\n";
            mensaje += $"Total encontrados: {objetos.Length}\n";
            
            if (errores.Count > 0)
            {
                mensaje += "\n⚠ Advertencias:\n";
                foreach (string error in errores)
                {
                    mensaje += $"• {error}\n";
                }
            }

            EditorUtility.DisplayDialog("Resultado", mensaje, "OK");
        }

        return configurados;
    }
}
#endif
