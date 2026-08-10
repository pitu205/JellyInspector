# Instalación

## Desde el repositorio de plugins de Jellyfin

1. Abre **Panel de administración → Plugins → Repositorios**.
2. Añade el repositorio de JellyInspector:

```
https://raw.githubusercontent.com/pitu205/JellyInspector/main/manifest.json
```

3. Guarda los cambios.
4. reinicia Jellyfin,cierra del todo el servidor y ejecutalo de nuevo.
5. Ve a **Complementos**.
6. Instala **JellyInspector**.
7. Reinicia Jellyfin.

8. Abre el plugin y ve a ajustes,pon tu api de TmDb(crea una cuenta gratis y en tu nombre pulsa ajustes/api y abajo del todo está.

---
para desistalar el plugin ve a gestos de complementos enyta en 
jelly
inspector y da a desistalar, reinicia el servidor.

## Compilación desde el código fuente

```bash
git clone https://github.com/pitu205/JellyInspector.git
cd JellyInspector
dotnet build -c Release
```
