using UglyToad.PdfPig;
using UglyToad.PdfPig.Annotations;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Geometry;
using System.Text.RegularExpressions;

namespace BackendLibrary
{
    /// <summary>
    /// Utility class for ShopTicket.cs to extract text from ShopTicket PDFs using PdfPig.
    /// </summary>
    internal class PdfTextExtractor
    {
        /// <summary>
        /// Extracts relevant text elements from a ShopTicket PDF. The TextGroup it extracts includes PageNames, 
        /// ProjectNumber, ProjectName, FileContentPieceMark, ControlNumbers, PiecesRequired, Weight, and DesignNumber.
        /// </summary>
        public static TextGroup GetExtractedText(ILogger<PdfTextExtractor> logger, byte[] pdfBytes)
        {
            logger.LogDebug("Start PdfPig PdfDocument initialization");
            using PdfDocument pdf = PdfDocument.Open(pdfBytes);
            logger.LogDebug("PdfPig PdfDocument opened");

            List<Page> pages = pdf.GetPages().ToList();
            logger.LogDebug("List<Page> created");

            if (pages.Count == 0)
            {
                throw new ExtractionException("No pages found in PDF document.");
            }

            // Check if PDF has text
            for (int i = 0; i < pages.Count; i++)
            {
                if (pages[i].GetWords().Any())
                {
                    break;
                }
                if (i == pages.Count - 1)
                {
                    throw new ExtractionException("No text found in PDF document.");
                }
            }

            TextGroup textGroup = new();

            CancellationTokenSource cts = new();
            List<Exception> exceptions = [];
            ParallelOptions opts = new() { CancellationToken = cts.Token };

            var jobs = new (Action Extract, string label)[]
            {
                (() => textGroup.PageNames            = ExtractPageNames(pages),            "PageNames"),
                (() => textGroup.ProjectNumber        = ExtractProjectNumber(pages),        "ProjectNumber"),
                (() => textGroup.ProjectName          = ExtractProjectName(pages),          "ProjectName"),
                (() => textGroup.FileContentPieceMark = ExtractFileContentPieceMark(pages), "FileContentPieceMark"),
                (() => textGroup.ControlNumbers       = ExtractControlNumbers(pages),       "ControlNumbers"),
                (() => textGroup.PiecesRequired       = ExtractPiecesRequired(pages),       "PiecesRequired"),
                (() => textGroup.Weight               = ExtractWeight(pages),               "Weight"),
                (() => textGroup.DesignNumber         = ExtractDesignNumber(pages),         "DesignNumber")
            };

            Parallel.Invoke(
                jobs.Select(job => (Action)(() =>
                {
                    try
                    {
                        job.Extract();
                        logger.LogDebug("{job.label} extracted", job.label);
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions) exceptions.Add(ex);
                        cts.Cancel();
                    }
                }))
                .ToArray()
            );

            if (exceptions.Count > 0)
            {
                throw exceptions.First();
            }

            return textGroup;
        }

