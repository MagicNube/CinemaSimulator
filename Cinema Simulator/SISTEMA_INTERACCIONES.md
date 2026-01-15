# Sistema de Interacciones - Cinema Simulator

## 📋 Resumen
Este sistema maneja todas las interacciones del jugador con objetos del cinema, mostrando un UI informativo y un outline blanco cuando se mira un objeto interactuable.

## 🎮 Tipos de Interacciones Implementadas

### Máquinas de Palomitas y Bebidas
- **Con envase vacío (cubo/vaso):**
  - `Click izquierdo` → Llenar envase
- **Con caja de suministros:**
  - `Click izquierdo` → Reponer 1 unidad
  - `Click derecho` → Reponer todo

### Máquina de Perritos
- **Sin nada en mano:**
  - `Click izquierdo` → Coger perrito
- **Con caja de perritos:**
  - `Click izquierdo` → Reponer 1 unidad
  - `Click derecho` → Reponer todo

### Máquinas de Envases
- **Sin nada en mano:**
  - `Click izquierdo` → Coger envase
- **Con caja de envases:**
  - `Click izquierdo` → Reponer 1 unidad
  - `Click derecho` → Reponer todo

### Campana (Bell)
- `Click izquierdo` → Pedir comanda al cliente

### Papelera
- **Con objeto en mano:**
  - `Click izquierdo` → Tirar objeto

### Máquinas Rotas
- **Con martillo en mano:**
  - `Mantener click izquierdo` → Reparar máquina

### Objetos en el suelo
- `Click izquierdo` → Coger objeto

### Clientes
- **Con pedido en mano:**
  - `Click izquierdo` → Entregar pedido

## 🔧 Configuración en Unity

### 1. Configurar el Canvas UI (Interfaz)

En la jerarquía, busca o crea el Canvas "Interfaz":

1. Selecciona el Canvas `Interfaz`
2. Añade el componente `UIInteractionManager`
3. Crea un Panel hijo (si no existe) para mostrar las interacciones:
   - Click derecho en Canvas → UI → Panel
   - Renómbralo a "PanelInteraccion"
   - Configura su posición (recomendado: parte inferior central de la pantalla)
   - Ajusta el tamaño a tu gusto (ej: 400x80)
   
4. Añade un Text - TextMeshPro hijo al Panel:
   - Click derecho en PanelInteraccion → UI → Text - TextMeshPro
   - Renómbralo a "TextoInteraccion"
   - Configura el texto (tamaño, color, alineación, etc.)
   - Establece el anchor al centro del panel

5. En el componente `UIInteractionManager` del Canvas:
   - Arrastra `PanelInteraccion` al campo "Panel Interaccion"
   - Arrastra `TextoInteraccion` al campo "Texto Interaccion"

### 2. Configurar Objetos Interactuables

Cada objeto que debe ser interactuable necesita:

#### Componentes Requeridos:
1. **Collider** - Para detectar el raycast
2. **Outline** - Para el efecto visual blanco
3. **Script de Interacción** - Uno de los siguientes:
   - `MaquinaDePalomitas`
   - `MaquinaDeBebidas`
   - `MaquinaDePerritos`
   - `MaquinaDeItems` (para envases)
   - `Papelera`
   - `CampanaInteractiva`
   - `GestorPedidos` (clientes)
   - `ItemData` (objetos del suelo)

#### Pasos para configurar un objeto:

1. Selecciona el objeto en la jerarquía
2. Asegúrate de que tiene un **Collider** (BoxCollider, SphereCollider, MeshCollider, etc.)
3. Añade el componente **Outline**:
   - Add Component → Outline
   - El script helper te ayudará a configurarlo automáticamente
   - Por defecto:
     - Color: Blanco (255, 255, 255)
     - Width: 3
     - **IMPORTANTE:** Desactiva el componente Outline por defecto (desmarca el checkbox)
       - Se activará automáticamente cuando el jugador mire el objeto

