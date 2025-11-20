using UglyToad.PdfPig;
using UglyToad.PdfPig.Annotations;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Geometry;
using System.Text.RegularExpressions;

namespace BackendLibrary
{
    internal class PdfTextExtractor
    {
        public static TextGroup GetExtractedText(ILogger<PdfTextExtractor> logger, byte[] pdfBytes)
        {
            logger.LogDebug("Start PdfPig PdfDocument initialization");
            PdfDocument pdf = PdfDocument.Open(pdfBytes);
            logger.LogDebug("PdfPig PdfDocument opened");

            List<Page> pages = pdf.GetPages().ToList();
            logger.LogDebug("List<Page> created");

            TextGroup textGroup = new TextGroup();

            var cts = new CancellationTokenSource();
            List<Exception> exceptions = new();
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

        private static string[] ExtractPageNames(List<Page> pages)
        {
            List<String> resultList = new List<String>();
            List<String> annotationStrings = new List<String>();
            foreach (Page page in pages)
            {
                IEnumerable<Annotation> annotations = page.GetAnnotations();

                // PDFs can either have AutoCAD annotations to signify view labels or no annotations
                // Move on to word search if there are no annotations
                if (annotations.Count() == 0)
                {
                    resultList.Add(ExtractPageNameNoAnnotations(page));
                    continue;
                }

                annotationStrings.Clear();
                foreach (Annotation annotation in annotations)
                {
                    // Annotation must be underlined to be considered
                    if (annotation.Content is null || !(annotation.Content.Contains("%%U", StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    String content = annotation.Content.Replace("%%U", "").Trim().ToUpper();
                    annotationStrings.Add(content);
                }

                if (annotationStrings.Contains("FORM VIEW"))
                {
                    resultList.Add("FormView");
                }
                else if (annotationStrings.Contains("FOAM DRAWING"))
                {
                    resultList.Add("FoamDrawing");
                }
                else if (annotationStrings.Contains("REVEAL DRAWING"))
                {
                    resultList.Add("RevealDrawing");
                }
                else // Move on to word search if annotations are invalid
                {
                    resultList.Add(ExtractPageNameNoAnnotations(page));
                }
            }
            return resultList.ToArray();
        }

        private static string ExtractPageNameNoAnnotations(Page page)
        {
            IEnumerable<Word> words = page.GetWords();
            List<Word> formWords = (from Word word in words
                                    where word.Text.Equals("FORM", StringComparison.OrdinalIgnoreCase)
                                    select word).ToList();
            List<Word> drawingWords = (from Word word in words
                                       where word.Text.Equals("DRAWING", StringComparison.OrdinalIgnoreCase)
                                       select word).ToList();
            foreach (Word word in formWords)
            {
                DetectionBox formWordDetectionBox = new DetectionBox(minX: 20, maxX: 50, minY: -1, maxY: 1);
                Word? foundWord = FindWordNextTo(word, words, formWordDetectionBox);
                if (foundWord is null)
                {
                    if (word.BoundingBox.Left > 900)
                    {
                        continue;
                    }
                    DetectionBox formWordDetectionBox2 = new DetectionBox(minX: -80, maxX: -20, minY: -1, maxY: 1);
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
                DetectionBox drawingWordDetectionBox = new DetectionBox(minX: -80, maxX: -20, minY: -1, maxY: 1);
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
        private static string ExtractProjectNumber(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> jobWords = (from Word word in words
                                       where word.Text.Equals("JOB", StringComparison.OrdinalIgnoreCase)
                                       select word).ToList();
                foreach (Word word in jobWords)
                {
                    DetectionBox jobWordDetectionBox = new DetectionBox(minX: 5, maxX: 15, minY: -1, maxY: 1);
                    Word? foundWord = FindWordNextTo(word, words, jobWordDetectionBox);
                    List<String> searchTerms = new List<String> { "NO.", "NO:", "NUMBER", "NUMBER:", "NUM", "NUM:" };
                    if (foundWord is null)
                    {
                        continue;
                    }
                    // If the word next to "JOB" is "NUMBER" or a variation of it
                    if (searchTerms.Any(searchTerm => searchTerm.Equals(foundWord.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        DetectionBox numberWordDetectionBox = new DetectionBox(minX: -4, maxX: 8, minY: -12, maxY: -4);
                        Word? piecesreqdWord = FindWordNextTo(word, words, numberWordDetectionBox);
                        if (piecesreqdWord is null)
                        {
                            throw new ExtractionException($"Failed to get ProjectNumber");
                        }
                        return piecesreqdWord.Text;
                    }
                }
            }
            throw new ExtractionException($"Failed to get ProjectNumber");
        }

        private static string ExtractProjectName(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> projectWords = (from Word word in words
                                           where word.Text.Contains("PROJECT", StringComparison.OrdinalIgnoreCase)
                                           select word).ToList();
                foreach (Word word in projectWords)
                {
                    DetectionBox projectWordDetectionBox = new DetectionBox(minX: -2, maxX: 120, minY: -10, maxY: 0);
                    List<Word> projectNameWords = FindWordsNextTo(word, words, projectWordDetectionBox);
                    if (projectNameWords.Count == 0)
                    {
                        throw new ExtractionException($"Failed to get ProjectName");
                    }

                    // Concatenate projectNameWords into a single string
                    String projectName = "";
                    foreach (Word w in projectNameWords)
                    {
                        if (projectName == "")
                        {
                            projectName = w.Text;
                        }
                        else
                        {
                            projectName += " " + w.Text;
                        }
                    }
                    return projectName;
                }
            }
            throw new ExtractionException($"Failed to get ProjectName");
        }

        private static string ExtractFileContentPieceMark(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> pieceWords = (from Word word in words
                                         where word.Text.Equals("PIECE", StringComparison.OrdinalIgnoreCase)
                                         select word).ToList();
                foreach (Word word in pieceWords)
                {
                    DetectionBox pieceWordDetectionBox = new DetectionBox(minX: 5, maxX: 15, minY: -1, maxY: 1);
                    Word? foundWord = FindWordNextTo(word, words, pieceWordDetectionBox);
                    if (foundWord is null)
                    {
                        continue;
                    }
                    if (string.Equals(foundWord.Text, "MARK", StringComparison.OrdinalIgnoreCase))
                    {
                        DetectionBox markWordDetectionBox = new DetectionBox(minX: -2, maxX: 15, minY: -10, maxY: -2);
                        Word? piecemarkWord = FindWordNextTo(word, words, markWordDetectionBox);
                        if (piecemarkWord is null)
                        {
                            throw new ExtractionException($"Failed to get FileContentPieceMark");
                        }
                        return piecemarkWord.Text;
                    }
                }
            }
            throw new ExtractionException($"Failed to get FileContentPieceMark");
        }

        private static string[]? ExtractControlNumbers(List<Page> pages)
        {
            List<Word> controlnumWords = new List<Word>();
            List<String> controlnumstrList = new List<String>();
            List<String> resultList = new List<String>();

            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<String> searchTerms = new List<String> { "CONTROL", "CTRL" };
                List<Word> controlWords = (from Word word in words
                                           where searchTerms.Any(searchTerm => searchTerm.Equals(word.Text, StringComparison.OrdinalIgnoreCase))
                                           select word).ToList();
                foreach (Word word in controlWords)
                {
                    DetectionBox controlWordDetectionBox = new DetectionBox(minX: 5, maxX: 35, minY: -1, maxY: 1);
                    Word? foundWord = FindWordNextTo(word, words, controlWordDetectionBox);
                    if (foundWord is null)
                    {
                        continue;
                    }

                    // Search method for when the word after "CONTROL" or "CTRL" is "NUMBER" or "NUMBER:"
                    searchTerms = new List<String> { "NUMBER", "NUMBER:" };
                    if (searchTerms.Any(searchTerm => searchTerm.Equals(foundWord.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        DetectionBox numberWordDetectionBox = new DetectionBox(minX: -4, maxX: 50, minY: -12, maxY: -2);
                        controlnumWords = FindWordsNextTo(word, words, numberWordDetectionBox);
                        if (controlnumWords.Count == 0)
                        {
                            return null;
                        }
                    }

                    // Search method for when the word after "CONTROL" or "CTRL" is "NO.", "NO:", or "NO.:"
                    searchTerms = new List<String> { "NO.", "NO:", "NO.:" };
                    if (searchTerms.Any(searchTerm => searchTerm.Equals(foundWord.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (word.BoundingBox.Left > 800 && word.BoundingBox.Left < 860)
                        {
                            DetectionBox numberWordDetectionBox2 = new DetectionBox(minX: -4, maxX: 30, minY: -15, maxY: -2);
                            controlnumWords = FindWordsNextTo(word, words, numberWordDetectionBox2);
                        }
                        else
                        {
                            DetectionBox numberWordDetectionBox3 = new DetectionBox(minX: -4, maxX: 150, minY: -40, maxY: -2);
                            controlnumWords = FindWordsNextTo(word, words, numberWordDetectionBox3);
                        }
                        if (controlnumWords.Count == 0)
                        {
                            return null;
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

        private static int ExtractPiecesRequired(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> piecesWords = (from Word word in words
                                          where word.Text.Equals("PIECES", StringComparison.OrdinalIgnoreCase)
                                          select word).ToList();
                foreach (Word word in piecesWords)
                {
                    DetectionBox piecesWordDetectionBox = new DetectionBox(minX: 5, maxX: 15, minY: -1, maxY: 1);
                    Word? foundWord = FindWordNextTo(word, words, piecesWordDetectionBox);
                    if (foundWord is null)
                    {
                        continue;
                    }
                    if (foundWord.Text.Contains("REQ", StringComparison.OrdinalIgnoreCase))
                    {
                        DetectionBox reqWordDetectionBox = new DetectionBox(minX: -2, maxX: 30, minY: -10, maxY: -4);
                        Word? piecesreqdWord = FindWordNextTo(word, words, reqWordDetectionBox);
                        if (piecesreqdWord is null)
                        {
                            throw new ExtractionException($"Failed to get PiecesRequired");
                        }
                        return int.Parse(piecesreqdWord.Text);
                    }
                }
            }
            throw new ExtractionException($"Failed to get PiecesRequired");
        }

        private static decimal ExtractWeight(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> weightWords = (from Word word in words
                                          where word.Text.Contains("WEIGHT", StringComparison.OrdinalIgnoreCase)
                                          select word).ToList();
                foreach (Word word in weightWords)
                {
                    DetectionBox weightWordDetectionBox = new DetectionBox(minX: -10, maxX: 30, minY: -20, maxY: -1);
                    Word? weightWord = FindWordNextTo(word, words, weightWordDetectionBox);
                    
                    // Remove any non-numeric characters except for decimal points and commas
                    string pattern = "[^0-9,.]";
                    string weightStr = Regex.Replace(weightWord.Text, pattern, "");

                    return decimal.Parse(weightStr);
                }
            }
            throw new ExtractionException($"Failed to get Weight");
        }

        private static string ExtractDesignNumber(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> designWords = (from Word word in words
                                          where word.Text.Equals("DESIGN:", StringComparison.OrdinalIgnoreCase)
                                          select word).ToList();
                foreach (Word word in designWords)
                {
                    DetectionBox designWordDetectionBox = new DetectionBox(minX: -2, maxX: 30, minY: -40, maxY: -4);
                    Word? foundWord = FindWordNextTo(word, words, designWordDetectionBox);
                    if (foundWord is null)
                    {
                        throw new ExtractionException($"Failed to get DesignNumber");
                    }
                    return foundWord.Text;
                }
            }
            throw new ExtractionException($"Failed to get DesignNumber");
        }

        // Holds the detection boundary values for find word(s) methods
        private record DetectionBox(int minX, int maxX, int minY, int maxY);

        // Finds one word among IEnumerable<Word> words relative to an anchorWord given specified bounds
        private static Word? FindWordNextTo(Word anchorWord, IEnumerable<Word> words, DetectionBox detectionBox)
        {
            return (from Word word in words
                    where (word.BoundingBox.Left - anchorWord.BoundingBox.Left > detectionBox.minX) &&
                      (word.BoundingBox.Left - anchorWord.BoundingBox.Left < detectionBox.maxX) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top > detectionBox.minY) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top < detectionBox.maxY)
                    select word).FirstOrDefault();
        }

        // Finds all words among IEnumerable<Word> words relative to an anchorWord given specified bounds
        private static List<Word> FindWordsNextTo(Word anchorWord, IEnumerable<Word> words, DetectionBox detectionBox)
        {
            return (from Word word in words
                    where (word.BoundingBox.Left - anchorWord.BoundingBox.Left > detectionBox.minX) &&
                      (word.BoundingBox.Left - anchorWord.BoundingBox.Left < detectionBox.maxX) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top > detectionBox.minY) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top < detectionBox.maxY)
                    select word).ToList();
        }
    }
}
