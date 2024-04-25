using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Steganograf
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void buttonGetFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Audio files | *.wav"; // file types, that will be allowed to upload
            dialog.Multiselect = false; // allow/deny user to upload more than one file at a time
            if (dialog.ShowDialog() == DialogResult.OK) // if user clicked OK
            {
                String path = dialog.FileName; // get name of file
                labelAudio.Text = path;
                //using (WaveFileReader reader = new WaveFileReader(path)) // do anything you want, e.g. read it
                //{
                //    int chennals = reader.WaveFormat.Channels;
                //    labelAudio.Text = chennals.ToString();


                //}
            }
        }

        private void buttonTextGet_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text files | *.txt"; // file types, that will be allowed to upload
            dialog.Multiselect = false; // allow/deny user to upload more than one file at a time
            if (dialog.ShowDialog() == DialogResult.OK) // if user clicked OK
            {
                String path = dialog.FileName; // get name of file
                labelText.Text = path;
            }
        }

        private void buttonCoding_Click(object sender, EventArgs e)
        {
            Wave signal = new Wave(labelAudio.Text);
            BinaryMessage message = new BinaryMessage(labelText.Text);
            Key key = new Key();

            Systema stegosystem = new Systema(signal, message, key);
            stegosystem.CreateStego();
            stegosystem.signal.CreateStegoaudio(stegosystem.key);

            labelAudio.Text = "Соранено в папку с проектом";
        }

        private void buttonDec_Click(object sender, EventArgs e)
        {
            Wave signal = new Wave(labelAudio.Text);
            BinaryMessageDec message = new BinaryMessageDec();
            KeyDec key = new KeyDec(labelText.Text);

            SystemDec stegosystem = new SystemDec(signal, message, key);
            stegosystem.ExtractStegomessage();
            labelAudio.Text = "Соранено в папку с проектом";

        }
    }
}
