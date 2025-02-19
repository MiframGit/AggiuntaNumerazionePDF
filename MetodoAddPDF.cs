using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace AggiuntaNumerazionePDF
{
    public class MetodoAddPDF
    {
        private readonly ILogger<MetodoAddPDF>? _logger;

        public MetodoAddPDF(ILogger<MetodoAddPDF>? logger = null)
        {
            _logger = logger;
        }

        public bool ProcessPdfFolder(string inputFolder, string outputFolder)
        {
            if (!Directory.Exists(inputFolder))
            {
                _logger?.LogError($"Cartella di Input inesistente: {inputFolder}");
                Console.WriteLine($"Cartella di Input inesistente: {inputFolder}");
                return false;
            }

            if (!Directory.Exists(outputFolder))
            {
                try
                {
                    Directory.CreateDirectory(outputFolder);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Errore nella creazione della cartella di Output: {ex.Message}");
                    Console.WriteLine($"Errore nella creazione della cartella di Output: {ex.Message}");
                    return false;
                }
            }

            string[] pdfFiles = Directory.GetFiles(inputFolder, "*.pdf");
            if (pdfFiles.Length == 0)
            {
                _logger?.LogInformation($"Nessun PDF Trovato nella cartella di input: {inputFolder}");
                Console.WriteLine($"Nessun PDF Trovato nella cartella di input: {inputFolder}");
                return true; //Considero Successo se non c'è nulla da fare
            }

            bool allSuccessful = true;

            foreach (string inputPdfPath in pdfFiles)
            {
                string filename = Path.GetFileNameWithoutExtension(inputPdfPath);
                string outputPdfPath = Path.Combine(outputFolder, filename + "_watermark.pdf");

                bool success = AddNumberToFirstPage(inputPdfPath, outputPdfPath);
                if (!success)
                {
                    allSuccessful = false;
                    _logger?.LogError($"Fallimento processamento PDF: {inputPdfPath}");
                    Console.WriteLine($"Fallimento processamento PDF: {inputPdfPath}");
                }
                else
                {
                    _logger?.LogInformation($"PDF processato correttamente: {inputPdfPath} -> {outputPdfPath}");
                    Console.WriteLine($"PDF processato correttamente: {inputPdfPath} -> {outputPdfPath}");
                }
            }

            return allSuccessful;
        }


        public bool AddNumberToFirstPage(string inputPdf, string outputPdf)
        {
            if (!inputPdf.ToLower().EndsWith(".pdf"))
            {
                _logger?.LogError("File Input non ha estensione .pdf");
                Console.WriteLine("File Input non ha estensione .pdf");
                return false;
            }

            try
            {
                // 1. Estrai il numero dal nome del file
                string filename = Path.GetFileNameWithoutExtension(inputPdf);
                if (!int.TryParse(filename, out int number))
                {
                    _logger?.LogError($"Non sono riuscito ad estrarre un numero valido dal filename: {filename}");
                    Console.WriteLine($"Non sono riuscito ad estrarre un numero valido dal filename: {filename}");
                    return false; // O Lancia un'eccezione
                }

                // 2. Check if the output file exists. Give an opportunity to overwrite
                if (File.Exists(outputPdf))
                {
                    Console.WriteLine($"Output file {outputPdf} esiste già. Sovrascrivere? (y/n)");
                    string response = Console.ReadLine()?.ToLower();
                    if (response != "y")
                    {
                        Console.WriteLine("Operazione cancellata.");
                        return false;
                    }
                }

                // 3. Open the PDF file
                using (PdfReader reader = new PdfReader(inputPdf))
                {
                    using (FileStream fs = new FileStream(outputPdf, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (PdfStamper stamper = new PdfStamper(reader, fs))
                        {
                            // 4. Get the first page
                            PdfContentByte cb = stamper.GetOverContent(1);

                            // 5. Set the font and size
                            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            try
                            {
                                cb.SetFontAndSize(bf, 12);

                                // 6. Set the position of the text (adjust these values)
                                float xPosition = 550;
                                float yPosition = 800;

                                ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, new Phrase(number.ToString()), xPosition, yPosition, 0);
                            }
                            finally
                            {
                                //bf.Dispose(); // Non sempre necessario ma è buona pratica farlo se si usano font grandi
                            }
                        }
                    }
                }

                _logger?.LogInformation($"PDF processato correttamente: {inputPdf}");
                Console.WriteLine($"PDF processato correttamente: {inputPdf}");
                return true; // Indica successo
            }
            catch (IOException ex)
            {
                _logger?.LogError($"Errore accesso file: {ex.Message}");
                Console.WriteLine($"Errore accesso file: {ex.Message}");
                return false;
            }
            catch (DocumentException ex) // Catch iTextSharp specific errors
            {
                _logger?.LogError($"Errore processamento PDF: {ex.Message}");
                Console.WriteLine($"Errore processamento PDF: {ex.Message}");
                return false;
            }
            catch (Exception ex) // Catch any other exceptions
            {
                _logger?.LogError($"Errore inatteso: {ex.Message}");
                Console.WriteLine($"Errore inatteso: {ex.Message}");
                return false;
            }
        }
    }
        public partial class CambioPDF : Form
        {
            private readonly ILogger<MetodoAddPDF>? _logger; // Logger opzionale

            public CambioPDF(ILogger<MetodoAddPDF>? logger = null)
            {
                InitializeComponent();
                _logger = logger;
            }
            private void Form1_Load(object sender, EventArgs e)
            {
                string suca = "panzoneConPizzetto";
            }

            private void btnSelectInput_Click(object sender, EventArgs e)
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.Description = "Seleziona la cartella Input";
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    txtInputPath.Text = folderBrowserDialog.SelectedPath;
                }
            }

            private void btnSelectOutputFolder_Click(object sender, EventArgs e)
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.Description = "Seleziona la cartella Output";
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutputPath.Text = folderBrowserDialog.SelectedPath;
                }
            }

            private async void btnProcess_Click(object sender, EventArgs e)
            {
                string inputFolderPath = txtInputPath.Text;
                string outputFolderPath = txtOutputPath.Text;

                if (string.IsNullOrEmpty(inputFolderPath) || string.IsNullOrEmpty(outputFolderPath))
                {
                    MessageBox.Show("Devi selezionare sia la cartella di Input che quella di Output.");
                    return;
                }

                // Visualizza la ProgressBar
                progressBar.Visible = true;

                // Disabilita il pulsante per evitare click multipli durante l'elaborazione
                btnProcess.Enabled = false;

                // Use the MetodoAddPDF class
                MetodoAddPDF pdfProcessor = new MetodoAddPDF(_logger); // Pass the logger if you're using it

                // Esegui il processamento in modo asincrono
                bool success = await Task.Run(() => pdfProcessor.ProcessPdfFolder(inputFolderPath, outputFolderPath));

                // Dopo aver completato:
                progressBar.Visible = false;
                btnProcess.Enabled = true;

                if (success)
                {
                    MessageBox.Show($"PDF processati correttamente. PDF salvati su: {outputFolderPath}");
                    _logger?.LogInformation($"PDFs processed successfully: {outputFolderPath}");
                }
                else
                {
                    MessageBox.Show("Uno o più processamenti PDF sono falliti. Guardare i logs per i dettagli.");
                    _logger?.LogError("Processamento PDF Fallito.");
                }
            }
        }

    }
