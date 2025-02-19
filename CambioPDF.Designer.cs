namespace AggiuntaNumerazionePDF
{
    public partial class CambioPDF

    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtInputPath = new TextBox();
            txtOutputPath = new TextBox();
            btnSelectInput = new Button();
            btnSelectOutputFolder = new Button();
            btnProcess = new Button();
            SuspendLayout();
            // 
            // txtInputPath
            // 
            txtInputPath.Location = new Point(12, 14);
            txtInputPath.Name = "txtInputPath";
            txtInputPath.Size = new Size(300, 25);
            txtInputPath.TabIndex = 0;
            // 
            // txtOutputPath
            // 
            txtOutputPath.Location = new Point(12, 46);
            txtOutputPath.Name = "txtOutputPath";
            txtOutputPath.Size = new Size(300, 25);
            txtOutputPath.TabIndex = 1;
            // 
            // btnSelectInput
            // 
            btnSelectInput.Location = new Point(318, 12);
            btnSelectInput.Name = "btnSelectInput";
            btnSelectInput.Size = new Size(182, 26);
            btnSelectInput.TabIndex = 2;
            btnSelectInput.Text = "Seleziona cartella d'ingresso";
            btnSelectInput.UseVisualStyleBackColor = true;
            btnSelectInput.Click += btnSelectInput_Click;
            // 
            // btnSelectOutputFolder
            // 
            btnSelectOutputFolder.Location = new Point(320, 46);
            btnSelectOutputFolder.Name = "btnSelectOutputFolder";
            btnSelectOutputFolder.Size = new Size(182, 26);
            btnSelectOutputFolder.TabIndex = 3;
            btnSelectOutputFolder.Text = "Seleziona cartella d'uscita";
            btnSelectOutputFolder.UseVisualStyleBackColor = true;
            btnSelectOutputFolder.Click += btnSelectOutputFolder_Click;
            // 
            // btnProcess
            // 
            btnProcess.Location = new Point(61, 77);
            btnProcess.Name = "btnProcess";
            btnProcess.Size = new Size(200, 34);
            btnProcess.TabIndex = 4;
            btnProcess.Text = "Trasferimento PDF";
            btnProcess.UseVisualStyleBackColor = true;
            btnProcess.Click += btnProcess_Click;
            // 
            // CambioPDF
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(513, 126);
            Controls.Add(btnProcess);
            Controls.Add(btnSelectOutputFolder);
            Controls.Add(btnSelectInput);
            Controls.Add(txtOutputPath);
            Controls.Add(txtInputPath);
            Name = "CambioPDF";
            Text = "CambioPDF";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.Button btnSelectInput;
        private System.Windows.Forms.Button btnSelectOutputFolder;
        private System.Windows.Forms.Button btnProcess;
    }

}
