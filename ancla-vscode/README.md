# Ancla – Port Manager

> **[English](#english) | [Español](#español)**

---

## English

<a name="english"></a>

**Ancla** is a VS Code extension that lets you view and manage active network ports directly from the editor — no terminal needed.

### Features

- **Live port list** — see every port currently in use on your machine, with process name and PID.
- **Framework detection** — automatically tags ports belonging to React, Vue, Angular, ASP.NET, Docker, Storybook, and more.
- **Smart filters** — quickly switch between *All ports*, *Dev ports only*, or a specific framework.
- **Kill process** — terminate the process holding a port with a single click.
- **Protection layer** — critical system processes are flagged and protected so you don't kill something important by accident.
- **Sidebar panel** — always accessible from the Activity Bar via the anchor icon.
- **Keyboard shortcut** — `Ctrl+Alt+P` / `Cmd+Alt+P` to open the panel instantly.

### Requirements

- VS Code `1.85.0` or later
- Windows (uses `netstat` and PowerShell internally)

### Usage

1. Click the **anchor icon** in the Activity Bar to open the port manager sidebar.  
   Or press `Ctrl+Alt+P` (`Cmd+Alt+P` on macOS).
2. Use the filter buttons at the top to narrow down results.
3. Click **Kill** next to any port to stop its process.

### Why "Ancla"?

*Ancla* is Spanish for *anchor* — something that holds things in place. The extension keeps your dev port chaos anchored and under control.

---

## Español

<a name="español"></a>

**Ancla** es una extensión de VS Code que te permite ver y gestionar los puertos de red activos directamente desde el editor — sin necesidad de abrir la terminal.

### Características

- **Lista de puertos en tiempo real** — ve todos los puertos en uso en tu máquina, con el nombre del proceso y el PID.
- **Detección de frameworks** — identifica automáticamente puertos de React, Vue, Angular, ASP.NET, Docker, Storybook y más.
- **Filtros inteligentes** — cambia rápidamente entre *Todos los puertos*, *Solo puertos dev* o un framework específico.
- **Matar proceso** — termina el proceso que ocupa un puerto con un solo clic.
- **Capa de protección** — los procesos críticos del sistema están marcados y protegidos para que no los cierres por accidente.
- **Panel lateral** — siempre accesible desde la Barra de Actividades a través del ícono de ancla.
- **Atajo de teclado** — `Ctrl+Alt+P` / `Cmd+Alt+P` para abrir el panel al instante.

### Requisitos

- VS Code `1.85.0` o superior
- Windows (usa `netstat` y PowerShell internamente)

### Uso

1. Haz clic en el **ícono de ancla** en la Barra de Actividades para abrir el panel lateral.  
   O presiona `Ctrl+Alt+P` (`Cmd+Alt+P` en macOS).
2. Usa los botones de filtro en la parte superior para reducir los resultados.
3. Haz clic en **Kill** junto a cualquier puerto para detener su proceso.

### ¿Por qué "Ancla"?

*Ancla* es la traducción de *anchor* en inglés — algo que mantiene las cosas en su lugar. La extensión mantiene el caos de tus puertos dev anclado y bajo control.

---

## License

MIT
