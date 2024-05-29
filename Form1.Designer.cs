namespace Steganograf
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonGetFile = new System.Windows.Forms.Button();
            this.labelText = new System.Windows.Forms.Label();
            this.labelAudio = new System.Windows.Forms.Label();
            this.buttonText = new System.Windows.Forms.Button();
            this.buttonCoding = new System.Windows.Forms.Button();
            this.buttonDec = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonGetImg = new System.Windows.Forms.Button();
            this.labelImg = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(387, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Загрузите файл с текстом, который будет скрываться или файл с ключом";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 183);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Загрузите аудиофайл";
            // 
            // buttonGetFile
            // 
            this.buttonGetFile.AutoSize = true;
            this.buttonGetFile.Location = new System.Drawing.Point(13, 218);
            this.buttonGetFile.Name = "buttonGetFile";
            this.buttonGetFile.Size = new System.Drawing.Size(99, 23);
            this.buttonGetFile.TabIndex = 3;
            this.buttonGetFile.Text = "Загрузка файла";
            this.buttonGetFile.UseVisualStyleBackColor = true;
            this.buttonGetFile.Click += new System.EventHandler(this.buttonGetFile_Click);
            // 
            // labelText
            // 
            this.labelText.AutoSize = true;
            this.labelText.Location = new System.Drawing.Point(127, 53);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(0, 13);
            this.labelText.TabIndex = 4;
            // 
            // labelAudio
            // 
            this.labelAudio.AutoSize = true;
            this.labelAudio.Location = new System.Drawing.Point(124, 228);
            this.labelAudio.Name = "labelAudio";
            this.labelAudio.Size = new System.Drawing.Size(0, 13);
            this.labelAudio.TabIndex = 5;
            // 
            // buttonText
            // 
            this.buttonText.AutoSize = true;
            this.buttonText.Location = new System.Drawing.Point(16, 48);
            this.buttonText.Name = "buttonText";
            this.buttonText.Size = new System.Drawing.Size(85, 23);
            this.buttonText.TabIndex = 6;
            this.buttonText.Text = "Выбор файла";
            this.buttonText.UseVisualStyleBackColor = true;
            this.buttonText.Click += new System.EventHandler(this.buttonTextGet_Click);
            // 
            // buttonCoding
            // 
            this.buttonCoding.AutoSize = true;
            this.buttonCoding.Location = new System.Drawing.Point(13, 279);
            this.buttonCoding.Name = "buttonCoding";
            this.buttonCoding.Size = new System.Drawing.Size(89, 23);
            this.buttonCoding.TabIndex = 7;
            this.buttonCoding.Text = "Закодировать";
            this.buttonCoding.UseVisualStyleBackColor = true;
            this.buttonCoding.Click += new System.EventHandler(this.buttonCoding_Click);
            // 
            // buttonDec
            // 
            this.buttonDec.AutoSize = true;
            this.buttonDec.Location = new System.Drawing.Point(127, 279);
            this.buttonDec.Name = "buttonDec";
            this.buttonDec.Size = new System.Drawing.Size(91, 23);
            this.buttonDec.TabIndex = 8;
            this.buttonDec.Text = "Декодировать";
            this.buttonDec.UseVisualStyleBackColor = true;
            this.buttonDec.Click += new System.EventHandler(this.buttonDec_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Загрузите изображение";
            // 
            // buttonGetImg
            // 
            this.buttonGetImg.AutoSize = true;
            this.buttonGetImg.Location = new System.Drawing.Point(16, 126);
            this.buttonGetImg.Name = "buttonGetImg";
            this.buttonGetImg.Size = new System.Drawing.Size(99, 23);
            this.buttonGetImg.TabIndex = 10;
            this.buttonGetImg.Text = "Загрузка файла";
            this.buttonGetImg.UseVisualStyleBackColor = true;
            this.buttonGetImg.Click += new System.EventHandler(this.buttunGetImg_Click);
            // 
            // labelImg
            // 
            this.labelImg.AutoSize = true;
            this.labelImg.Location = new System.Drawing.Point(127, 136);
            this.labelImg.Name = "labelImg";
            this.labelImg.Size = new System.Drawing.Size(0, 13);
            this.labelImg.TabIndex = 11;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.labelImg);
            this.Controls.Add(this.buttonGetImg);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonDec);
            this.Controls.Add(this.buttonCoding);
            this.Controls.Add(this.buttonText);
            this.Controls.Add(this.labelAudio);
            this.Controls.Add(this.labelText);
            this.Controls.Add(this.buttonGetFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonGetFile;
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.Label labelAudio;
        private System.Windows.Forms.Button buttonText;
        private System.Windows.Forms.Button buttonCoding;
        private System.Windows.Forms.Button buttonDec;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonGetImg;
        private System.Windows.Forms.Label labelImg;
    }
}

