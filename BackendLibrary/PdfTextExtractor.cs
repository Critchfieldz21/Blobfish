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

        public string? ExtractText(string text)
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
                        //foreach (Word word in pieceWords)
                        //{
                        //    Console.WriteLine($"Word: {word.Text}, Bounding Box: {word.BoundingBox}");
                        //}
                        foreach (Word word in pieceWords)
                        {
                            Word? foundWord = FindWordNextTo(word, words, 5, 15, -1, 1);
                            if (string.Equals(foundWord.Text, "MARK"))
                            {
                                Word? piecemarkWord = FindWordNextTo(word, words, -2, 2, -10, 0);
                                if (piecemarkWord is null)
                                {
                                    return null;
                                }
                                return piecemarkWord.Text;
                            }
                        }
                    }
                    break;
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
                                    return null;
                                }
                                return piecesreqdWord.Text;
                            }
                        }
                    }
                    break;

                case "DesignNumber":
                    foreach (Page page in pages)
                    {
                        IEnumerable<Word> words = page.GetWords();
                        List<Word> designWords = (from Word word in words
                                                  where word.Text == "DESIGN:"
                                                  select word).ToList();
                        foreach (Word word in designWords)
                        {
                            Word? designnumberWord = FindWordNextTo(word, words, -2, 30, -10, -4);
                            if (designnumberWord is null)
                            {
                                return null;
                            }
                            return designnumberWord.Text;
                        }
                    }
                   
                    break;

                case "": 
                    break;
            }
            
            return null;
        }
        
        // Finds one word among IEnumerable<Word> words relative to an anchorWord given specified bounds
        public Word? FindWordNextTo(Word anchorWord, IEnumerable<Word> words, double minX, double maxX, double minY, double maxY)
        {
            Word? foundWord = (from Word word in words
                              where (word.BoundingBox.Left - anchorWord.BoundingBox.Left > minX) &&
                                    (word.BoundingBox.Left - anchorWord.BoundingBox.Left < maxX) &&
                                    (word.BoundingBox.Top - anchorWord.BoundingBox.Top > minY) &&
                                    (word.BoundingBox.Top - anchorWord.BoundingBox.Top < maxY)
                              select word).FirstOrDefault();
            return foundWord;
        }
    }
}
