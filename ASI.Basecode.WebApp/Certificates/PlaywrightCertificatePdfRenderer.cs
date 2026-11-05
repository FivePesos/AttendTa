using ASI.Basecode.Services.Interfaces;
using Microsoft.Playwright;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Certificates;

/// <summary>Renders a fixed, self-contained sample for the Development certificate tracer.</summary>
public sealed class PlaywrightCertificatePdfRenderer : ICertificatePdfRenderer
{
    private const string SampleHtml = """
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <style>
            @page { size: A4 landscape; margin: 16mm; }
            * { box-sizing: border-box; }
            body { margin: 0; color: #142a40; font-family: Arial, sans-serif; }
            .sheet { min-height: 178mm; border: 3px solid #287a75; padding: 20mm 24mm; text-align: center; }
            .eyebrow { color: #287a75; font-size: 13pt; font-weight: 700; letter-spacing: .16em; text-transform: uppercase; }
            h1 { margin: 16mm 0 5mm; font-size: 34pt; }
            .recipient { margin: 0 0 5mm; font-size: 26pt; font-weight: 700; }
            .copy { margin: 0 auto; max-width: 210mm; font-size: 14pt; line-height: 1.6; }
            .notice { display: inline-block; margin-top: 14mm; border: 1px solid #b54635; color: #9d3024; padding: 3mm 8mm; font-size: 12pt; font-weight: 700; letter-spacing: .08em; }
          </style>
        </head>
        <body>
          <main class="sheet">
            <div class="eyebrow">AttendTa Development Tracer</div>
            <h1>Sample Certificate</h1>
            <p class="recipient">Registration PDF integration check</p>
            <p class="copy">This fixed sample confirms the development rendering and private-storage path for a confirmed registration.</p>
            <div class="notice">SAMPLE · NOT AN OFFICIAL CERTIFICATE</div>
          </main>
        </body>
        </html>
        """;

    public async Task<byte[]> RenderSampleAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        await using var page = await browser.NewPageAsync();
        await page.SetContentAsync(SampleHtml, new PageSetContentOptions
        {
            WaitUntil = WaitUntilState.Load
        });
        cancellationToken.ThrowIfCancellationRequested();
        return await page.PdfAsync(new PagePdfOptions
        {
            Format = "A4",
            Landscape = true,
            PrintBackground = true,
            PreferCSSPageSize = false
        });
    }
}
