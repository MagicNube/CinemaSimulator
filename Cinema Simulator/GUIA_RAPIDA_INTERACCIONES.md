# Guía Rápida de Configuración

## ✅ Checklist de Configuración

### Paso 1: Canvas UI
- [ ] Canvas "Interfaz" existe en la escena
- [ ] Canvas tiene componente `UIInteractionManager`
- [ ] Panel hijo "PanelInteraccion" creado
- [ ] TextMeshPro "TextoInteraccion" dentro del Panel
- [ ] Referencias asignadas en UIInteractionManager

### Paso 2: Objetos Interactuables
Para cada objeto interactuable (máquinas, papelera, campana, etc.):
- [ ] Tiene Collider
- [ ] Tiene componente Outline (OutlineColor = blanco, OutlineWidth = 3)
- [ ] Outline está DESACTIVADO por defecto
- [ ] Tiene su script de interacción específico

### Paso 3: Jugador
- [ ] ControladorInteraccion tiene todas las referencias asignadas
- [ ] La cámara está configurada correctamente
- [ ] El script de movimiento está asignado

## 🎯 Configuración Rápida del Canvas

```
Jerarquía Recomendada:

Canvas (Interfaz)
├── UIInteractionManager (Script)
└── PanelInteraccion (Panel)
    └── TextoInteraccion (TextMeshPro)

Propiedades de PanelInteraccion:
- Anchor: Bottom Center
- Pos X: 0, Pos Y: 100
- Width: 400, Height: 80
- Color: Negro con Alpha 180

Propiedades de TextoInteraccion:
- Fuente: [Tu fuente preferida]
- Tamaño: 24
- Color: Blanco
- Alineación: Centro
- Anchor: Stretch (llenar todo el panel)
```

## 🔨 Ejemplo: Configurar una Máquina de Palomitas

1. Selecciona la máquina de palomitas en la jerarquía
2. Verifica que tiene:
   - Component: `MaquinaDePalomitas` ✓
   - Component: BoxCollider (o el que tenga) ✓
3. Añade el componente `Outline`:
   - Add Component → escribir "Outline" → Enter
   - En el Inspector del Outline:
     - Outline Color: RGB(255, 255, 255) - Blanco
     - Outline Width: 3
     - **DESMARCAR** el checkbox del componente (disabled)
4. ¡Listo!

## 🎮 Probando el Sistema

1. Entra en Play Mode
2. Acércate a un objeto interactuable (máquina, papelera, etc.)
3. Míralo con el centro de la cámara
4. Deberías ver:
   - ✨ Un outline blanco alrededor del objeto
   - 📋 El panel UI con el texto de interacción
5. Deja de mirarlo:
   - El outline desaparece
   - El panel UI se oculta

## 🐛 Problemas Comunes

### "No veo el outline"
- El Outline debe estar DESACTIVADO en el Inspector cuando no lo miras
- Se activa automáticamente al mirarlo

### "No veo el UI"
- Comprueba que el Canvas está en modo Screen Space - Overlay
- Verifica que el Panel no está fuera de la pantalla
- Asegúrate de que el Canvas Scaler está configurado

### "El raycast no detecta"
- La distancia por defecto es 3 metros
- Puedes aumentarla en `distanciaInteraccion` del ControladorInteraccion

## 📍 Ubicación de Archivos

```
Assets/
├── Scripts/
│   ├── ControladorInteracción.cs (MODIFICADO)
│   ├── UIInteractionManager.cs (NUEVO)
│   └── Editor/
│       └── InteractableObjectHelper.cs (NUEVO)
└── [Tu estructura de proyecto...]
```

## 💬 Mensajes Personalizables

Si quieres cambiar los mensajes, edita el método `ObtenerMensajeInteraccion()` en ControladorInteracción.cs

Ejemplo de línea a modificar:
```csharp
// Línea ~237
return "Click izquierdo - Llenar cubo";

// Cámbiala por:
return "[E] Llenar cubo de palomitas";
```

## 🎨 Estilos de UI Recomendados

### Estilo Moderno
- Fondo del panel: Negro #000000 con Alpha 200
- Texto: Blanco #FFFFFF, Bold, Tamaño 20-24
- Sombra en el texto para mejor legibilidad

### Estilo Vibrante  
- Fondo del panel: Azul oscuro #1a237e con Alpha 220
- Texto: Amarillo #ffeb3b, Tamaño 22-26
- Outline en el texto: Negro con distancia 1

### Estilo Minimalista
- Fondo del panel: Gris oscuro #424242 con Alpha 150
- Texto: Blanco #FFFFFF, Light, Tamaño 18-20
- Sin efectos adicionales

¡Elige el que mejor vaya con tu juego!
