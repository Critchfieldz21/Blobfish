using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace BackendLibrary
{
    public class PageNameNotFoundException : Exception
    {
        public int PageNumber { get; }
        public PageNameNotFoundException(int pageNumber, string msg) : base(msg) { PageNumber = pageNumber; }
    }

    // Extract a “page name” per page using a header-band heuristic (top ~1 inch).
    internal class PdfPageNameExtractor
    {
        private readonly double _topBandHeight;
        public PdfPageNameExtractor(double topBandHeightPoints = 72) => _topBandHeight = topBandHeightPoints;

        /// <summary>
        /// Returns a page-name for each page (1-based). Throws if any page is missing a name.
        /// Enforces max length 100 chars.
        /// </summary>
        public IReadOnlyList<string> ExtractAll(string pdfPath, out TimeSpan elapsed)
        {
            var sw = Stopwatch.StartNew();
            using var doc = PdfDocument.Open(System.IO.File.OpenRead(pdfPath));
            var names = new List<string>(doc.NumberOfPages);

            foreach (var page in doc.GetPages())
            {
                var words = page.GetWords().ToList();

                
                // Coordinates: origin bottom-left, so "top band" is [page.Height - _topBandHeight, page.Height]
                var pageTopY = page.Height;

                var headerWords = words
                    .Where(w => w.BoundingBox.Top >= pageTopY - _topBandHeight)
                    .OrderBy(w => w.BoundingBox.Left)
                    .ToList();

                string? header = JoinTopLine(headerWords);
                if (string.IsNullOrWhiteSpace(header))
                {
                    // Fallback: first non-empty line anywhere on the page
                    header = FirstNonEmptyLine(words);
                }

                header = Normalize(header);

                if (string.IsNullOrWhiteSpace(header))
                    throw new PageNameNotFoundException(page.Number, $"Page name not found (page {page.Number}).");

                if (header.Length > 100) header = header[..100];
                names.Add(header);
            }

            sw.Stop();
            elapsed = sw.Elapsed;
            return names;
        }

        private static string Normalize(string? s)
            => string.IsNullOrWhiteSpace(s)
               ? ""
               : System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ").Trim();

        // Join words that lie on the top-most text line inside the header band
        private static string? JoinTopLine(List<Word> words)
        {
            if (words.Count == 0) return null;
            const double yTol = 5.0; // points
            var lines = words
                .GroupBy(w => Math.Round(w.BoundingBox.Top / yTol))
                .OrderByDescending(g => g.Key) // highest Y first
                .ToList();

            var top = lines.FirstOrDefault();
            if (top == null) return null;

            return string.Join(" ", top.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text));
        }

        private static string? FirstNonEmptyLine(List<Word> words)
        {
            const double yTol = 5.0;
            var lines = words
                .GroupBy(w => Math.Round(w.BoundingBox.Top / yTol))
                .OrderByDescending(g => g.Key); // top to bottom

            foreach (var line in lines)
            {
                var text = string.Join(" ", line.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)).Trim();
                if (!string.IsNullOrWhiteSpace(text)) return text;
            }
            return null;
        }
    }
}
