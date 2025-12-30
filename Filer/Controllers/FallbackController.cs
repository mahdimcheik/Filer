using Filer.Secvices;
using Microsoft.AspNetCore.Mvc;

namespace Filer.Controllers;

/// <summary>
/// Controller dédié pour gérer les requêtes de fichiers non gérées par les autres routes
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)] // Cache tout le contrôleur de Swagger
public class FallbackController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FallbackController> _logger;

    public FallbackController(IFileService fileService, ILogger<FallbackController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Fallback handler pour toutes les URLs non correspondantes
    /// Exemple: /user/123/avatar.jpg
    /// </summary>
    [HttpGet("/{**path}")]
    [Route("/{**path}")]
    public async Task<IActionResult> HandleFallback(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest("Le chemin du fichier est requis.");

            // Exclure les chemins Swagger et API
            var excludedPrefixes = new[]
            {
                "swagger",
                "swagger-ui",
                "files/",
                "auth/",
                "index"
            };


            if (excludedPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound();
            }

            var (content, contentType) = await _fileService.DownloadFileAsync(path);

            return File(content, contentType, Path.GetFileName(path));
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound("Le fichier n'existe pas sur le serveur de stockage.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du fichier: {Path}", path);
            return StatusCode(500, "Erreur lors de la récupération du fichier.");
        }
    }
}
