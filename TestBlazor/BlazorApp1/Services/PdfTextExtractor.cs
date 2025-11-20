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

            //// Uncomment to debug word extraction
            //foreach (Page page in pages)
            //{
            //    IEnumerable<Word> words = page.GetWords();
            //    IEnumerable<Annotation> annotations = page.GetAnnotations();

            //    foreach (Word word in words)
            //    {
            //        Console.WriteLine($"Word: {word.Text}, Bounding Box: {word.BoundingBox}");
            //    }
            //    foreach (Annotation annotation in annotations)
            //    {
            //        Console.WriteLine($"Annotation: {annotation.Content}");
            //    }
            //}

            TextGroup textGroup = new TextGroup();

            var cts = new CancellationTokenSource();
            List<Exception> exceptions = new();
            ParallelOptions opts = new() { CancellationToken = cts.Token };

            Parallel.Invoke(
                () =>
                {
                    try { textGroup.PageNames = ExtractPageNames(pages); logger.LogDebug("PageNames extracted"); }
                    catch (Exception ex) { lock (exceptions) exceptions.Add(ex); cts.Cancel(); }
                },
                () =>
                {
                    try { textGroup.ProjectNumber = ExtractProjectNumber(pages); logger.LogDebug("ProjectNumber extracted"); }
                    catch (Exception ex) { lock (exceptions) exceptions.Add(ex); cts.Cancel(); }
                },
                () =>
                {
                    try { textGroup.ProjectName = ExtractProjectName(pages); logger.LogDebug("ProjectName extracted"); }
                    catch (Exception ex) { lock (exceptions) exceptions.Add(ex); cts.Cancel(); }
                },
                () =>
                {
                    try { textGroup.FileContentPieceMark = ExtractFileContentPieceMark(pages); logger.LogDebug("FileContentPieceMark extracted"); }
                    catch (Exception ex) { lock (exceptions) exceptions.Add(ex); cts.Cancel(); }
                },
                () =>
                {
                    try { textGroup.ControlNumbers = ExtractControlNumbers(pages); logger.LogDebug("ControlNumbers extracted"); }
                    catch (Exception ex) { lock (exceptions) exceptions.Add(ex); cts.Cancel(); }
                },
                () =>
                {
                    try { textGroup.PiecesRequired = ExtractPiecesRequired(pages); logger.LogDebug("PiecesRequired extracted"); }
                    catch (Exception ex) { lock (exceptions) exceptions.Add(ex); cts.Cancel(); }
                },
                () =>
                {
                    try { textGroup.Weight = ExtractWeight(pages); logger.LogDebug("Weight extracted"); }
                    catch (Exception ex) { lock (exceptions) exceptions.Add(ex); cts.Cancel(); }
                },
                () =>
                {
                    try { textGroup.DesignNumber = ExtractDesignNumber(pages); logger.LogDebug("DesignNumber extracted"); }
                    catch (Exception ex) { lock (exceptions) exceptions.Add(ex); cts.Cancel(); }
                }
            );

            if (exceptions.Count > 0)
            {
                throw exceptions.First();
            }

            return textGroup;
        }

        public static string[] ExtractPageNames(List<Page> pages)
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
                Word? foundWord = FindWordNextTo(word, words, 20, 50, -1, 1);
                if (foundWord is null)
                {
                    if (word.BoundingBox.Left > 900)
                    {
                        continue;
                    }
                    List<Word> foundWords = FindWordsNextTo(word, words, -80, -20, -1, 1);
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
                Word? foundWord = FindWordNextTo(word, words, -80, -20, -1, 1);
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

            return "UnknownPage";
            //throw new ExtractionException($"Failed to get PageNames - Page {page.Number} has no valid view label");
        }
        public static string ExtractProjectNumber(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> jobWords = (from Word word in words
                                       where word.Text.Equals("JOB", StringComparison.OrdinalIgnoreCase)
                                       select word).ToList();
                foreach (Word word in jobWords)
                {
                    Word? foundWord = FindWordNextTo(word, words, 5, 15, -1, 1);
                    List<String> searchTerms = new List<String> { "NO.", "NO:", "NUMBER", "NUMBER:", "NUM", "NUM:" };
                    if (foundWord is null)
                    {
                        continue;
                    }
                    if (searchTerms.Any(searchTerm => searchTerm.Equals(foundWord.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        Word? piecesreqdWord = FindWordNextTo(word, words, -4, 8, -12, -4);
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

        public static string ExtractProjectName(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> projectWords = (from Word word in words
                                           where word.Text.Contains("PROJECT", StringComparison.OrdinalIgnoreCase)
                                           select word).ToList();
                foreach (Word word in projectWords)
                {
                    List<Word> projectNameWords = FindWordsNextTo(word, words, -2, 120, -10, 0);
                    if (projectNameWords.Count == 0)
                    {
                        throw new ExtractionException($"Failed to get ProjectName");
                    }
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

        public static string ExtractFileContentPieceMark(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> pieceWords = (from Word word in words
                                         where word.Text.Equals("PIECE", StringComparison.OrdinalIgnoreCase)
                                         select word).ToList();
                foreach (Word word in pieceWords)
                {
                    Word? foundWord = FindWordNextTo(word, words, 5, 15, -1, 1);
                    if (foundWord is null)
                    {
                        continue;
                    }
                    if (string.Equals(foundWord.Text, "MARK", StringComparison.OrdinalIgnoreCase))
                    {
                        Word? piecemarkWord = FindWordNextTo(word, words, -2, 15, -10, -2);
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

        public static string[]? ExtractControlNumbers(List<Page> pages)
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
                    Word? foundWord = FindWordNextTo(word, words, 5, 35, -1, 1);
                    if (foundWord is null)
                    {
                        continue;
                    }

                    // Search method for when the word after "CONTROL" or "CTRL" is "NUMBER" or "NUMBER:"
                    searchTerms = new List<String> { "NUMBER", "NUMBER:" };
                    if (searchTerms.Any(searchTerm => searchTerm.Equals(foundWord.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        controlnumWords = FindWordsNextTo(word, words, -4, 50, -12, -2);
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
                            controlnumWords = FindWordsNextTo(word, words, -4, 30, -15, -2);
                        }
                        else
                        {
                            controlnumWords = FindWordsNextTo(word, words, -4, 150, -40, -2);
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

        public static int ExtractPiecesRequired(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> piecesWords = (from Word word in words
                                          where word.Text.Equals("PIECES", StringComparison.OrdinalIgnoreCase)
                                          select word).ToList();
                foreach (Word word in piecesWords)
                {
                    Word? foundWord = FindWordNextTo(word, words, 5, 15, -1, 1);
                    if (foundWord is null)
                    {
                        continue;
                    }
                    if (foundWord.Text.Contains("REQ", StringComparison.OrdinalIgnoreCase))
                    {
                        Word? piecesreqdWord = FindWordNextTo(word, words, -2, 30, -10, -4);
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

        public static decimal ExtractWeight(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> weightWords = (from Word word in words
                                          where word.Text.Contains("WEIGHT", StringComparison.OrdinalIgnoreCase)
                                          select word).ToList();
                foreach (Word word in weightWords)
                {
                    Word? weightWord = FindWordNextTo(word, words, -10, 30, -20, -1);
                    if (weightWord is null)
                    {
                        throw new ExtractionException($"Failed to get Weight");
                    }
                    // Remove any non-numeric characters except for decimal points and commas
                    string pattern = "[^0-9,.]";
                    string weightStr = Regex.Replace(weightWord.Text, pattern, "");

                    return decimal.Parse(weightStr);
                }
            }
            throw new ExtractionException($"Failed to get Weight");
        }

        public static string ExtractDesignNumber(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                IEnumerable<Word> words = page.GetWords();
                List<Word> designWords = (from Word word in words
                                          where word.Text.Equals("DESIGN:", StringComparison.OrdinalIgnoreCase)
                                          select word).ToList();
                foreach (Word word in designWords)
                {
                    Word? designnumberWord = FindWordNextTo(word, words, -2, 30, -40, -4);
                    if (designnumberWord is null)
                    {
                        throw new ExtractionException($"Failed to get DesignNumber");
                    }
                    return designnumberWord.Text;
                }
            }
            throw new ExtractionException($"Failed to get DesignNumber");
        }

        // Finds one word among IEnumerable<Word> words relative to an anchorWord given specified bounds
        public static Word? FindWordNextTo(Word anchorWord, IEnumerable<Word> words, double minX, double maxX, double minY, double maxY)
        {
            return (from Word word in words
                    where (word.BoundingBox.Left - anchorWord.BoundingBox.Left > minX) &&
                      (word.BoundingBox.Left - anchorWord.BoundingBox.Left < maxX) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top > minY) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top < maxY)
                    select word).FirstOrDefault();
        }

        // Finds all words among IEnumerable<Word> words relative to an anchorWord given specified bounds
        public static List<Word> FindWordsNextTo(Word anchorWord, IEnumerable<Word> words, double minX, double maxX, double minY, double maxY)
        {
            return (from Word word in words
                    where (word.BoundingBox.Left - anchorWord.BoundingBox.Left > minX) &&
                      (word.BoundingBox.Left - anchorWord.BoundingBox.Left < maxX) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top > minY) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top < maxY)
                    select word).ToList();
        }
    }
}