#### Verificación Automática:
El script `InteractableObjectHelper` (en la carpeta Editor) te ayudará:
- Al seleccionar un objeto interactuable, verás información en el Inspector
- Te avisará si falta el Outline o Collider
- Botón para añadir Outline automáticamente con la configuración correcta

### 3. Configurar el Jugador

En el GameObject del jugador que tiene el `ControladorInteraccion`:

1. Asegúrate de que todos los campos estén asignados:
   - `Camara Jugador`
   - `Punto De Agarre`
   - `Animador Del Personaje`
   - `Script Movimiento`
   - Etc.

2. No necesitas configurar nada adicional para el sistema de UI, se conecta automáticamente.

## 🎨 Personalización del UI

### Cambiar la apariencia del Panel de Interacción:

1. Selecciona `PanelInteraccion` en la jerarquía
2. Modifica:
   - **Image**: Color de fondo, transparencia
   - **RectTransform**: Posición y tamaño
   - **Layout**: Padding, alineación

3. Selecciona `TextoInteraccion`
4. Modifica:
   - **Fuente**: Font, tamaño, estilo
   - **Color**: Color del texto
   - **Alineación**: Centro, izquierda, derecha
   - **Efectos**: Sombra, outline, etc.

### Cambiar el color del Outline:

Por defecto es blanco, pero puedes cambiarlo en [ControladorInteracción.cs](Assets/Scripts/ControladorInteracción.cs) línea ~134:

```csharp
outlineActual.OutlineColor = Color.white; // Cambia a Color.yellow, Color.cyan, etc.
outlineActual.OutlineWidth = 3f; // Cambia el grosor
```

## 🐛 Solución de Problemas

### El UI no aparece:
- Verifica que `UIInteractionManager` esté en el Canvas
- Verifica que el Panel y Texto estén asignados en el Inspector
- Asegúrate de que el Canvas esté activo en la escena

### El Outline no aparece:
- Verifica que el objeto tiene el componente `Outline`
- Asegúrate de que el Outline está **desactivado** por defecto (se activará automáticamente)
- Verifica que el objeto tiene un Collider
- Comprueba que la distancia de interacción es suficiente (campo `distanciaInteraccion` en ControladorInteraccion)

### Los objetos no se detectan:
- Verifica que tienen Collider
- Asegúrate de que no están en la capa "Ignore Raycast"
- Comprueba que el script de interacción apropiado está añadido
- Verifica que la cámara está asignada en `ControladorInteraccion`

### El mensaje de interacción es incorrecto:
- Revisa el método `ObtenerMensajeInteraccion()` en [ControladorInteracción.cs](Assets/Scripts/ControladorInteracción.cs)
- Personaliza los mensajes según tus necesidades

## 📝 Scripts Nuevos Creados

1. **UIInteractionManager.cs** - Gestiona el panel UI de interacciones
2. **Editor/InteractableObjectHelper.cs** - Helper para configurar objetos en el editor

## ✏️ Scripts Modificados

1. **ControladorInteracción.cs**:
   - Añadido sistema de mensajes de interacción
   - Configuración automática del Outline en blanco
   - Integración con UIInteractionManager

## 💡 Consejos

- Usa el sistema de capas para organizar los objetos interactuables
- El Outline solo se activa cuando miras el objeto, no siempre
- Puedes añadir sonidos cuando se muestra/oculta el UI
- Personaliza los mensajes en `ObtenerMensajeInteraccion()` según tu juego
- El ancho del outline (OutlineWidth) afecta la visibilidad en diferentes resoluciones

## 🚀 Extensiones Futuras

Puedes extender el sistema para:
- Añadir iconos en lugar de solo texto
- Animaciones del UI (fade in/out)
- Diferentes colores de outline según el tipo de interacción
- Tooltips con más información
- Soporte para gamepad (mostrar botones del mando)
