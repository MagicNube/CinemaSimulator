# 🎨 Configuración del Canvas - Paso a Paso

## Paso 1: Crear/Encontrar el Canvas "Interfaz"

### Si ya existe:
1. En la jerarquía, busca el Canvas llamado "Interfaz"
2. Selecciónalo

### Si no existe:
1. Click derecho en la Jerarquía
2. UI → Canvas
3. Renómbralo a "Interfaz"

## Paso 2: Configurar el Canvas

### Componentes necesarios:
- `Canvas` (ya existe)
- `Canvas Scaler` (ya existe)
- `Graphic Raycaster` (ya existe)
- `UIInteractionManager` (AÑADIR)

### Añadir UIInteractionManager:
1. Con el Canvas seleccionado
2. En el Inspector → Add Component
3. Buscar: "UIInteractionManager"
4. Click para añadir

**Todavía NO asignes nada en el componente, primero creamos los elementos UI**

## Paso 3: Crear el Panel de Interacción

1. Click derecho en el Canvas "Interfaz"
2. UI → Panel
3. Renómbralo a: **PanelInteraccion**

### Configurar el Panel:

#### Rect Transform:
```
Anchor Preset: Bottom - Center (shift + alt + click)
Pivot: X: 0.5, Y: 0.5

Position:
- Pos X: 0
- Pos Y: 100  (ajusta según prefieras)

Size:
- Width: 400
- Height: 80
```

#### Image Component:
```
Color: Negro (R:0, G:0, B:0, A:180)
      o el color que prefieras con algo de transparencia

Material: None
Raycast Target: ✅ (activado)
```

#### Opcional - Añadir borde:
1. Add Component → Outline
2. Effect Color: Blanco o Color de acento
3. Effect Distance: X:2, Y:-2

## Paso 4: Crear el Texto de Interacción

1. Click derecho en **PanelInteraccion**
2. UI → Text - TextMeshPro
3. Si aparece un diálogo de importar TMP essentials → Click "Import TMP Essentials"
4. Renombra el texto a: **TextoInteraccion**

### Configurar el Texto:

#### Rect Transform:
```
Anchor Preset: Stretch - Stretch (shift + alt + click)

Esto hará que el texto llene todo el panel automáticamente.

Left: 10
Top: 10
Right: 10
Bottom: 10
(Esto da un poco de padding)
```

#### TextMeshPro Component:
```
Text: "Click izquierdo - Interactuar"  (texto de ejemplo)

Font Asset: [Elige tu fuente favorita]
Font Style: Normal o Bold (a tu gusto)

Font Size: 24
           (o Auto Size: Min:18, Max:30)

Alignment: Centro (horizontalmente y verticalmente)
           ┌─┬─┬─┐
           │ │ │ │
           ├─┼─┼─┤
           │ │●│ │  ← Click este
           ├─┼─┼─┤
           │ │ │ │
           └─┴─┴─┘

Color: Blanco (R:255, G:255, B:255, A:255)

Wrapping: Enabled
Overflow: Truncate
```

#### Opcional - Añadir sombra:
1. En TextMeshPro → Scroll down
2. Extra Settings → Enable
3. Shadow:
   - Color: Negro semi-transparente
   - Offset: X:2, Y:-2
   - Dilate: 0.5

#### Opcional - Outline en texto:
1. Material Preset → Outline
2. Outline Color: Negro
3. Outline Thickness: 0.2

## Paso 5: Conectar Referencias

1. Selecciona el Canvas "Interfaz"
2. En el Inspector, encuentra el componente `UIInteractionManager`
3. Arrastra los objetos:

```
UIInteractionManager:
├─ Panel Interaccion: [Arrastra PanelInteraccion aquí]
└─ Texto Interaccion: [Arrastra TextoInteraccion aquí]
```

**Cómo arrastrar:**
- Click en PanelInteraccion en la Jerarquía
- Sin soltar, arrastra al campo "Panel Interaccion" del Inspector
- Suelta
- Repite con TextoInteraccion

## Paso 6: Verificación

