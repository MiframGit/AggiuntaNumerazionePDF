using Microsoft.Extensions.Logging;
using System;
using System.Drawing; // Per Color
using System.Threading.Tasks; // Per Task
using System.Windows.Forms;
using System.IO; // Per Path.Combine, Directory.Exists

namespace AggiuntaNumerazionePDF
{
    public partial class CambioPDF : Form
    {
        private readonly ILogger<MetodoAddPDF>? _logger;
        private MetodoAddPDF? _pdfProcessor; // Per gestire l'iscrizione/disiscrizione

        public CambioPDF(ILogger<MetodoAddPDF>? logger = null)
        {
            InitializeComponent();
            _logger = logger;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Eventuali inizializzazioni al caricamento del form
            lblStatusMessage.Text = "Selezionare le cartelle e premere il pulsante.";
        }

        private void btnSelectInput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Seleziona la cartella Input contenente i PDF";
                folderBrowserDialog.ShowNewFolderButton = false;
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    txtInputPath.Text = folderBrowserDialog.SelectedPath;
                    lblStatusMessage.Text = "Cartella di input selezionata.";
                    lblStatusMessage.ForeColor = SystemColors.ControlText;
                }
            }
        }

        private void btnSelectOutputFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Seleziona la cartella Output dove salvare i PDF modificati";
                folderBrowserDialog.ShowNewFolderButton = true;
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutputPath.Text = folderBrowserDialog.SelectedPath;
                    lblStatusMessage.Text = "Cartella di output selezionata.";
                    lblStatusMessage.ForeColor = SystemColors.ControlText;
                }
            }
        }

        // Gestore per l'evento LogMessageForUI
        private void PdfProcessor_LogMessageForUI(string message, UIMessageType type)
        {
            if (lblStatusMessage.InvokeRequired)
            {
                lblStatusMessage.Invoke(new Action(() => UpdateStatusLabel(message, type)));
            }
            else
            {
                UpdateStatusLabel(message, type);
            }
        }

        private void UpdateStatusLabel(string message, UIMessageType type)
        {
            lblStatusMessage.Text = message;
            switch (type)
            {
                case UIMessageType.Error:
                    lblStatusMessage.ForeColor = Color.Red;
                    break;
                case UIMessageType.Success:
                    lblStatusMessage.ForeColor = Color.DarkGreen;
                    break;
                case UIMessageType.Warning:
                    lblStatusMessage.ForeColor = Color.OrangeRed;
                    break;
                case UIMessageType.Info:
                default:
                    lblStatusMessage.ForeColor = SystemColors.ControlText;
                    break;
            }
        }

        private async void btnProcess_Click(object sender, EventArgs e)
        {
            string inputFolderPath = txtInputPath.Text;
            string outputFolderPath = txtOutputPath.Text;

            if (string.IsNullOrWhiteSpace(inputFolderPath) || !Directory.Exists(inputFolderPath))
            {
                MessageBox.Show("La cartella di Input non è valida o non è stata selezionata.", "Errore Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtInputPath.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(outputFolderPath))
            {
                MessageBox.Show("La cartella di Output non è valida o non è stata selezionata.", "Errore Output", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtOutputPath.Focus();
                return;
            }

            // Controllo che input e output non siano la stessa cartella
            if (Path.GetFullPath(inputFolderPath).Equals(Path.GetFullPath(outputFolderPath), StringComparison.InvariantCultureIgnoreCase))
            {
                MessageBox.Show("La cartella di Input e di Output non possono essere la stessa.", "Errore Cartelle", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UpdateStatusLabel("Inizio elaborazione...", UIMessageType.Info);
            progressBar.Visible = true;
            btnProcess.Enabled = false;
            btnSelectInput.Enabled = false;
            btnSelectOutputFolder.Enabled = false;

            _pdfProcessor = new MetodoAddPDF(_logger);
            _pdfProcessor.LogMessageForUI += PdfProcessor_LogMessageForUI;

            bool success = false;
            try
            {
                success = await Task.Run(() => _pdfProcessor.ProcessPdfFolder(inputFolderPath, outputFolderPath));
            }
            catch (Exception ex)
            {
                // Questo catch serve per errori imprevisti nel Task.Run o nella gestione dell'evento.
                // Gli errori di elaborazione PDF sono già loggati da MetodoAddPDF.
                UpdateStatusLabel($"Errore critico durante l'operazione: {ex.Message}", UIMessageType.Error);
                _logger?.LogCritical(ex, "Errore critico nel task di elaborazione PDF.");
                success = false;
            }
            finally
            {
                if (_pdfProcessor != null)
                {
                    _pdfProcessor.LogMessageForUI -= PdfProcessor_LogMessageForUI;
                    _pdfProcessor = null; // Rilascia il riferimento
                }

                progressBar.Visible = false;
                btnProcess.Enabled = true;
                btnSelectInput.Enabled = true;
                btnSelectOutputFolder.Enabled = true;

                if (success)
                {
                    // Il messaggio di successo finale è già stato inviato da ProcessPdfFolder
                    // UpdateStatusLabel($"Operazione completata con successo. PDF salvati in: {outputFolderPath}", UIMessageType.Success);
                    MessageBox.Show($"Operazione completata. Controllare i messaggi di stato sotto per i dettagli.\nI file elaborati sono in: {outputFolderPath}", "Completato", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Il messaggio di errore/warning finale è già stato inviato
                    MessageBox.Show("Operazione completata con errori o avvisi. Controllare i messaggi di stato sotto e i log (se configurati) per i dettagli.", "Errori/Avvisi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}