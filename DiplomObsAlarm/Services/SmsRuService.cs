using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Web;

namespace DiplomObsAlarm.Services;

public class SmsRuService
{
    private readonly HttpClient _httpClient;
    private const string ApiKey = "722DCFC1-A38E-8003-5A46-4E263D9510FC";
    private const string BaseUrl = "https://sms.ru/sms/send";

    public SmsRuService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<(bool success, string response, string error)> SendSmsAsync(string phone, string message)
    {
        try
        {
            // 1. Очистка номера телефона
            string originalPhone = phone;
            phone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");

            Debug.WriteLine($"[SMS] Исходный номер: {originalPhone}");
            Debug.WriteLine($"[SMS] Очищенный номер: {phone}");

            // 2. Приведение к формату 7XXXXXXXXXX
            if (phone.StartsWith("8") && phone.Length == 11)
            {
                phone = "7" + phone.Substring(1);
                Debug.WriteLine($"[SMS] Замена 8 на 7: {phone}");
            }
            else if (phone.StartsWith("9") && phone.Length == 10)
            {
                phone = "7" + phone;
                Debug.WriteLine($"[SMS] Добавлена 7: {phone}");
            }

            // 3. Проверка формата
            if (!phone.StartsWith("7") || phone.Length != 11)
            {
                return (false, "", $"Неверный формат номера: {phone} (должен быть 7XXXXXXXXXX)");
            }

            // 4. Проверка длины сообщения
            if (message.Length > 160)
            {
                Debug.WriteLine($"[SMS] Предупреждение: сообщение длинное ({message.Length} символов)");
            }

            // 5. Формирование URL
            var encodedMessage = HttpUtility.UrlEncode(message);
            var url = $"{BaseUrl}?api_id={ApiKey}&to={phone}&msg={encodedMessage}&json=1";

            Debug.WriteLine($"[SMS] URL: {url}");

            // 6. Отправка запроса
            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await _httpClient.GetAsync(url);
            }
            catch (TaskCanceledException)
            {
                return (false, "", "Таймаут запроса (30 сек)");
            }
            catch (HttpRequestException ex)
            {
                return (false, "", $"Ошибка HTTP: {ex.Message}");
            }

            // 7. Чтение ответа
            string content;
            try
            {
                content = await httpResponse.Content.ReadAsStringAsync();
                Debug.WriteLine($"[SMS] Статус HTTP: {(int)httpResponse.StatusCode}");
                Debug.WriteLine($"[SMS] Полный ответ: {content}");
            }
            catch (Exception ex)
            {
                return (false, "", $"Ошибка чтения ответа: {ex.Message}");
            }

            // 8. Проверка пустого ответа
            if (string.IsNullOrWhiteSpace(content))
            {
                return (false, "", "Пустой ответ от сервера");
            }

            // 9. Попытка парсинга JSON
            SmsRuResponse? smsResponse = null;
            try
            {
                smsResponse = JsonSerializer.Deserialize<SmsRuResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"[SMS] Ошибка парсинга JSON: {ex.Message}");
                // Продолжаем - попробуем разобрать вручную
            }

            // 10. Анализ ответа
            if (smsResponse != null && smsResponse.Status != null)
            {
                Debug.WriteLine($"[SMS] Статус из JSON: {smsResponse.Status}");

                if (smsResponse.Status == "OK")
                {
                    var sms = smsResponse.Sms?.FirstOrDefault().Value;
                    if (sms != null)
                    {
                        Debug.WriteLine($"[SMS] Статус SMS: {sms.Status}, ID: {sms.SmsId}");

                        if (sms.Status == "OK")
                        {
                            return (true, content, "");
                        }
                        else
                        {
                            return (false, content, $"SMS не отправлено: {sms.StatusText} (код: {sms.StatusCode})");
                        }
                    }
                    return (true, content, "");
                }

                if (smsResponse.Status == "ERROR")
                {
                    var errorText = smsResponse.StatusText ?? "Неизвестная ошибка";
                    var errorCode = smsResponse.StatusCode;
                    Debug.WriteLine($"[SMS] Ошибка API: {errorCode} - {errorText}");

                    // Расшифровка кодов ошибок
                    string description = errorCode switch
                    {
                        100 => "Неправильный api_id",
                        101 => "Неправильный пароль",
                        102 => "Номер в стоп-листе",
                        103 => "Номер запрещён",
                        104 => "Номер не существует",
                        105 => "Недостаточно средств",
                        106 => "Неверная подпись",
                        107 => "IP не в белом списке",
                        108 => "Ключ не активирован",
                        109 => "Аккаунт заблокирован",
                        110 => "Неправильный номер",
                        111 => "Сообщение пустое",
                        112 => "Сообщение слишком длинное",
                        113 => "Превышен лимит отправки",
                        114 => "Нельзя отправлять на этот номер",
                        115 => "Время отправки ограничено",
                        116 => "Неправильный формат времени",
                        117 => "Время отправки в прошлом",
                        118 => "Слишком частая отправка",
                        130 => "Неправильный параметр to",
                        131 => "Неправильный параметр msg",
                        132 => "Неправильный параметр from",
                        133 => "Неправильный параметр time",
                        134 => "Неправильный параметр translit",
                        135 => "Неправильный параметр test",
                        136 => "Неправильный параметр partner_id",
                        137 => "Неправильный параметр format",
                        _ => $"Неизвестная ошибка (код {errorCode})"
                    };

                    return (false, content, $"{errorText}. {description}");
                }
            }

