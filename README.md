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

---

## Compilación desde el código fuente

```bash
git clone https://github.com/pitu205/JellyInspector.git
cd JellyInspector
dotnet build -c Release
```
