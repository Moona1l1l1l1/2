using System.Net;
using System.Text.Json;
using UniversityClient.Models;

namespace UniversityClient.Services;

public interface ICourseApiClient
{
    Task<ApiResult<List<CourseViewModel>>> GetCoursesAsync(CancellationToken ct = default);
    Task<ApiResult<CourseViewModel>> GetCourseAsync(int id, CancellationToken ct = default);
}

/// <summary>Типизированный клиент на IHttpClientFactory. К БД не обращается — только HTTP к API.</summary>
public class CourseApiClient : ICourseApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly string _coursesPath;
    private readonly ILogger<CourseApiClient> _logger;

    public CourseApiClient(HttpClient http, IConfiguration config, ILogger<CourseApiClient> logger)
    {
        _http = http;
        _logger = logger;
        _coursesPath = (config["ApiSettings:CoursesPath"] ?? "api/v2/courses").Trim('/');
    }

    public Task<ApiResult<List<CourseViewModel>>> GetCoursesAsync(CancellationToken ct = default)
        => SendAsync<List<CourseViewModel>>(_coursesPath, ct);

    public Task<ApiResult<CourseViewModel>> GetCourseAsync(int id, CancellationToken ct = default)
        => SendAsync<CourseViewModel>($"{_coursesPath}/{id}", ct);

    private async Task<ApiResult<T>> SendAsync<T>(string path, CancellationToken ct)
    {
        try
        {
            using var response = await _http.GetAsync(path, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return ApiResult<T>.Fail("Запрошенные данные не найдены.", notFound: true);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API returned {StatusCode} for {Path}", (int)response.StatusCode, path);
                return ApiResult<T>.Fail($"API вернул ошибку: {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<T>>(JsonOptions, ct);
            if (envelope is null || !envelope.IsSuccess || envelope.Result is null)
                return ApiResult<T>.Fail(envelope?.ErrorMessage ?? "API вернул пустой или некорректный ответ.");

            return ApiResult<T>.Ok(envelope.Result);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("API request timed out: {Path}", path);
            return ApiResult<T>.Fail("Превышено время ожидания ответа от API.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API is unreachable: {Path}", path);
            return ApiResult<T>.Fail("API недоступен. Проверьте, что сервер запущен и адрес указан верно.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON from API: {Path}", path);
            return ApiResult<T>.Fail("API вернул данные в неожиданном формате.");
        }
    }
}
