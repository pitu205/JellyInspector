# Instalación

## Desde el repositorio de plugins de Jellyfin

1. Abre **Panel de administración → Plugins → Repositorios**.
2. Añade el repositorio de JellyInspector:

```
https://TU_URL_DEL_MANIFEST/manifest.json
```

3. Guarda los cambios.
4. Ve a **Catálogo**.
5. Instala **JellyInspector**.
6. Reinicia Jellyfin.

7. Abre el plugin y ve a ajustes,pon tu api de TmDb(crea una cuenta gratis y en tu nombre pulsa ajustes/api y abajo del todo está.

---

## Compilación desde el código fuente

```bash
git clone https://github.com/pitu205/JellyInspector.git
cd JellyInspector
dotnet build -c Release
```
