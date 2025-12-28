using Filer.Models;
using Filer.Utilities;
using System.Net.Http.Headers;

namespace Filer.Secvices;

public interface IFileService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath);
    Task<(Stream Content, string ContentType)> DownloadFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<FileUrl> GetFileDataAsync(string filePath);
    Task<(Stream Content, string ContentType, string FileName)> DownloadFileFromUrlAsync(string fileUrl);
}

public class SeaweedService : IFileService
{
    private readonly HttpClient _httpClient;
    private readonly string _filerUrl;

    public SeaweedService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _filerUrl =  EnvironmentVariables.FilerUrl ?? throw new ArgumentNullException("FilerUrl is missing");
    }

    // ÉCRITURE : Envoie un fichier vers le Filer
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath)
    {
        var requestUrl = $"{_filerUrl}/{folderPath.Trim('/')}/{fileName}";

        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync(requestUrl, content);
        response.EnsureSuccessStatusCode();

        return requestUrl; // Retourne le chemin d'accès au fichier
    }

    // LECTURE : Récupère le flux du fichier
    public async Task<(Stream Content, string ContentType)> DownloadFileAsync(string filePath)
    {
        var requestUrl = $"{_filerUrl}/{filePath.TrimStart('/')}";
        var response = await _httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        return (stream, contentType);
    }

    // SUPPRESSION
    public async Task<bool> DeleteFileAsync(string filePath)
    {
        var requestUrl = $"{_filerUrl}/{filePath.TrimStart('/')}";
        var response = await _httpClient.DeleteAsync(requestUrl);

        return response.IsSuccessStatusCode;
    }

    public async Task<FileUrl> GetFileDataAsync(string filePath)
    {
        var requestUrl = $"{_filerUrl}/{filePath.TrimStart('/')}";
        var response = await _httpClient.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();

        var fileUrl = new FileUrl
        {
            Url = requestUrl,
            Name = filePath,
            Size = response.Content.Headers.ContentLength ?? 0
        };
        return fileUrl;
    }

    /// <summary>
    /// Télécharge un fichier à partir d'une URL complète
    /// Exemple: domaine.com/user/123/avatar.jpg -> télécharge avatar.jpg depuis user/123/
    /// </summary>
    /// <param name="fileUrl">URL complète du fichier (ex: https://domain.com/user/123/avatar.jpg)</param>
    /// <returns>Tuple contenant le flux du fichier, le type de contenu et le nom du fichier</returns>
    public async Task<(Stream Content, string ContentType, string FileName)> DownloadFileFromUrlAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            throw new ArgumentException("L'URL du fichier ne peut pas être vide.", nameof(fileUrl));

        // Parse l'URL pour extraire le chemin
        Uri uri;
        try
        {
            uri = new Uri(fileUrl);
        }
        catch (UriFormatException ex)
        {
            throw new ArgumentException("L'URL fournie n'est pas valide.", nameof(fileUrl), ex);
        }

        // Extrait le chemin sans le slash initial (ex: /user/123/avatar.jpg -> user/123/avatar.jpg)
        var filePath = uri.AbsolutePath.TrimStart('/');
        
        // Extrait le nom du fichier depuis le chemin
        var fileName = Path.GetFileName(filePath);

        // Utilise la méthode existante pour télécharger le fichier
        var (content, contentType) = await DownloadFileAsync(filePath);

        return (content, contentType, fileName);
    }
}