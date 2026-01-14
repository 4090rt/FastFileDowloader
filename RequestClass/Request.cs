using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace FileDowloader.Request
{
    public class Requestt
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Requestt> _logger;
        private SemaphoreSlim _semaphor = new SemaphoreSlim(2,2);


        public Requestt(IHttpClientFactory clientFactory, ILogger<Requestt> logger)
        {
            _httpClientFactory = clientFactory;
            _logger = logger;
        }

        public async Task Semaphore(string url, CancellationToken cancellationToken = default)
        { 
            await _semaphor.WaitAsync(cancellationToken);

            try
            {
                await HttpRequest(url, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Возинкло исключение", ex.Message);
            }
            finally
            { 
                _semaphor.Release();
            }
        }

        public async Task<bool> HttpRequest(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("httpclient");
                var path = "";

                try
                {
                    _logger.LogInformation("Начинаем запрос к {Url}", url);
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                    _logger.LogInformation("Запрос завершен. StatusCode: {StatusCode}", response.StatusCode);
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] content = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                        if (content != null)
                        {
                            using (FileStream file = System.IO.File.Create(path))
                            { 
                                file.Write(content, 0, content.Length);
                                return true;
                            }
                        }
                        else
                        {
                            _logger.LogError("Поток байтов пуст");
                            return false;
                        }
                    }
                    else
                    {
                        _logger.LogError("Запрос завершился с ошибкой {StatusCode}", response.StatusCode);
                        return false;
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError("Ошибка в теле запроса" + ex.Message);
                    return false;
                }
            }
            catch(TaskCanceledException ex)
            {
                _logger.LogWarning($"Таймаут запроса: {ex.Message}");
                return false;
            }
            catch(HttpRequestException ex)
            {
                _logger.LogWarning($"Ошибка HTTP запроса: {ex.Message}");
                return false;
            }
            catch(Exception ex)
            {
                _logger.LogWarning("Возникло исключение при попытке запроса:" + ex.Message);
                return false;
            }
        }
    }

}
