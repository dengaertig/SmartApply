using Microsoft.Playwright;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SmartApply.Services.Crawling;

public class BrowserHtmlLoader
{
    public async Task<string> LoadRenderedHtmlAsync(string url)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            Timeout = 60000
        });

        var page = await browser.NewPageAsync();

        Console.WriteLine($"🌐 Navigiere zu: {url}");

        await page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 60000
        });

        // ⏳ Warte kurz auf Initial-Layout
        await page.WaitForTimeoutAsync(2000);

        // 🍪 Cookie-Banner klicken
        try
        {
            var cookieButton = await page.WaitForSelectorAsync("#ccmgt_explicit_accept", new()
            {
                Timeout = 8000,
                State = WaitForSelectorState.Attached
            });

            if (cookieButton != null && await cookieButton.IsVisibleAsync())
            {
                Console.WriteLine("✅ Cookie-Banner sichtbar – klicke...");
                await cookieButton.ClickAsync(new() { Force = true });
                await page.WaitForTimeoutAsync(2000);
            }
            else
            {
                Console.WriteLine("⚠️ Cookie-Button nicht sichtbar.");
            }
        }
        catch (TimeoutException)
        {
            Console.WriteLine("❌ Kein Cookie-Banner gefunden.");
        }

        // 🔐 „Weiter als Recruiter“-Popup schließen
        try
        {
            var recruiterButton = await page.WaitForSelectorAsync("//span[text()='Weiter als Recruiter']/ancestor::button", new()
            {
                Timeout = 8000,
                State = WaitForSelectorState.Attached
            });
            if (recruiterButton != null && await recruiterButton.IsVisibleAsync())
            {
                Console.WriteLine("✅ 'Weiter als Recruiter' Popup – klicke...");
                await recruiterButton.ClickAsync(new() { Force = true });
                await page.WaitForTimeoutAsync(2000);
            }
            else
            {
                Console.WriteLine("⚠️ 'Weiter als Recruiter' Button nicht gefunden.");
            }
        }
        catch (TimeoutException)
        {
            Console.WriteLine("❌ Kein 'Weiter als Recruiter' Popup sichtbar.");
        }

        // 🕵️‍♂️ Jetzt auf Jobkarten warten
        try
        {
            await page.WaitForSelectorAsync("article[data-testid='job-item']", new()
            {
                Timeout = 15000,
                State = WaitForSelectorState.Attached
            });

            Console.WriteLine("✅ Jobkarten gefunden!");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("❌ Timeout: Keine Jobkarten sichtbar.");
        }

        await page.WaitForTimeoutAsync(1000);

        var html = await page.ContentAsync();

        // 💾 Speichere HTML lokal zur Analyse
        var path = Path.Combine(Directory.GetCurrentDirectory(), "stepstone_debug_output.html");
        await File.WriteAllTextAsync(path, html);
        Console.WriteLine($"📁 HTML gespeichert unter: {path}");

        return html;
    }
}
