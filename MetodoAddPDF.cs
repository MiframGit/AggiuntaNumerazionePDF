using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging; // Manteniamo per il logging standard
using System;
using System.IO;

namespace AggiuntaNumerazionePDF
{
    // Definiamo un enum per il tipo di messaggio per l'UI
    public enum UIMessageType
    {
        Info,
        Error,
        Success,
        Warning
    }

    public class MetodoAddPDF
    {
        private readonly ILogger<MetodoAddPDF>? _logger;

        // Evento per inviare messaggi all'UI
        public event Action<string, UIMessageType>? LogMessageForUI;

        public MetodoAddPDF(ILogger<MetodoAddPDF>? logger = null)
        {
            _logger = logger;
        }

        // Metodo helper per inviare log sia al logger standard che all'evento UI
        private void SendLog(string message, LogLevel logLevel, UIMessageType uiType)
        {
            _logger?.Log(logLevel, message); // Log standard
            LogMessageForUI?.Invoke(message, uiType); // Notifica l'UI
        }


        public bool ProcessPdfFolder(string inputFolder, string outputFolder)
        {
            if (!Directory.Exists(inputFolder))
            {
                SendLog($"Cartella di Input inesistente: {inputFolder}", LogLevel.Error, UIMessageType.Error);
                return false;
            }

            if (!Directory.Exists(outputFolder))
            {
                try
                {
                    Directory.CreateDirectory(outputFolder);
                    SendLog($"Cartella di Output creata: {outputFolder}", LogLevel.Information, UIMessageType.Info);
                }
                catch (Exception ex)
                {
                    SendLog($"Errore nella creazione della cartella di Output: {ex.Message}", LogLevel.Error, UIMessageType.Error);
                    return false;
                }
            }

            string[] pdfFiles = Directory.GetFiles(inputFolder, "*.pdf");
            if (pdfFiles.Length == 0)
            {
                SendLog($"Nessun PDF Trovato nella cartella di input: {inputFolder}", LogLevel.Information, UIMessageType.Warning);
                return true; //Considero Successo se non c'è nulla da fare
            }

            bool allSuccessful = true;
            int processedCount = 0;
            int totalFiles = pdfFiles.Length;

            SendLog($"Trovati {totalFiles} PDF da processare.", LogLevel.Information, UIMessageType.Info);

            foreach (string inputPdfPath in pdfFiles)
            {
                string filename = Path.GetFileNameWithoutExtension(inputPdfPath);
                string outputPdfPath = Path.Combine(outputFolder, filename + "_watermark.pdf");

                processedCount++;
                SendLog($"Processando {processedCount}/{totalFiles}: {Path.GetFileName(inputPdfPath)}...", LogLevel.Information, UIMessageType.Info);

                bool success = AddNumberToFirstPage(inputPdfPath, outputPdfPath);
                if (!success)
                {
                    allSuccessful = false;
                    // I log specifici di errore sono già inviati da AddNumberToFirstPage
                }
                // I log specifici di successo sono già inviati da AddNumberToFirstPage
            }

            if (allSuccessful && pdfFiles.Length > 0)
            {
                SendLog($"Tutti i {pdfFiles.Length} PDF processati con successo.", LogLevel.Information, UIMessageType.Success);
            }
            else if (!allSuccessful)
            {
                SendLog("Processamento completato con alcuni errori. Controllare i messaggi precedenti.", LogLevel.Warning, UIMessageType.Warning);
            }

            return allSuccessful;
        }


        public bool AddNumberToFirstPage(string inputPdf, string outputPdf)
        {
            if (!inputPdf.ToLower().EndsWith(".pdf"))
            {
                SendLog($"Input non è un PDF: {Path.GetFileName(inputPdf)}", LogLevel.Error, UIMessageType.Error);
                return false;
            }

            try
            {
                string filename = Path.GetFileNameWithoutExtension(inputPdf);
                if (!int.TryParse(filename, out int number))
                {
                    SendLog($"Nome file non numerico: {filename}.pdf", LogLevel.Error, UIMessageType.Error);
                    return false;
                }

                if (File.Exists(outputPdf))
                {
                    SendLog($"File di output {Path.GetFileName(outputPdf)} esistente, sarà sovrascritto.", LogLevel.Warning, UIMessageType.Warning);
                }

                using (PdfReader reader = new PdfReader(inputPdf))
                {
                    using (FileStream fs = new FileStream(outputPdf, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (PdfStamper stamper = new PdfStamper(reader, fs))
                        {
                            if (reader.NumberOfPages == 0)
                            {
                                SendLog($"Il PDF {Path.GetFileName(inputPdf)} non ha pagine.", LogLevel.Warning, UIMessageType.Warning);
                                stamper.Close(); // Chiudi lo stamper prima di uscire
                                fs.Close();      // Chiudi il FileStream
                                reader.Close();  // Chiudi il reader
                                File.Delete(outputPdf); // Elimina il file vuoto creato
                                return false;
                            }
                            PdfContentByte cb = stamper.GetOverContent(1); // Assumiamo pagina 1 esista
                            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            cb.SetFontAndSize(bf, 12);

                            // Calcola la posizione per l'angolo in alto a destra
                            // Rectangle pageSize = reader.GetPageSize(1);
                            // float xPosition = pageSize.GetRight(20); // 20 punti dal bordo destro
                            // float yPosition = pageSize.GetTop(20);   // 20 punti dal bordo superiore

                            // Posizione fissa come prima
                            float xPosition = 550;
                            float yPosition = 800;


                            ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, new Phrase(number.ToString()), xPosition, yPosition, 0);
                        }
                    }
                }

                SendLog($"OK: {Path.GetFileName(inputPdf)} -> {Path.GetFileName(outputPdf)}", LogLevel.Information, UIMessageType.Success);
                return true;
            }
            catch (IOException ex)
            {
                SendLog($"Errore I/O su {Path.GetFileName(inputPdf)}: {ex.Message}", LogLevel.Error, UIMessageType.Error);
                return false;
            }
            catch (ArgumentException ex) when (ex.Message.Contains("PdfReader not opened with owner password"))
            {
                SendLog($"PDF protetto da password (richiede owner password): {Path.GetFileName(inputPdf)}. {ex.Message}", LogLevel.Error, UIMessageType.Error);
                return false;
            }
            catch (DocumentException ex)
            {
                SendLog($"Errore documento PDF {Path.GetFileName(inputPdf)}: {ex.Message}", LogLevel.Error, UIMessageType.Error);
                return false;
            }
            catch (Exception ex)
            {
                SendLog($"Errore inatteso su {Path.GetFileName(inputPdf)}: {ex.Message}", LogLevel.Error, UIMessageType.Error);
                return false;
            }
        }
    }
}