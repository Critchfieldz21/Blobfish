using PdfSharp.Snippets.Font;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace BackendLibrary
{
    internal class PdfTextExtractor
    {
        private PdfDocument pdf;

        public PdfTextExtractor(String pdfPath)
        {
            pdf = PdfDocument.Open(File.OpenRead(pdfPath));
        }

        public PdfTextExtractor(byte[] pdfBytes) 
        {
            pdf = PdfDocument.Open(pdfBytes);
        }

        public string ExtractText(string text)
        {
            IEnumerable<Page> pages = pdf.GetPages();

            switch (text)
            {
                case "FileContentPieceMark":
                    foreach (Page page in pages)
                    {
                        IEnumerable<Word> words = page.GetWords();
                        List<Word> pieceWords = (from Word word in words
                                                 where word.Text == "PIECE"
                                                 select word).ToList();
                        // foreach (Word word in words)
                        // {
                        //     Console.WriteLine($"Word: {word.Text}, Bounding Box: {word.BoundingBox}");
                        // }
                        foreach (Word word in pieceWords)
                        {
                            Word? foundWord = FindWordNextTo(word, words, 5, 15, -1, 1);
                            if (string.Equals(foundWord.Text, "MARK"))
                            {
                                Word? piecemarkWord = FindWordNextTo(word, words, -2, 2, -10, 0);
                                if (piecemarkWord is null)
                                {
                                    throw new NullReferenceException($"Failed to get {text}");
                                }
                                return piecemarkWord.Text;
                            }
                        }
                    }
                    throw new NullReferenceException($"Failed to get {text}");
                case "PiecesRequired":
                    foreach (Page page in pages)
                    {
                        IEnumerable<Word> words = page.GetWords();
                        List<Word> piecesWords = (from Word word in words
                                                  where word.Text == "PIECES"
                                                  select word).ToList();
                        foreach (Word word in piecesWords)
                        {
                            Word? foundWord = FindWordNextTo(word, words, 5, 15, -1, 1);
                            if (string.Equals(foundWord.Text, "REQ'D:"))
                            {
                                Word? piecesreqdWord = FindWordNextTo(word, words, -2, 30, -10, -4);
                                if (piecesreqdWord is null)
                                {
                                    throw new NullReferenceException($"Failed to get {text}");
                                }
                                return piecesreqdWord.Text;
                            }
                        }
                    }
                    throw new NullReferenceException($"Failed to get {text}");
                case "DesignNumber":
                    foreach (Page page in pages)
                    {
                        IEnumerable<Word> words = page.GetWords();
                        List<Word> designWords = (from Word word in words
                                                  where word.Text == "DESIGN:"
                                                  select word).ToList();
                        foreach (Word word in designWords)
                        {
                            Word? designnumberWord = FindWordNextTo(word, words, -2, 30, -40, -4);
                            if (designnumberWord is null)
                            {
                                throw new NullReferenceException($"Failed to get {text}");
                            }
                            return designnumberWord.Text;
                        }
                    }
                    throw new NullReferenceException($"Failed to get {text}");
                case "ProjectNumber":
                    foreach (Page page in pages)
                    {
                        IEnumerable<Word> words = page.GetWords();
                        List<Word> piecesWords = (from Word word in words
                                                  where word.Text == "JOB"
                                                  select word).ToList();
                        foreach (Word word in piecesWords)
                        {
                            Word? foundWord = FindWordNextTo(word, words, 5, 15, -1, 1);
                            if (string.Equals(foundWord.Text, "NO."))
                            {
                                Word? piecesreqdWord = FindWordNextTo(word, words, -4, 8, -12, -4);
                                if (piecesreqdWord is null)
                                {
                                    throw new NullReferenceException($"Failed to get {text}");
                                }
                                return piecesreqdWord.Text;
                            }
                        }
                    }
                    throw new NullReferenceException($"Failed to get {text}");
                case "Weight":
                    foreach (Page page in pages)
                    {
                        IEnumerable<Word> words = page.GetWords();
                        List<Word> weightWords = (from Word word in words
                                                  where word.Text == "WEIGHT:"
                                                  select word).ToList();
                        foreach (Word word in weightWords)
                        {
                            Word? weightWord = FindWordNextTo(word, words, -10, 30, -20, -1);
                            if (weightWord is null)
                            {
                                throw new NullReferenceException($"Failed to get {text}");
                            }
                            return weightWord.Text;
                        }
                    }
                    throw new NullReferenceException($"Failed to get {text}");
                case "ProjectName":
                    foreach (Page page in pages)
                    {
                        IEnumerable<Word> words = page.GetWords();
                        List<Word> projectWords = (from Word word in words
                                                where word.Text == "PROJECT:"
                                                select word).ToList();
                        foreach (Word word in projectWords)
                        {
                            List<Word> projectNameWords = FindWordsNextTo(word, words, -2, 120, -10, -1);
                            if (projectNameWords.Count == 0)
                            {
                                throw new NullReferenceException($"Failed to get {text}");
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
                    throw new NullReferenceException($"Failed to get {text}");
                default:
                    throw new ArgumentException($"Extraction for '{text}' is not implemented.");
            }
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
