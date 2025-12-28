using Filer.Secvices;
using Filer.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;

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
    [Authorize]
    [DisableRequestSizeLimit] // Utile pour les gros fichiers
    //[Authorize]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string folder = "uploads")
    {
        if (file == null || file.Length == 0)
            return BadRequest("Aucun fichier n'a été fourni.");

        try
        {
            var serviceName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? folder;
            using var stream = file.OpenReadStream();
            var fileUrl = await _fileService.UploadFileAsync(stream, file.FileName, serviceName);

            return Ok(
                new
                {
                    Message = "Fichier uploadé avec succès",
                    Url = fileUrl,
                    FileName = file.FileName,
                }
            );
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
    [Authorize]
    public async Task<IActionResult> Download([FromQuery] string path)
    {
        if (string.IsNullOrEmpty(path))
            return BadRequest("Le chemin du fichier est requis.");

        try
        {
            var serviceName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            path = $"{serviceName}/{path}";
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
    /// Retrieves metadata information about a file located at the specified path from the storage server.
    /// </summary>
    /// <remarks>This method is intended for use in HTTP GET requests to obtain file details from the storage
    /// backend. The response includes appropriate HTTP status codes for error scenarios, such as missing files or
    /// invalid input.</remarks>
    /// <param name="path">The relative or absolute path to the file whose information is to be retrieved. Cannot be null or empty.</param>
    /// <returns>An <see cref="IActionResult"/> containing the file metadata if found; returns a 400 Bad Request if <paramref
    /// name="path"/> is null or empty, a 404 Not Found if the file does not exist, or a 500 Internal Server Error if an
    /// unexpected error occurs.</returns>
    [HttpGet("info")]
    [Authorize]
    public async Task<IActionResult> GetFileInfo([FromQuery] string path)
    {
        if (string.IsNullOrEmpty(path))
            return BadRequest("Le chemin du fichier est requis.");
        try
        {
            var serviceName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            path = $"{serviceName}/{path}";
            var fileInfo = await _fileService.GetFileDataAsync(path);
            return Ok(fileInfo);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound("Le fichier n'existe pas sur le serveur de stockage.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des informations de {Path}", path);
            return StatusCode(500, "Erreur lors de la récupération des informations du fichier.");
        }
    }

    /// <summary>
    /// Supprime un fichier
    /// </summary>
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete([FromQuery] string path)
    {
        if (string.IsNullOrEmpty(path))
            return BadRequest("Le chemin du fichier est requis.");

        var serviceName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        path = $"{serviceName}/{path}";
        var success = await _fileService.DeleteFileAsync(path);

        if (success)
            return Ok(new { Message = "Fichier supprimé" });

        return NotFound("Le fichier n'a pas pu être trouvé ou supprimé.");
    }

    /// <summary>
    /// Fallback handler for all unmatched URLs
    /// Handles requests like: /user/123/avatar.jpg
    /// </summary>
    /// <param name="path">The full path to the file (captured by catch-all route)</param>
    [HttpGet("/{**path}")]
    [ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger
    public async Task<IActionResult> HandleFallback(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest("Le chemin du fichier est requis.");

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
