# 🎬 Cinema Simulator - Sistema de Interacciones

## ✨ Características Implementadas

### 1. UI de Interacciones
- Panel que aparece automáticamente al mirar objetos interactuables
- Texto descriptivo de la acción posible
- Se oculta cuando dejas de mirar el objeto

### 2. Feedback Visual (Outline Blanco)
- Contorno blanco alrededor de los objetos al mirarlos
- Grosor personalizable (por defecto: 3)
- Color personalizable (por defecto: blanco)

### 3. Sistema de Interacciones Completo

#### 🍿 Máquinas de Palomitas
| Situación | Acción | Resultado |
|-----------|--------|-----------|
| Con cubo vacío | Click izquierdo | Llenar cubo de palomitas |
| Con caja de palomitas | Click izquierdo | Reponer 1 unidad |
| Con caja de palomitas | Click derecho | Reponer todo |
| Con caja de envases (cubos) | Click izquierdo | Reponer 1 envase |
| Con caja de envases (cubos) | Click derecho | Reponer todos los envases |

#### 🥤 Máquinas de Bebidas
| Situación | Acción | Resultado |
|-----------|--------|-----------|
| Con vaso vacío | Click izquierdo | Llenar vaso de bebida |
| Con caja de bebidas | Click izquierdo | Reponer 1 unidad |
| Con caja de bebidas | Click derecho | Reponer todo |
| Con caja de envases (vasos) | Click izquierdo | Reponer 1 envase |
| Con caja de envases (vasos) | Click derecho | Reponer todos los envases |

#### 🌭 Máquina de Perritos
| Situación | Acción | Resultado |
|-----------|--------|-----------|
| Manos vacías | Click izquierdo | Coger un perrito caliente |
| Con caja de perritos | Click izquierdo | Reponer 1 perrito |
| Con caja de perritos | Click derecho | Reponer todos |

#### 📦 Máquinas de Envases
| Situación | Acción | Resultado |
|-----------|--------|-----------|
| Manos vacías | Click izquierdo | Coger un envase vacío |
| Con caja de envases | Click izquierdo | Reponer 1 envase |
| Con caja de envases | Click derecho | Reponer todos |

#### 🔔 Campana (Bell)
| Situación | Acción | Resultado |
|-----------|--------|-----------|
| Cualquiera | Click izquierdo | Llamar a un cliente / Pedir comanda |

#### 🗑️ Papelera
| Situación | Acción | Resultado |
|-----------|--------|-----------|
| Con cualquier objeto | Click izquierdo | Tirar objeto a la basura |

#### 🔧 Máquinas Rotas
| Situación | Acción | Resultado |
|-----------|--------|-----------|
| Con martillo | Mantener click izquierdo | Reparar máquina (barra de progreso) |

#### 📦 Objetos en el Suelo
| Situación | Acción | Resultado |
|-----------|--------|-----------|
| Manos vacías | Click izquierdo | Recoger objeto |

#### 👥 Clientes
| Situación | Acción | Resultado |
|-----------|--------|-----------|
| Con pedido correcto | Click izquierdo | Entregar pedido al cliente |

## 📂 Archivos Creados/Modificados

### Nuevos Archivos:
```
Assets/Scripts/
├── UIInteractionManager.cs                    [NUEVO]
└── Editor/
    ├── InteractableObjectHelper.cs            [NUEVO]
    └── InteractableSetupTool.cs               [NUEVO]

Documentación/
├── SISTEMA_INTERACCIONES.md                   [NUEVO]
└── GUIA_RAPIDA_INTERACCIONES.md               [NUEVO]
```

### Archivos Modificados:
```
Assets/Scripts/
└── ControladorInteracción.cs                  [MODIFICADO]
    ├── + Método: ObtenerMensajeInteraccion()
    ├── + Variable: mensajeInteraccion
    ├── + Integración con UIInteractionManager
    └── + Configuración automática del Outline
```

## 🎮 Cómo Usar (Para Desarrolladores)

### Configuración Inicial (Una sola vez)

1. **Configurar el Canvas UI:**
   ```
   1. Abre tu escena principal
   2. Busca o crea Canvas "Interfaz"
   3. Añade componente: UIInteractionManager
   4. Crea estructura:
      Canvas > PanelInteraccion > TextoInteraccion (TMP)
   5. Asigna referencias en UIInteractionManager
   ```

2. **Configurar Objetos Interactuables (Automático):**
   ```
   1. En Unity: Menú > Herramientas > Configurar Objetos Interactuables
   2. Click en "Buscar y Configurar Todos los Objetos"
   3. ¡Listo! Todos tus objetos están configurados
   ```

   **O manualmente:**
   ```
   Para cada objeto interactuable:
   1. Añadir componente: Outline
   2. Configurar:
      - Color: Blanco (255, 255, 255)
      - Width: 3
      - Desactivar checkbox (enabled = false)
   ```

### Testing

1. Entra en Play Mode
2. Mueve el jugador cerca de objetos interactuables
3. Mira los objetos con el centro de la cámara
4. Verifica:
   - ✅ Aparece el outline blanco
   - ✅ Aparece el panel UI con el mensaje
   - ✅ El mensaje describe correctamente la acción
   - ✅ Al dejar de mirar, todo desaparece

## 🔧 Personalización Avanzada

