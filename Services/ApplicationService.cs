using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SmartApply.Models;
using Microsoft.Extensions.Configuration;

namespace SmartApply.Services
{
    public class ApplicationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public ApplicationService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config     = config;
        }

        public async Task<string> GenerateCoverLetterAsync(JobPosting posting)
        {
            var prompt = $@"
Erstelle ein individuelles Bewerbungsschreiben auf Deutsch für folgende Stelle:

- Unternehmen: {posting.Company}
- Ansprechpartner: {(string.IsNullOrEmpty(posting.ContactPerson) ? "nicht bekannt" : posting.ContactPerson)}
- Adresse: {posting.Address}
- Ort: {posting.Location}
- Position: {posting.Title}

Stellenbeschreibung:
{posting.Description}

Das Anschreiben soll:
- direkt an die Firma gerichtet sein
- mit einer formellen Anrede starten
- auf die konkrete Position und Firma eingehen
- höflich, professionell und individuell klingen
- als Fließtext formuliert sein (kein Bullet-Style)

Beginne mit Adresse und Betreff wie in einem klassischen DIN-5008-Anschreiben.";

            var requestBody = new
            {
                model    = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "Du bist ein professioneller Bewerbungsschreiber." },
                    new { role = "user",   content = prompt }
                }
            };

            var requestJson = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _config["OpenAI:ApiKey"]);

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                requestJson
            );
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString()
                   ?? "Fehler bei GPT-Antwort.";
        }

        public async Task<string> SummarizeDescriptionAsync(string fullDescription)
        {
            var prompt = $"Fasse folgenden Jobtext stichpunktartig in 3 kurzen Bulletpoints zusammen (max. 300 Zeichen insgesamt):\n\n{fullDescription}";

            var requestBody = new
            {
                model    = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "Du bist ein Assistent für Jobzusammenfassungen." },
                    new { role = "user",   content = prompt }
                }
            };

            var requestJson = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _config["OpenAI:ApiKey"]);

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                requestJson
            );
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString()
                   ?? "Fehler bei GPT-Zusammenfassung.";
        }
    }
}