        /// <summary>
        /// Utility method to extract page names from PDF pages based on AutoCAD annotations or view labels.
        /// </summary>
        private static string[] ExtractPageNames(List<Page> pages)
        {
            List<String> resultList = [];
            foreach (Page page in pages)
            {
                IEnumerable<Annotation> annotations = page.GetAnnotations();
                List<String> annotationStrings = [];

                // PDFs can either have AutoCAD annotations to signify view labels or no annotations
                // Move on to word search if there are no annotations
                if (!annotations.Any())
                {
                    resultList.Add(ExtractPageNameNoAnnotations(page));
                    continue;
                }

                foreach (Annotation annotation in annotations)
                {
                    // Annotation must be underlined to be considered
                    if (annotation.Content is null || !(annotation.Content.Contains("%%U", StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    String content = annotation.Content.Replace("%%U", "").Trim().ToLower();
                    annotationStrings.Add(content);
                }
                List<String> validPageNames = ["FORM VIEW", "FOAM DRAWING", "REVEAL DRAWING"];

                if (annotationStrings.Intersect(validPageNames, StringComparer.OrdinalIgnoreCase).Any())
                {
                    string pageNameStr = annotationStrings.Intersect(validPageNames, StringComparer.OrdinalIgnoreCase).First();

                    // Capitalize first letter of each word and remove space
                    IEnumerable<string> pageNameList = pageNameStr.Split(' ').Select(s => char.ToUpper(s[0]) + s.Substring(1));
                    pageNameStr = string.Join("", pageNameList);

                    resultList.Add(pageNameStr);
                }
                else // Move on to word search if annotations are invalid
                {
                    resultList.Add(ExtractPageNameNoAnnotations(page));
                }
            }
            return resultList.ToArray();
        }

        /// <summary>
        /// Utility method to extract page name from a PDF page without annotations by searching for specific words.
        /// </summary>
        private static string ExtractPageNameNoAnnotations(Page page)
        {
            IEnumerable<Word> words = page.GetWords();
            List<Word> formWords = GetWordsEqualTo("FORM", words);
            List<Word> drawingWords = GetWordsEqualTo("DRAWING", words);

            foreach (Word word in formWords)
            {
                DetectionBox formWordDetectionBox = new(MinX: 20, MaxX: 50, MinY: -1, MaxY: 1);
                Word? foundWord = FindWordNextTo(word, words, formWordDetectionBox);
                if (foundWord is null)
                {
                    if (word.BoundingBox.Left > 900)
                    {
                        continue;
                    }
                    DetectionBox formWordDetectionBox2 = new(MinX: -80, MaxX: -20, MinY: -1, MaxY: 1);
                    List<Word> foundWords = FindWordsNextTo(word, words, formWordDetectionBox2);
                    foreach (Word foundWord2 in foundWords)
                    {
                        if (string.Equals(foundWord2.Text, "TOP", StringComparison.OrdinalIgnoreCase))
                        {
                            return "FormView";
                        }
                    }
                    continue;
                }
                if (string.Equals(foundWord.Text, "VIEW", StringComparison.OrdinalIgnoreCase))
                {
                    return "FormView";
                }
            }

            foreach (Word word in drawingWords)
            {
                DetectionBox drawingWordDetectionBox = new(MinX: -80, MaxX: -20, MinY: -1, MaxY: 1);
                Word? foundWord = FindWordNextTo(word, words, drawingWordDetectionBox);
                if (foundWord is null)
                {
                    continue;
                }
                if (string.Equals(foundWord.Text, "FOAM", StringComparison.OrdinalIgnoreCase))
                {
                    return "FoamDrawing";
                }
                else if (string.Equals(foundWord.Text, "REVEAL", StringComparison.OrdinalIgnoreCase))
                {
                    return "RevealDrawing";
                }
            }

            foreach (Word word in words)
            {
                if (word.Text.Contains("REVEAL", StringComparison.OrdinalIgnoreCase))
                {
                    return "RevealDrawing";
                }
            }

            return "OtherPage";
        }

        /// <summary>
        /// Utility method to extract project number from PDF pages by searching for "JOB NO." keywords and its context.
        /// </summary>
        private static string ExtractProjectNumber(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> jobWords = GetWordsEqualTo("JOB", words);
                foreach (Word word in jobWords)
                {
                    DetectionBox jobWordDetectionBox = new(MinX: 5, MaxX: 15, MinY: -1, MaxY: 1);
                    Word? foundWord = FindWordNextTo(word, words, jobWordDetectionBox);
                    List<String> searchTerms = ["NO.", "NO.:", "NUMBER", "NUMBER:", "NUM", "NUM:", "#", "#:"];
                    if (foundWord is null)
                    {
                        continue;
                    }
                    // If the word next to "JOB" is "NO." or a variation of it
                    if (searchTerms.Any(searchTerm => searchTerm.Equals(foundWord.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        DetectionBox numberWordDetectionBox = new(MinX: -4, MaxX: 8, MinY: -12, MaxY: -4);
                        Word? piecesreqdWord = FindWordNextTo(word, words, numberWordDetectionBox);
                        if (piecesreqdWord is null)
                        {
                            throw new ExtractionException($"Missing data under ProjectNumber field.");
                        }
                        return piecesreqdWord.Text;
                    }
                }
            }
            throw new ExtractionException($"Failed to get ProjectNumber.");
        }

        /// <summary>
        /// Utility method to extract project name from PDF pages by searching for "PROJECT" keyword and its context.
        /// </summary>
        private static string ExtractProjectName(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> projectWords = GetWordsContaining("PROJECT", words);
                foreach (Word word in projectWords)
                {
                    DetectionBox projectWordDetectionBox = new(MinX: -2, MaxX: 120, MinY: -10, MaxY: 0);
                    List<Word> projectNameWords = FindWordsNextTo(word, words, projectWordDetectionBox);
                    if (projectNameWords.Count == 0)
                    {
                        throw new ExtractionException("Missing data under ProjectName field.");
                    }

                    List<String> projectNameStrList = projectNameWords.Select(w => w.Text).ToList();
                    return string.Join(" ", projectNameStrList);
                }
            }
            throw new ExtractionException("Failed to get ProjectName.");
        }

        /// <summary>
        /// Utility method to extract file content piece mark from PDF pages by searching for "PIECE MARK" keywords and its context.
        /// </summary>
        private static string ExtractFileContentPieceMark(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> pieceWords = GetWordsEqualTo("PIECE", words);
                foreach (Word word in pieceWords)
                {
                    DetectionBox pieceWordDetectionBox = new(MinX: 5, MaxX: 15, MinY: -1, MaxY: 1);
                    Word? foundWord = FindWordNextTo(word, words, pieceWordDetectionBox);
                    if (foundWord is null)
                    {
                        continue;
                    }
                    if (string.Equals(foundWord.Text, "MARK", StringComparison.OrdinalIgnoreCase))
                    {
                        DetectionBox markWordDetectionBox = new(MinX: -2, MaxX: 15, MinY: -10, MaxY: -2);
                        Word? piecemarkWord = FindWordNextTo(word, words, markWordDetectionBox);
                        if (piecemarkWord is null)
                        {
                            throw new ExtractionException("Missing data under FileContentPieceMark field.");
                        }
                        return piecemarkWord.Text;
                    }
                }
            }
            throw new ExtractionException("Failed to get FileContentPieceMark.");
        }

        /// <summary>
        /// Utility method to extract control numbers from PDF pages by searching for "CONTROL NO." keywords and its context.
        /// </summary>
        private static string[]? ExtractControlNumbers(List<Page> pages)
        {
            List<Word> controlnumWords = [];
            List<String> controlnumstrList = [];
            List<String> resultList = [];

            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<String> searchTerms = ["CONTROL", "CTRL"];
                List<Word> controlWords = GetWordsEqualTo(searchTerms, words);
                foreach (Word word in controlWords)
                {
                    DetectionBox controlWordDetectionBox = new DetectionBox(MinX: 5, MaxX: 35, MinY: -1, MaxY: 1);
                    Word? foundWord = FindWordNextTo(word, words, controlWordDetectionBox);
                    if (foundWord is null)
                    {
                        continue;
                    }

                    // Search method for when the word after "CONTROL" or "CTRL" is "NUMBER" or "NUMBER:"
                    searchTerms = ["NUMBER", "NUMBER:"];
                    if (searchTerms.Any(searchTerm => searchTerm.Equals(foundWord.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        DetectionBox numberWordDetectionBox = new(MinX: -4, MaxX: 50, MinY: -12, MaxY: -2);
                        controlnumWords = FindWordsNextTo(word, words, numberWordDetectionBox);
                        if (controlnumWords.Count == 0)
                        {
                            throw new ExtractionException("Missing data under ControlNumbers field.");
                        }
                    }

                    // Search method for when the word after "CONTROL" or "CTRL" is "NO.", "NO:", or "NO.:"
                    searchTerms = ["NO.", "NO:", "NO.:"];
                    if (searchTerms.Any(searchTerm => searchTerm.Equals(foundWord.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (word.BoundingBox.Left > 800 && word.BoundingBox.Left < 860)
                        {
                            DetectionBox numberWordDetectionBox2 = new(MinX: -4, MaxX: 30, MinY: -15, MaxY: -2);
                            controlnumWords = FindWordsNextTo(word, words, numberWordDetectionBox2);
                        }
                        else
                        {
                            DetectionBox numberWordDetectionBox3 = new(MinX: -4, MaxX: 150, MinY: -40, MaxY: -2);
                            controlnumWords = FindWordsNextTo(word, words, numberWordDetectionBox3);
                        }
                        if (controlnumWords.Count == 0)
                        {
                            throw new ExtractionException("Missing data under ControlNumbers field.");
                        }
                    }

                    if (controlnumWords is not null)
                    {
                        // Add each controlnumWord to controlnumstrList if it is not overlapping with any other word
                        for (int i = 0; i < controlnumWords.Count; i++)
                        {
                            bool isOverlapping = false;
                            for (int j = i + 1; j < controlnumWords.Count; j++)
                            {
                                // Check overlapping bounding boxes to avoid duplicates
                                if (controlnumWords[i].BoundingBox.IntersectsWith(controlnumWords[j].BoundingBox))
                                {
                                    isOverlapping = true;
                                    break;
                                }
                            }
                            if (!isOverlapping)
                            {
                                controlnumstrList.Add(controlnumWords[i].Text);
                            }
                        }

                        controlnumstrList.ForEach(str => resultList.AddRange(str.Split(',', StringSplitOptions.RemoveEmptyEntries)));

                        return resultList.ToArray();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Utility method to extract pieces required from PDF pages by searching for "PIECES REQ'D" keywords and its context.
        /// </summary>
        private static int ExtractPiecesRequired(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> piecesWords = GetWordsEqualTo("PIECES", words);
                foreach (Word word in piecesWords)
                {
                    DetectionBox piecesWordDetectionBox = new(MinX: 5, MaxX: 15, MinY: -1, MaxY: 1);
                    Word? foundWord = FindWordNextTo(word, words, piecesWordDetectionBox);
                    if (foundWord is null)
                    {
                        continue;
                    }
                    if (foundWord.Text.Contains("REQ", StringComparison.OrdinalIgnoreCase))
                    {
                        DetectionBox reqWordDetectionBox = new(MinX: -2, MaxX: 30, MinY: -10, MaxY: -4);
                        Word? piecesreqdWord = FindWordNextTo(word, words, reqWordDetectionBox);
                        if (piecesreqdWord is null)
                        {
                            throw new ExtractionException("Missing data under PiecesRequired field.");
                        }
                        return int.Parse(piecesreqdWord.Text);
                    }
                }
            }
            throw new ExtractionException("Failed to get PiecesRequired.");
        }

        /// <summary>
        /// Utility method to extract weight from PDF pages by searching for "WEIGHT" keyword and its context.
        /// </summary>
        private static decimal ExtractWeight(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> weightWords = GetWordsContaining("WEIGHT", words);
                foreach (Word word in weightWords)
                {
                    DetectionBox weightWordDetectionBox = new(MinX: -10, MaxX: 30, MinY: -20, MaxY: -1);
                    Word? weightWord = FindWordNextTo(word, words, weightWordDetectionBox);

                    if (weightWord is null)
                    {
                        throw new ExtractionException("Missing data under Weight field.");
                    }

                    // Remove any non-numeric characters except for decimal points and commas
                    string pattern = "[^0-9,.]";
                    string weightStr = Regex.Replace(weightWord.Text, pattern, "");

                    return decimal.Parse(weightStr);
                }
            }
            throw new ExtractionException("Failed to get Weight.");
        }

        /// <summary>
        /// Utility method to extract design number from PDF pages by searching for "DESIGN" keyword and its context.
        /// </summary>
        private static string ExtractDesignNumber(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> designWords = GetWordsEqualTo("DESIGN:", words);
                foreach (Word word in designWords)
                {
                    DetectionBox designWordDetectionBox = new(MinX: -2, MaxX: 30, MinY: -20, MaxY: -4);
                    Word? foundWord = FindWordNextTo(word, words, designWordDetectionBox);
                    if (foundWord is null)
                    {
                        throw new ExtractionException("Missing data under DesignNumber field.");
                    }
                    return foundWord.Text;
                }
            }
            throw new ExtractionException("Failed to get DesignNumber.");
        }

        /// <summary>
        /// Gets all words equal to the searchText from IEnumerable&lt;Word&gt; words
        /// </summary>
        private static List<Word> GetWordsEqualTo(string searchText, IEnumerable<Word> words)
        {
            return (from Word word in words
                    where word.Text.Equals(searchText, StringComparison.OrdinalIgnoreCase)
                    select word).ToList();
        }

        /// <summary>
        /// Gets all words equal to any of the searchTexts from IEnumerable&lt;Word&gt; words
        /// </summary>
        private static List<Word> GetWordsEqualTo(List<string> searchTexts, IEnumerable<Word> words)
        {
            return (from Word word in words
                    where searchTexts.Any(searchText => word.Text.Equals(searchText, StringComparison.OrdinalIgnoreCase))
                    select word).ToList();
        }

        /// <summary>
        /// Gets all words containing the searchText from IEnumerable&lt;Word&gt; words
        /// </summary>
        private static List<Word> GetWordsContaining(string searchText, IEnumerable<Word> words)
        {
            return (from Word word in words
                    where word.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    select word).ToList();
        }

        /// <summary>
        /// Holds the detection boundary values for find word(s) methods
        /// </summary>
        private record DetectionBox(int MinX, int MaxX, int MinY, int MaxY);

        /// <summary>
        /// Finds one word among IEnumerable&lt;Word&gt; words relative to an anchorWord given specified bounds
        /// </summary>
        private static Word? FindWordNextTo(Word anchorWord, IEnumerable<Word> words, DetectionBox detectionBox)
        {
            return (from Word word in words
                    where (word.BoundingBox.Left - anchorWord.BoundingBox.Left > detectionBox.MinX) &&
                      (word.BoundingBox.Left - anchorWord.BoundingBox.Left < detectionBox.MaxX) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top > detectionBox.MinY) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top < detectionBox.MaxY)
                    select word).FirstOrDefault();
        }

        /// <summary>
        /// Finds all words among IEnumerable&lt;Word&gt; words relative to an anchorWord given specified bounds
        /// </summary>
        private static List<Word> FindWordsNextTo(Word anchorWord, IEnumerable<Word> words, DetectionBox detectionBox)
        {
            return (from Word word in words
                    where (word.BoundingBox.Left - anchorWord.BoundingBox.Left > detectionBox.MinX) &&
                      (word.BoundingBox.Left - anchorWord.BoundingBox.Left < detectionBox.MaxX) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top > detectionBox.MinY) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top < detectionBox.MaxY)
                    select word).ToList();
        }
    }
}
