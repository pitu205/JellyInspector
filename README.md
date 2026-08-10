# Instalación

## Desde el repositorio de plugins de Jellyfin

1. Abre **Panel de administración → Plugins → Repositorios**.
2. Añade el repositorio de JellyInspector:
3. https://raw.githubusercontent.com/pitu205/JellyInspector/main/manifest.json
4. Guarda los cambios.
5. reinicia Jellyfin,cierra del todo el servidor y ejecutalo de nuevo.
6. Ve a **Complementos**.
7. Instala **JellyInspector**.
8. Reinicia Jellyfin.
 -----
8. para desistalar el plugin ve a gestor de complementos entra en jellyinspector y da a desistalar, reinicia el servidor.
-------
9. JellyInspector necesita una API Key de TMDb para que salgan las caratulas,Se utiliza para consultar series, temporadas, episodios y datos de emisión.
Instrucciones:
Crea una cuenta en TMDb. https://www.themoviedb.org/u/JellyInspector
Ve a Perfil / Ajustes → API.
Solicita una API Developer haciendo clic en el botón Solicitar API.
Copia la API Key v3.
Pégala en JellyInspector y pulsa Probar conexión.

---

## Compilación desde el código fuente

```bash
git clone https://github.com/pitu205/JellyInspector.git
cd JellyInspector
dotnet build -c Release
```
