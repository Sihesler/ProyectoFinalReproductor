﻿using Newtonsoft.Json;
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

//
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace ProyectoFinalReproductorMP3
{
    public partial class Form1 : Form
    {
        int contador = -1;
        List<ClassCancion> listCancion = new List<ClassCancion>();


        public Form1()
        {
            InitializeComponent();
        }

        private static string NormalizeVideoId(string input)
        {
            string videoId = string.Empty;

            return YoutubeClient.TryParseVideoId(input, out videoId)
                ? videoId
                : input;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //nuevo cliente de Youtube
            var client = new YoutubeClient();
            //lee la dirección de youtube que le escribimos en el textbox
            var videoId = NormalizeVideoId(txtURL.Text); //normaliza
            var video = await client.GetVideoAsync(videoId); //descarga el video
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId); //descarga la informacion del video

            // Busca la mejor resolución en la que está disponible el video
            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality(); //descarga el video con la maxima calidad

            // Compone el nombre que tendrá el video en base a su título y extensión
            var fileExtension = streamInfo.Container.GetFileExtension(); //mira la extencion mp4
            var fileName = $"{video.Title}.{fileExtension}"; //agrega el titulo del video y la extencion mp4

            //TODO: Reemplazar los caractéres ilegales del nombre
            //fileName = RemoveIllegalFileNameChars(fileName);

            //Activa el timer para que el proceso funcione de forma asincrona
            tmrVideo.Enabled = true;

            // mensajes indicando que el video se está descargando
            txtMensaje.Text = "Descargando el video ... "; //mensaje 

            //TODO: se pude usar una barra de progreso para ver el avance
            //using (var progress = new ProgressBar())

            //Empieza la descarga
            await client.DownloadMediaStreamAsync(streamInfo, fileName); //empieza la descarga

            //Ya descargado se inicia la conversión a MP3
            var Convert = new NReco.VideoConverter.FFMpegConverter(); //convierte el video
            //Especificar la carpeta donde se van a guardar los archivos, recordar la \ del final
            String SaveMP3File = @"D:\DescargarMP3\" + fileName.Replace(".mp4", ".mp3"); //en donde se va a guardar el archivo
            //Guarda el archivo convertido en la ubicación indicada
            Convert.ConvertMedia(fileName, SaveMP3File, "mp3");

            //Si el checkbox de solo audio está chequeado, borrar el mp4 despues de la conversión
            if (ckbAudio.Checked) //funcion checkbox
                File.Delete(fileName); // si quemos solo el audio manda a borrar el mp4 y nos quedamos con el mp3


            //Indicar que se terminó la conversion
            txtMensaje.Text = "Video Convertido en MP3";
            tmrVideo.Enabled = false;
            txtMensaje.BackColor = Color.White;

            //TODO: Cargar el MP3 al reproductor o a la lista de reproducción
            //CargarMP3s();
            //Se puede incluir un checkbox para indicar que de una vez se reproduzca el MP3
            //if (ckbAutoPlay.Checked) 
            //  ReproducirMP3(SaveMP3File);

            //Declarar un objeto de Clase cliente
            ClassCancion cancionJson = new ClassCancion();
            string nombre = System.IO.Path.GetFileName(SaveMP3File);
            //Asignarle valores al cliente
            cancionJson.Nombre = nombre;
            cancionJson.Ubicacion = SaveMP3File;
            

            //Convertir el objeto en una cadena JSON
            string salida = JsonConvert.SerializeObject(cancionJson);
            //Guardar el archivo de texto, con extension Json
            FileStream stream = new FileStream("Cancion.json", FileMode.Append, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
            MessageBox.Show("La Canción: " + cancionJson.Nombre + " Se Registro Correctamente en la Biblioteca");

            writer.WriteLine(salida);
            writer.Close();

            return;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // reproductor.uiMode = "invisible";

            FileStream stream = new FileStream("Cancion.json", FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);
            while (reader.Peek() > -1)
            {
                string lectura = reader.ReadLine();
                ClassCancion cancionLeido = JsonConvert.DeserializeObject<ClassCancion>(lectura);
                listCancion.Add(cancionLeido);
                contador = contador + 1;
            }
            reader.Close();
            //Mostrar la lista de alquileres en el gridview
            dataGridView1.DataSource = listCancion;
            dataGridView1.Refresh();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            reproductor.URL = dataGridView1.CurrentCell.Value.ToString();
            label3.Text = System.IO.Path.GetFileName(reproductor.URL);
            reproductor.Ctlcontrols.play();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                reproductor.URL = openFileDialog1.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            reproductor.Ctlcontrols.play();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            reproductor.Ctlcontrols.pause();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            reproductor.Ctlcontrols.stop();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //Guardar el archivo de texto, con extension Json
            FileStream stream = new FileStream(@"C:\Users\DELL\Source\Repos\ProyectoFinalReproductor\ProyectoFinalReproductorMP3\ProyectoFinalReproductorMP3\bin\Debug\MisListas\" + textBox1.Text + ".json", FileMode.Append, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
            MessageBox.Show("La Lista: " + textBox1.Text + " se Creó Correctamente");
            // writer.WriteLine(salida);
            //writer.Close();
            foreach (string k in listBox2.Items)
            {
                //Declarar un objeto de Clase cliente
                ClassLista listaJson = new ClassLista();
                //Asignarle valores al cliente
                listaJson.Nombre = System.IO.Path.GetFileName(k);
                listaJson.Ubicacion = k;

                //Convertir el objeto en una cadena JSON
                string salida = JsonConvert.SerializeObject(listaJson);

                writer.WriteLine(salida);
                // musicas.Add(k);
                
            }
            writer.Close();
            listBox1.Items.Clear();
            textBox1.Clear();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            int ind;
            ind = dataGridView1.CurrentRow.Index;

            if (ind > 0)
            {
                dataGridView1.CurrentCell = dataGridView1[1, ind - 1];

                int inu;
                inu = dataGridView1.CurrentRow.Index;
                reproductor.URL = dataGridView1[1, inu].Value.ToString();
            }
            else
            {
                dataGridView1.CurrentCell = dataGridView1[1, contador];

                int inu;
                inu = dataGridView1.CurrentRow.Index;
                reproductor.URL = dataGridView1[1, inu].Value.ToString();




            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int ind;
            ind = dataGridView1.CurrentRow.Index;
            if (ind < contador)
            {
                dataGridView1.CurrentCell = dataGridView1[1, ind + 1];
                int inu;

                inu = dataGridView1.CurrentRow.Index;
                reproductor.URL = dataGridView1[1, inu].Value.ToString();
            }
            else
            {
                dataGridView1.CurrentCell = dataGridView1[1, 0];
                int inu;

                inu = dataGridView1.CurrentRow.Index;
                reproductor.URL = dataGridView1[1, inu].Value.ToString();


            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            listBox2.Items.Add(this.dataGridView1.CurrentRow.Cells[1].Value.ToString());
            listBox1.Items.Add(this.dataGridView1.CurrentRow.Cells[0].Value.ToString());            
        }
    }
}
