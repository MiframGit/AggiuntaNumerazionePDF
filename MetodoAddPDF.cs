using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AggiuntaNumerazionePDF
{
    public class MetodoAddPDF
    {
        private readonly ILogger<MetodoAddPDF>? _logger;

        public MetodoAddPDF(ILogger<MetodoAddPDF>? logger = null)
        {
            _logger = logger;
        }

        public bool AddNumberToFirstPage(string inputPdf, string outputPdf)
        {
            if (!inputPdf.ToLower().EndsWith(".pdf"))
            {
                _logger?.LogError("Input file does not have a .pdf extension.");
                Console.WriteLine("Input file does not have a .pdf extension.");
                return false;
            }

            try
            {
                // 1. Extract the number from the filename (Robust Approach)
                string filename = Path.GetFileNameWithoutExtension(inputPdf);
                if (!int.TryParse(filename, out int number))
                {
                    _logger?.LogError($"Could not extract a valid number from filename: {filename}");
                    Console.WriteLine($"Could not extract a valid number from filename: {filename}");
                    return false; // Or throw an exception
                }

                // 2. Check if the output file exists. Give an opportunity to overwrite
                if (File.Exists(outputPdf))
                {
                    Console.WriteLine($"Output file {outputPdf} already exists. Overwrite? (y/n)");
                    string response = Console.ReadLine()?.ToLower();
                    if (response != "y")
                    {
                        Console.WriteLine("Operation cancelled.");
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
                                //bf.Dispose(); // Not strictly needed but good practice for larger fonts
                            }
                        }
                    }
                }

                _logger?.LogInformation($"Successfully added number {number} to PDF: {inputPdf}");
                Console.WriteLine($"Successfully added number {number} to PDF: {inputPdf}");
                return true; // Indicate success
            }
            catch (IOException ex)
            {
                _logger?.LogError($"Error accessing file: {ex.Message}");
                Console.WriteLine($"Error accessing file: {ex.Message}");
                return false;
            }
            catch (DocumentException ex) // Catch iTextSharp specific errors
            {
                _logger?.LogError($"Error processing PDF: {ex.Message}");
                Console.WriteLine($"Error processing PDF: {ex.Message}");
                return false;
            }
            catch (Exception ex) // Catch any other exceptions
            {
                _logger?.LogError($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
            openFileDialog.Title = "Seleziona un file PDF";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtInputPath.Text = openFileDialog.FileName;
            }
        }

        private void btnSelectOutputFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtOutputPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            string inputPdfPath = txtInputPath.Text;
            string outputPath = txtOutputPath.Text;

            if (string.IsNullOrEmpty(inputPdfPath) || string.IsNullOrEmpty(outputPath))
            {
                MessageBox.Show("Please select both input PDF file and output folder.");
                return;
            }

            // Create the output file path
            string outputFileName = System.IO.Path.GetFileNameWithoutExtension(inputPdfPath) + "_watermark.pdf";
            string outputPdfPath = System.IO.Path.Combine(outputPath, outputFileName);

            // Use the MetodoAddPDF class
            MetodoAddPDF pdfProcessor = new MetodoAddPDF(_logger); // Pass the logger if you're using it

            // Call the PDF processing method
            bool success = pdfProcessor.AddNumberToFirstPage(inputPdfPath, outputPdfPath);

            if (success)
            {
                MessageBox.Show($"PDF processato correttamente. PDF salvato su: {outputPdfPath}");
                _logger?.LogInformation($"PDF processed successfully: {outputPdfPath}");
            }
            else
            {
                MessageBox.Show("Processo fallito. Guardare i logs per i dettagli.");
                _logger?.LogError("PDF processing failed.");
            }
        }
    }

}
