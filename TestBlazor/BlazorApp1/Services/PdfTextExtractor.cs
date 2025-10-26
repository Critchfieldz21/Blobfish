using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace BackendLibrary
{
    internal class PdfTextExtractor
    {
        private PdfDocument pdf;
        private IEnumerable<Page> pages;

        public PdfTextExtractor(String pdfPath)
        {
            pdf = PdfDocument.Open(File.OpenRead(pdfPath));
            pages = pdf.GetPages();
        }

        public PdfTextExtractor(byte[] pdfBytes)
        {
            pdf = PdfDocument.Open(pdfBytes);
            pages = pdf.GetPages();

            //// Uncomment to debug word extraction
            //foreach (Page page in pages)
            //{
            //    IEnumerable<Word> words = page.GetWords();
            //    foreach (Word word in words)
            //    {
            //        Console.WriteLine($"Word: {word.Text}, Bounding Box: {word.BoundingBox}");
            //    }
            //}
        }

        public string ExtractProjectNumber()
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
                            throw new NullReferenceException($"Failed to get ProjectNumber");
                        }
                        return piecesreqdWord.Text;
                    }
                }
            }
            throw new NullReferenceException($"Failed to get ProjectNumber");
        }

        public string ExtractProjectName()
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
                        throw new NullReferenceException($"Failed to get ProjectName");
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
            throw new NullReferenceException($"Failed to get ProjectName");
        }

        public string ExtractFileContentPieceMark()
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
                            throw new NullReferenceException($"Failed to get FileContentPieceMark");
                        }
                        return piecemarkWord.Text;
                    }
                }
            }
            throw new NullReferenceException($"Failed to get FileContentPieceMark");
        }

        public string[]? ExtractControlNumbers()
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
                    searchTerms = new List<String> { "NUMBER", "NUMBER:" };
                    if (searchTerms.Any(searchTerm => searchTerm.Equals(foundWord.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        controlnumWords = FindWordsNextTo(word, words, -4, 50, -12, -2);
                        if (controlnumWords.Count == 0)
                        {
                            return null;
                        }
                    }
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
                        controlnumstrList.AddRange(controlnumWords.Select(word => word.Text));
                        controlnumstrList.ForEach(str => resultList.AddRange(str.Split(',', StringSplitOptions.RemoveEmptyEntries)));

                        return resultList.Distinct().ToArray();
                    }
                }
            }
            return null;
        }

        public int ExtractPiecesRequired()
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
                            throw new NullReferenceException($"Failed to get PiecesRequired");
                        }
                        return int.Parse(piecesreqdWord.Text);
                    }
                }
            }
            throw new NullReferenceException($"Failed to get PiecesRequired");
        }

        public decimal ExtractWeight()
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
                        throw new NullReferenceException($"Failed to get Weight");
                    }
                    return decimal.Parse(weightWord.Text);
                }
            }
            throw new NullReferenceException($"Failed to get Weight");
        }

        public string ExtractDesignNumber()
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
                        throw new NullReferenceException($"Failed to get DesignNumber");
                    }
                    return designnumberWord.Text;
                }
            }
            throw new NullReferenceException($"Failed to get DesignNumber");
        }

        // Finds one word among IEnumerable<Word> words relative to an anchorWord given specified bounds
        public Word? FindWordNextTo(Word anchorWord, IEnumerable<Word> words, double minX, double maxX, double minY, double maxY)
        {
            return (from Word word in words
                    where (word.BoundingBox.Left - anchorWord.BoundingBox.Left > minX) &&
                      (word.BoundingBox.Left - anchorWord.BoundingBox.Left < maxX) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top > minY) &&
                      (word.BoundingBox.Top - anchorWord.BoundingBox.Top < maxY)
                    select word).FirstOrDefault();
        }

        // Finds all words among IEnumerable<Word> words relative to an anchorWord given specified bounds
        public List<Word> FindWordsNextTo(Word anchorWord, IEnumerable<Word> words, double minX, double maxX, double minY, double maxY)
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