            // 11. Ручной парсинг если JSON не сработал
            Debug.WriteLine("[SMS] Ручной анализ ответа...");

            if (content.Contains("status"))
            {
                var status = ExtractField(content, "status");
                var statusText = ExtractField(content, "status_text");
                var statusCode = ExtractField(content, "status_code");

                Debug.WriteLine($"[SMS] Ручной парсинг: status={status}, text={statusText}, code={statusCode}");

                if (status == "OK")
                    return (true, content, "");

                return (false, content, $"Ошибка: {statusText} (код: {statusCode})");
            }

            // 12. Неизвестный формат ответа
            Debug.WriteLine($"[SMS] Неизвестный формат: {content}");
            return (false, content, $"Неизвестный ответ: {content.Substring(0, Math.Min(100, content.Length))}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SMS] Критическая ошибка: {ex}");
            return (false, "", $"Критическая ошибка: {ex.Message}");
        }
    }

    public async Task<(int sent, int failed, List<string> errors)> SendBulkSmsAsync(List<string> phones, string message)
    {
        int sent = 0;
        int failed = 0;
        var errors = new List<string>();

        foreach (var phone in phones.Distinct())
        {
            var (success, response, error) = await SendSmsAsync(phone, message);

            if (success)
            {
                sent++;
                Debug.WriteLine($"[SMS] ✓ Успешно: {phone}");
            }
            else
            {
                failed++;
                errors.Add($"{phone}: {error}");
                Debug.WriteLine($"[SMS] ✗ Ошибка {phone}: {error}");
            }

            // Небольшая задержка между отправками
            await Task.Delay(100);
        }

        Debug.WriteLine($"[SMS] Итого: отправлено {sent}, ошибок {failed}");
        return (sent, failed, errors);
    }

    public async Task<string> CheckBalance()
    {
        try
        {
            var url = $"https://sms.ru/my/balance?api_id={ApiKey}&json=1";
            var response = await _httpClient.GetStringAsync(url);
            return response;
        }
        catch (Exception ex)
        {
            return $"Ошибка проверки баланса: {ex.Message}";
        }
    }

    private string ExtractField(string json, string fieldName)
    {
        try
        {
            var pattern = $"\"{fieldName}\":";
            var idx = json.IndexOf(pattern);
            if (idx == -1) return "";

            idx += pattern.Length;

            // Пропускаем пробелы
            while (idx < json.Length && char.IsWhiteSpace(json[idx])) idx++;

            // Проверяем кавычки
            bool hasQuotes = idx < json.Length && json[idx] == '"';
            if (hasQuotes) idx++;

            var start = idx;
            if (hasQuotes)
            {
                var end = json.IndexOf('"', start);
                if (end == -1) return "";
                return json.Substring(start, end - start);
            }
            else
            {
                // Число или null
                var end = json.IndexOfAny(new[] { ',', '}', ']' }, start);
                if (end == -1) end = json.Length;
                return json.Substring(start, end - start).Trim();
            }
        }
        catch
        {
            return "";
        }
    }
}

// Классы для десериализации JSON
public class SmsRuResponse
{
    public string? Status { get; set; }
    public int StatusCode { get; set; }
    public string? StatusText { get; set; }
    public Dictionary<string, SmsInfo>? Sms { get; set; }
    public string? Balance { get; set; }
}

public class SmsInfo
{
    public string? Status { get; set; }
    public int StatusCode { get; set; }
    public string? StatusText { get; set; }
    public string? SmsId { get; set; }
    public decimal? Cost { get; set; }
}