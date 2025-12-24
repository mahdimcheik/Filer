using Filer.Utilities;
using System.Net.Http.Headers;

namespace Filer.Secvices;

public interface IFileService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath);
    Task<(Stream Content, string ContentType)> DownloadFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
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
}