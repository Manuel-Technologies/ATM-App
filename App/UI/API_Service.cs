using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ATMApp.Services;

public sealed class APIService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly bool _ownsHttpClient;
    private bool _disposed;

    public APIService(string baseUrl, HttpClient? httpClient = null)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Base URL is required.", nameof(baseUrl));
        }

        _httpClient = httpClient ?? new HttpClient();
        _ownsHttpClient = httpClient is null;
        _httpClient.BaseAddress = new Uri(AppendTrailingSlash(baseUrl));
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public async Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(
            NormalizeEndpoint(endpoint),
            cancellationToken);

        return await HandleResponse<TResponse>(response, cancellationToken);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        using StringContent content = CreateJsonContent(payload);
        using HttpResponseMessage response = await _httpClient.PostAsync(
            NormalizeEndpoint(endpoint),
            content,
            cancellationToken);

        return await HandleResponse<TResponse>(response, cancellationToken);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        using StringContent content = CreateJsonContent(payload);
        using HttpResponseMessage response = await _httpClient.PutAsync(
            NormalizeEndpoint(endpoint),
            content,
            cancellationToken);

        return await HandleResponse<TResponse>(response, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.DeleteAsync(
            NormalizeEndpoint(endpoint),
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"DELETE request failed with status {(int)response.StatusCode}: {errorContent}");
    }

    public void AddBearerToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token is required.", nameof(token));
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearBearerToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private StringContent CreateJsonContent<TRequest>(TRequest payload)
    {
        string json = JsonSerializer.Serialize(payload, _serializerOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task<TResponse?> HandleResponse<TResponse>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Request failed with status {(int)response.StatusCode}: {rawContent}");
        }

        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TResponse>(rawContent, _serializerOptions);
    }

    private static string NormalizeEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Endpoint is required.", nameof(endpoint));
        }

        return endpoint.TrimStart('/');
    }

    private static string AppendTrailingSlash(string url)
    {
        return url.EndsWith("/") ? url : $"{url}/";
    }
}