### Checklist:
- [ ] Canvas "Interfaz" existe
- [ ] Canvas tiene componente `UIInteractionManager`
- [ ] PanelInteraccion es hijo del Canvas
- [ ] TextoInteraccion es hijo de PanelInteraccion
- [ ] Referencias están asignadas en UIInteractionManager
- [ ] Panel está posicionado en la parte inferior de la pantalla

### Jerarquía Final:
```
Canvas (Interfaz)
├─ UIInteractionManager (Component)
└─ PanelInteraccion (Panel UI)
    └─ TextoInteraccion (TextMeshPro)
```

## Paso 7: Probar en el Editor

1. **Sin entrar en Play Mode:**
   - El Panel debe estar VISIBLE en la Scene view
   - Puedes ver el texto de ejemplo

2. **Entrar en Play Mode:**
   - El Panel se ocultará automáticamente al inicio
   - Aparecerá cuando mires un objeto interactuable

3. **Si no aparece:**
   - Verifica que hay objetos con Outline configurado
   - Verifica que estás lo suficientemente cerca (distancia por defecto: 3 metros)
   - Mira el objeto con el centro de la cámara (el crosshair/puntero)

## 🎨 Ejemplos de Diseño

### Diseño 1: Moderno y Elegante
```
Panel:
- Color: #1a1a1a (gris muy oscuro) con Alpha 220
- Borde: Outline blanco de 2px
- Esquinas redondeadas (si usas Image → Image Type: Sliced)

Texto:
- Fuente: Roboto Bold o Montserrat Bold
- Tamaño: 22
- Color: #ffffff (blanco puro)
- Sombra: negra, offset (2, -2)
```

### Diseño 2: Colorido y Vibrante
```
Panel:
- Color: #2196F3 (azul material) con Alpha 200
- Sin borde

Texto:
- Fuente: Arial Bold
- Tamaño: 24
- Color: #FFEB3B (amarillo)
- Outline del texto: negro, thickness 0.3
```

### Diseño 3: Minimalista
```
Panel:
- Color: #ffffff (blanco) con Alpha 50 (muy transparente)
- Borde: Outline gris oscuro de 1px

Texto:
- Fuente: Helvetica Regular
- Tamaño: 20
- Color: #212121 (casi negro)
- Sin efectos adicionales
```

## 📐 Posiciones Alternativas

### Parte Superior Centro:
```
Anchor: Top - Center
Pos Y: -100 (negativo para bajar desde arriba)
```

### Esquina Inferior Derecha:
```
Anchor: Bottom - Right
Pos X: -220 (negativo para mover a la izquierda)
Pos Y: 50
```

### Centro de la Pantalla:
```
Anchor: Middle - Center
Pos X: 0
Pos Y: -100 (un poco debajo del centro)
```

## 🔍 Troubleshooting

### "No veo el Panel en Play Mode"
- **Correcto:** El panel se oculta al inicio
- Debe aparecer solo al mirar objetos interactuables

### "El Panel no se mueve con diferentes resoluciones"
- Verifica que usaste los **Anchor Presets** correctamente
- Usa Shift + Alt + Click para fijar tanto anchor como pivot

### "El texto se sale del Panel"
- Activa **Wrapping** en TextMeshPro
- Reduce el tamaño del texto
- O aumenta el tamaño del Panel

### "Las referencias están asignadas pero no funciona"
- Verifica que UIInteractionManager está en el CANVAS, no en el Panel
- Comprueba que los objetos interactuables tienen Outline

## ✅ Resultado Final

Al terminar debes tener:
- ✨ Un panel elegante en la parte inferior de la pantalla
- 📝 Texto claro y legible
- 🔗 Referencias correctamente conectadas
- 🎮 Sistema listo para mostrar interacciones

## 🚀 Siguiente Paso

Ahora configura los objetos interactuables:
1. Abre el menú: **Herramientas → Configurar Objetos Interactuables**
2. Click en **"Buscar y Configurar Todos los Objetos"**
3. ¡Listo!

O consulta [GUIA_RAPIDA_INTERACCIONES.md](GUIA_RAPIDA_INTERACCIONES.md) para configuración manual.
