using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Playwright;
using SmartApply.Models;

namespace SmartApply.Services.Crawling
{
    public class StepStoneSource : IJobSource
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationService _gpt;
        private readonly BrowserHtmlLoader _browserHtmlLoader;

        public StepStoneSource(HttpClient httpClient,
                               ApplicationService gpt,
                               BrowserHtmlLoader browserHtmlLoader)
        {
            _httpClient = httpClient;
            _gpt = gpt;
            _browserHtmlLoader = browserHtmlLoader;
        }

        public async Task<List<JobPosting>> SearchJobsAsync(string keyword, string location, bool internshipsOnly)
        {
            Console.WriteLine($"🔍 Suche gestartet mit Keyword: {keyword}, Ort: {location}");

            var url = $"https://www.stepstone.de/jobs/{keyword.Replace(" ", "-").ToLower()}"
                    + $"/in-{location.Replace(" ", "-").ToLower()}?radius=30";

            var html = await _browserHtmlLoader.LoadRenderedHtmlAsync(url);
            var doc  = new HtmlDocument();
            doc.LoadHtml(html);

            var postings = new List<JobPosting>();
            var jobCards = doc.DocumentNode.SelectNodes("//article[@data-testid='job-item']");

            if (jobCards == null)
            {
                Console.WriteLine("❌ Keine Jobkarten gefunden!");
                return postings;
            }

            Console.WriteLine($"✅ Gefundene Stellen: {jobCards.Count}");

            foreach (var card in jobCards.Take(1))
            {
                var title   = card.SelectSingleNode(".//h2")?.InnerText.Trim() ?? "Kein Titel";
                var company = card.SelectSingleNode(".//span[contains(@class,'company-name')]")
                                  ?.InnerText.Trim() ?? "Unbekannt";

                var linkNode     = card.SelectSingleNode(".//a[@data-at='job-item-title']");
                var relativeLink = linkNode?.GetAttributeValue("href","") ?? "";
                if (string.IsNullOrWhiteSpace(relativeLink))
                {
                    Console.WriteLine("⚠️ Unvollständige Karte – übersprungen.");
                    continue;
                }

                var fullUrl = relativeLink.StartsWith("http")
                              ? relativeLink
                              : $"https://www.stepstone.de{relativeLink}";

                Console.WriteLine($"🧩 Titel:   {title}");
                Console.WriteLine($"🏢 Firma:   {company}");
                Console.WriteLine($"🔗 Link:    {fullUrl}");

                // Detailseite laden & Jobtext extrahieren
                var detailHtml = await _browserHtmlLoader.LoadRenderedHtmlAsync(fullUrl);
                var detailDoc  = new HtmlDocument();
                detailDoc.LoadHtml(detailHtml);

                var descNode = detailDoc.DocumentNode
                                   .SelectSingleNode("//span[contains(@class,'job-ad-display')]")
                               ?? detailDoc.DocumentNode
                                   .SelectSingleNode("//div[contains(@class,'listing-content')]");
                var descriptionText = descNode?.InnerText.Trim() ?? "Keine Beschreibung gefunden.";

                // Ansprechpartner (falls vorhanden)
                var contactPerson = detailDoc.DocumentNode
                    .SelectSingleNode("//span[contains(text(),'Ansprechpartner')]/following-sibling::span")
                    ?.InnerText.Trim() ?? "";

                // 1) Zusammenfassung
                var summary = await _gpt.SummarizeDescriptionAsync(descriptionText);

                // 2) JobPosting aufbauen
                var posting = new JobPosting
                {
                    Title         = title,
                    Company       = company,
                    Location      = location,
                    Address       = "",              // bei Bedarf noch extrahieren
                    ContactPerson = contactPerson,
                    Url           = fullUrl,
                    Description   = summary,
                    IsSelected    = false
                };

                // 3) Anschreiben generieren und in das Posting schreiben
                Console.WriteLine("✍️ Generiere Anschreiben …");
                posting.CoverLetter = await _gpt.GenerateCoverLetterAsync(posting);

                postings.Add(posting);
            }

            Console.WriteLine($"✅ Gefundene Jobs: {postings.Count}");
            return postings;
        }
    }
}
