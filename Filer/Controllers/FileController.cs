using Filer.Secvices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Filer.Controllers;

[ApiController]
[Route("[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileService fileService, ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    /// <summary>
    /// Téléverse un fichier vers SeaweedFS
    /// </summary>
    /// <param name="file">Le fichier provenant du formulaire</param>
    /// <param name="folder">Le dossier de destination (optionnel)</param>
    [HttpPost("upload")]
    [DisableRequestSizeLimit] // Utile pour les gros fichiers
    [Authorize]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string folder = "uploads")
    {
        if (file == null || file.Length == 0)
            return BadRequest("Aucun fichier n'a été fourni.");

        try
        {
            using var stream = file.OpenReadStream();
            var fileUrl = await _fileService.UploadFileAsync(stream, file.FileName, folder);

            return Ok(new
            {
                Message = "Fichier uploadé avec succès",
                Url = fileUrl,
                FileName = file.FileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'upload du fichier {FileName}", file.FileName);
            return StatusCode(500, "Une erreur interne est survenue.");
        }
    }

    /// <summary>
    /// Télécharge un fichier depuis SeaweedFS
    /// </summary>
    /// <param name="path">Chemin complet du fichier (ex: uploads/mon-image.png)</param>
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string path)
    {
        if (string.IsNullOrEmpty(path))
            return BadRequest("Le chemin du fichier est requis.");

        try
        {
            var (content, contentType) = await _fileService.DownloadFileAsync(path);

            // On retourne le flux directement au navigateur/client Angular
            return File(content, contentType, Path.GetFileName(path));
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound("Le fichier n'existe pas sur le serveur de stockage.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du téléchargement de {Path}", path);
            return StatusCode(500, "Erreur lors de la récupération du fichier.");
        }
    }

    /// <summary>
    /// Supprime un fichier
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string path)
    {
        if (string.IsNullOrEmpty(path))
            return BadRequest("Le chemin du fichier est requis.");

        var success = await _fileService.DeleteFileAsync(path);

        if (success)
            return Ok(new { Message = "Fichier supprimé" });

        return NotFound("Le fichier n'a pas pu être trouvé ou supprimé.");
    }
}
