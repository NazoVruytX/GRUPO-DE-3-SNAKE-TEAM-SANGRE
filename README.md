# GRUPO-DE-3-SNAKE-TEAM-SANGRE 🐍

Proyecto académico grupal para el desarrollo del clásico videojuego **Snake** utilizando el motor gráfico **Unity (C#)**.

## 👥 Integrantes del Equipo (Team Sangre)

| Integrante | Aportes principales |
|---|---|
| Carlos Daniel Valverde Mendoza | Sprites pixel art · Contador de puntos, récord y cuenta regresiva · Fin de juego y pausa |
| Jhoel Eduardo Condoreno Chura | Tablero y movimiento de la serpiente · Efectos de sonido y música · Ejecutable y documentación |
| Alvaro Luis Carlos Del Carpio Blanco | Configuración del proyecto · Comida, crecimiento y colisiones · Menú principal y escenas |

## 🎮 Descripción del Proyecto

Este repositorio contiene el código fuente, assets y configuraciones para nuestro juego de Snake en 2D. El proyecto ha sido estructurado para cumplir con todos los requerimientos del trabajo en aula (presencial).

La serpiente avanza por un tablero de 26×14 casillas. Cada manzana suma **10 puntos**, hace crecer a la serpiente y la acelera un poco. La partida termina al chocar con una pared o con el propio cuerpo (o con victoria, si la serpiente llena todo el tablero).

## ✅ Características y Requisitos Implementados

Según lo establecido para grupos de 3 personas, el juego incluye:

- **Mecánicas Clásicas:** Movimiento continuo y crecimiento de la serpiente al comer.
- **Sistema de Audio:** Implementación de sonidos para recolección de puntos y Game Over.
- **Físicas:** Detección de colisiones (con los bordes y con el propio cuerpo de la serpiente).
- **UI / Interfaz:** Contador de puntuación en pantalla y lógica de finalización de partida.
- **Gestión de Escenas:** El juego cuenta con al menos 2 escenas funcionales:
  - Menú Principal (Inicio).
  - Escena de Juego (Gameplay).
- **Entrega Final:** Repositorio en GitHub y ejecutable funcional listo para su rápida exposición.

### ¿Cómo se implementó cada requisito?

| Requisito | Implementación |
|---|---|
| Sonidos | `AudioManager` (persistente entre escenas) con música en bucle y efectos al comer, chocar, fin de juego, cuenta regresiva y clic de botones. Tecla **M** para silenciar. |
| Colisiones | Física 2D de Unity: la cabeza tiene `Rigidbody2D` + `BoxCollider2D` (trigger) y usa `OnTriggerEnter2D`. Paredes y cuerpo llevan el tag `Obstacle`; la comida tiene el componente `Food`. |
| Contador | HUD con **puntos**, **récord** guardado con `PlayerPrefs` y cuenta regresiva **3, 2, 1, ¡YA!** al iniciar. |
| Finalización | Pantalla **¡FIN DEL JUEGO!** (o **¡GANASTE!**) con puntaje, récord, aviso de **nuevo récord** y botones Reintentar / Menú. También hay menú de **pausa**. |
| 2 escenas | `MainMenu` (Jugar, Sonido, Salir) y `Game`, con transición de fundido entre ellas. |
| Ejecutable | `Ejecutable/Snake.exe` incluido en el repositorio, generado desde el menú **Snake → Compilar para Windows**. |

## 🕹️ Controles

| Acción | Tecla |
|---|---|
| Mover | Flechas o **W A S D** (también cruceta de mando) |
| Pausa / continuar | **P** o **Esc** |
| Silenciar sonido | **M** |
| Reintentar (al perder) | **Enter** |
| Volver al menú (al perder) | **Esc** |
| Navegar menús | Mouse, o flechas + **Enter** |

## ▶️ Cómo jugar (ejecutable)

El juego compilado para Windows (64 bits) está en la carpeta [`Ejecutable/`](Ejecutable) del repositorio.

1. Clonar el repositorio, o descargarlo con **Code → Download ZIP** y extraerlo.
2. Abrir la carpeta `Ejecutable` y ejecutar **`Snake.exe`**.

> `Snake.exe` necesita los demás archivos de la carpeta (`UnityPlayer.dll`, `Snake_Data`, etc.): si se copia a otro lugar, hay que copiar la carpeta `Ejecutable` completa.
> Si Windows muestra el aviso de SmartScreen (el ejecutable no está firmado), elegir **Más información → Ejecutar de todas formas**.
> Con **Alt + Enter** se cambia entre ventana y pantalla completa.

## 🛠️ Tecnologías

- Unity **6000.3.16f1** (Unity 6.3) — Built-in Render Pipeline, 2D
- C#
- Input System (paquete `com.unity.inputsystem`)
- TextMeshPro (incluido en `com.unity.ugui`)
- Python 3 (solo para generar los sprites y audios, ver *Origen de los assets*)

## 📂 Cómo abrir el proyecto

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/NazoVruytX/GRUPO-DE-3-SNAKE-TEAM-SANGRE.git
   ```
2. En **Unity Hub** → **Add** → **Add project from disk** → seleccionar la carpeta clonada.
3. Abrir con la versión **6000.3.16f1**.
4. Abrir la escena `Assets/Scenes/MainMenu.unity` y presionar **Play**.

## 🏗️ Cómo compilar el ejecutable

En Unity: menú **Snake → Compilar para Windows**. El juego se genera (o se actualiza) en `Ejecutable/Snake.exe`.

## 🗂️ Estructura del proyecto

```
Assets/
├── Audio/            Música y efectos (.wav)
├── Editor/
│   └── BuildScript.cs        Compilación para Windows (menú Snake)
├── Materials/        Material de las partículas
├── Prefabs/          SnakeBody (segmento del cuerpo) y AudioManager
├── Scenes/           MainMenu.unity y Game.unity
├── Scripts/
│   ├── Audio/
│   │   └── AudioManager.cs   Música y efectos; singleton que sobrevive entre escenas
│   ├── Core/
│   │   ├── GameManager.cs    Flujo de la partida: cuenta regresiva, juego, pausa y fin
│   │   ├── HighScore.cs      Récord guardado con PlayerPrefs
│   │   └── SceneLoader.cs    Cambio de escena con fundido
│   ├── Gameplay/
│   │   ├── CameraFit.cs      Ajusta la cámara al tablero en cualquier resolución
│   │   ├── Food.cs           Comida: aparece en una celda libre al azar
│   │   ├── GameGrid.cs       Tablero en cuadrícula, piso y paredes
│   │   └── SnakeController.cs  Movimiento, crecimiento y colisiones de la serpiente
│   └── UI/
│       ├── ButtonFeedback.cs   Animación y sonido de los botones
│       ├── GameHUD.cs          Puntos, récord y cuenta regresiva
│       ├── GameOverScreen.cs   Pantalla de fin de juego / victoria
│       ├── MainMenu.cs         Botones del menú principal
│       ├── PauseScreen.cs      Menú de pausa
│       └── UIFloat.cs          Efecto de flotación en la UI
├── Sprites/          Sprites pixel art (.png)
└── TextMesh Pro/     Recursos de TextMeshPro (fuente y shaders)
Ejecutable/           Juego compilado para Windows (Snake.exe y sus archivos)
Tools/
├── generar_sprites.py  Genera los sprites de Assets/Sprites
└── generar_audio.py    Genera la música y los efectos de Assets/Audio
```

## 🎨 Origen de los assets

Todos los recursos gráficos y de audio del juego son **originales** y se generaron por código; **no se usaron imágenes ni sonidos de terceros**.

| Recurso | Origen | Licencia |
|---|---|---|
| Sprites (serpiente, manzana, tablero, paredes, botones, ícono) | Pixel art de 16×16 px dibujado por código con [`Tools/generar_sprites.py`](Tools/generar_sprites.py) (Python, solo librería estándar). | Propia del proyecto |
| Música y efectos de sonido | Sonidos estilo *chiptune* sintetizados con [`Tools/generar_audio.py`](Tools/generar_audio.py) a partir de ondas cuadradas, triangulares, senoidales y ruido. La música es un bucle de 8 compases en La menor a 128 BPM. | Propia del proyecto |
| Fuente *Liberation Sans* | Incluida en los recursos esenciales de **TextMeshPro** (Unity). | SIL Open Font License 1.1 |
| Paquetes Input System y TextMeshPro | Paquetes oficiales de **Unity Technologies**. | Unity Companion License |

Para volver a generar los assets (desde la raíz del repositorio):

```bash
python Tools/generar_sprites.py
python Tools/generar_audio.py
```

Los scripts siempre producen exactamente los mismos archivos.

## 🚦 Estado

✅ Terminado.
