using System;
using System.Windows.Forms;


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
            }
        }

        private void buttunGetImg_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.jpg"; // file types, that will be allowed to upload
            dialog.Multiselect = false; // allow/deny user to upload more than one file at a time
            if (dialog.ShowDialog() == DialogResult.OK) // if user clicked OK
            {
                String path = dialog.FileName; // get name of file
                labelImg.Text = path;
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
            BinaryMessage message = new BinaryMessage(labelText.Text);

            ImgConteiner img = new ImgConteiner(labelImg.Text);
            img.EmbedMessage(message);
            img.CreateImg();
            img.ImageToByte();

            AudioWave wave = new AudioWave(labelAudio.Text);
            Key key = new Key();

            Systema stegosystem = new Systema(wave, img.binOrd, key);
            stegosystem.CreateStego();
            stegosystem.signal.CreateStegoaudio();

            labelAudio.Text = "Соранено в папку с проектом";
        }

        private void buttonDec_Click(object sender, EventArgs e)
        {
            AudioWave waveDec = new AudioWave(labelAudio.Text);
            BinaryMessageDec messageDec = new BinaryMessageDec();
            KeyDec keyDec = new KeyDec(labelText.Text);

            SystemDec systemDec = new SystemDec(waveDec, messageDec, keyDec);
            systemDec.ExtractStegomessage();

            ImgConteiner img = new ImgConteiner(labelImg.Text);
            img.CreateImgFromArray(systemDec.Message.Bits);

            ImgConteiner stegoImg = new ImgConteiner("C:\\Users\\vkise\\OneDrive\\Рабочий стол\\Диплом\\Миска_new.jpg");
            stegoImg.ExtractMessage(80);
            labelAudio.Text = "Соранено в папку с проектом";
        }


    }
}