### Cambiar Mensajes de Interacción

Edita el método `ObtenerMensajeInteraccion()` en `ControladorInteracción.cs`:

```csharp
// Ejemplo: Cambiar mensaje de máquina de palomitas
// Línea ~246
if (data.tipoDeItem == ItemData.TipoDeItem.CuboVacio)
    return "Click izquierdo - Llenar cubo";

// Cámbialo por:
if (data.tipoDeItem == ItemData.TipoDeItem.CuboVacio)
    return "[E] Servir palomitas 🍿";
```

### Cambiar Color del Outline

En `ControladorInteracción.cs`, línea ~134:

```csharp
outlineActual.OutlineColor = Color.white;

// Opciones:
outlineActual.OutlineColor = Color.yellow;        // Amarillo
outlineActual.OutlineColor = Color.cyan;          // Cian
outlineActual.OutlineColor = new Color(1, 0.5f, 0); // Naranja
```

### Cambiar Grosor del Outline

```csharp
outlineActual.OutlineWidth = 3f;

// Prueba otros valores:
outlineActual.OutlineWidth = 5f;  // Más grueso
outlineActual.OutlineWidth = 2f;  // Más fino
```

### Añadir Sonidos

Modifica `UIInteractionManager.cs`:

```csharp
[Header("Audio")]
public AudioSource audioSource;
public AudioClip sonidoMostrar;
public AudioClip sonidoOcultar;

public void MostrarInteraccion(string mensaje)
{
    if (panelInteraccion != null)
        panelInteraccion.SetActive(true);
    
    if (textoInteraccion != null)
        textoInteraccion.text = mensaje;
    
    // Añadir sonido
    if (audioSource != null && sonidoMostrar != null)
        audioSource.PlayOneShot(sonidoMostrar);
}
```

### Animaciones del Panel

Usa el Animator:

```csharp
[Header("Animación")]
public Animator panelAnimator;

public void MostrarInteraccion(string mensaje)
{
    if (panelInteraccion != null)
        panelInteraccion.SetActive(true);
    
    if (textoInteraccion != null)
        textoInteraccion.text = mensaje;
    
    // Trigger de animación
    if (panelAnimator != null)
        panelAnimator.SetTrigger("Show");
}
```

## 📊 Diagrama de Flujo del Sistema

```
Jugador mira con la cámara
           ↓
    Raycast detecta objeto
           ↓
    ¿Tiene script interactuable?
           ↓ Sí
    ¿Puede interactuar ahora?
           ↓ Sí
    ┌──────────────────────┐
    │  Activar Outline     │
    │  Configurar color    │
    │  (blanco, width 3)   │
    └──────────────────────┘
           ↓
    ┌──────────────────────┐
    │  Obtener mensaje     │
    │  según tipo objeto   │
    │  y contexto          │
    └──────────────────────┘
           ↓
    ┌──────────────────────┐
    │  Mostrar UI          │
    │  con mensaje         │
    └──────────────────────┘
           ↓
    Jugador hace click
           ↓
    Ejecutar interacción
```

## 🎯 Tipos de Objetos Soportados

| Tipo | Script Requerido | Collider | Outline |
|------|-----------------|----------|---------|
| Máquina Palomitas | `MaquinaDePalomitas` | ✅ | ✅ |
| Máquina Bebidas | `MaquinaDeBebidas` | ✅ | ✅ |
| Máquina Perritos | `MaquinaDePerritos` | ✅ | ✅ |
| Máquina Envases | `MaquinaDeItems` | ✅ | ✅ |
| Papelera | `Papelera` | ✅ | ✅ |
| Campana | `CampanaInteractiva` | ✅ | ✅ |
| Cliente | `GestorPedidos` | ✅ | ✅ |
| Item Suelo | `ItemData` | ✅ | ✅ |
| Tablet | `TabletManager` | ✅ | ✅ |

## 💡 Tips y Trucos

1. **Distancia de Interacción:**
   - Por defecto: 3 metros
   - Ajusta en `ControladorInteraccion` → `distanciaInteraccion`

2. **UI Responsivo:**
   - Usa anchors para diferentes resoluciones
   - Prueba en varias resoluciones

3. **Optimización:**
   - El Outline solo se activa cuando es necesario
   - No hay cálculos innecesarios cuando no miras nada

4. **Debugging:**
   - Activa Gizmos para ver el raycast
   - Usa Debug.Log en `ObtenerMensajeInteraccion()` si algo falla

5. **Extensibilidad:**
   - Fácil añadir nuevos tipos de objetos
   - Solo modifica `PuedeInteractuar()` y `ObtenerMensajeInteraccion()`

## ⚠️ Importante

- El Outline debe estar **DESACTIVADO** en el Inspector por defecto
- Se activará automáticamente al detectar el objeto
- Cada objeto necesita **Collider** para ser detectado
- El Canvas debe ser **Screen Space - Overlay** para mayor compatibilidad

## 🚀 Próximos Pasos

Ahora puedes:
- [ ] Personalizar los mensajes según tu diseño
- [ ] Ajustar el estilo visual del UI
- [ ] Añadir sonidos de feedback
- [ ] Crear animaciones del panel
- [ ] Añadir iconos junto al texto
- [ ] Implementar soporte para gamepad

¡El sistema está listo para usar! 🎉
