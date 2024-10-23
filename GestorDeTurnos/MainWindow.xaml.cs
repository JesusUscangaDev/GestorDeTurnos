using iTextSharpText = iTextSharp.text;
using SystemWindowsParagraph = System.Windows.Documents.Paragraph;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Diagnostics;
using System.Drawing.Printing;

namespace GestorDeTurnos
{
    public partial class MainWindow : Window
    {
        // Cambia la cadena de conexión según tu configuración de MySQL
        private string connectionString = "server=localhost;port=3307;database=gestorturnosdb;user=root;password=;";

        private int numeroDeTurno = 1;

        public MainWindow()
        {
            InitializeComponent();
            //CargarModulosDeDb();
        }

        private void CargarModulosDeDb()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id, nombre FROM Modulos";
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cbModulos.Items.Add(new Modulo
                            {
                                Id = reader.GetInt32("id"),
                                Nombre = reader.GetString("nombre")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los modulos: " + ex.Message);
            }
        }

        // Evento del botón para solicitar turno
        private void SolicitarTurno_Click(object sender, RoutedEventArgs e)
        {
            //if (cbModulos.SelectedItem == null)
            //{
            //    MessageBox.Show("Seleccione un modulo.");
            //    return;
            //}

            //Modulo moduloSeleccionado = (Modulo)cbModulos.SelectedItem;

            //string nuevoTurno = GenerarTurnoParaModulo(moduloSeleccionado.Id);
            string nuevoTurno = "C";
            if (!string.IsNullOrEmpty(nuevoTurno))
            {
                // Generar ticket en PDF
                //GenerarTicketPDF(nuevoTurno, moduloSeleccionado.Nombre);
            }
            GenerarTicketPDF(nuevoTurno, "C");
        }

        // Método para generar un nuevo turno para el modulo seleccionado
        private string GenerarTurnoParaModulo(int moduloId)
        {

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string turno = "Turno " + numeroDeTurno.ToString("D3");
                    numeroDeTurno++;

                    string query = "INSERT INTO Turnos (turno, modulo_id, fecha) VALUES (@turno, @moduloId, NOW())";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@turno", turno);
                    cmd.Parameters.AddWithValue("@moduloId", moduloId);
                    cmd.ExecuteNonQuery();

                    return turno;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el turno: " + ex.Message);
                return null;
            }
        }

        private void GenerarTicketPDF(string turno, string modulo)
        {
            Document doc = new Document(PageSize.A7); // Tamaño pequeño, similar al de los tickets creo
            string filePath = @"C:\Tickets\ticket_" + turno + ".pdf";

            // Verifica si la carpeta existe
            if (!Directory.Exists(@"C:\Tickets"))
            {
                Directory.CreateDirectory(@"C:\Tickets");
            }

            try
            {
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                // Añadir contenido al ticket
                doc.Add(new Paragraph("Aquí va a ir un título o algo"));
                doc.Add(new Paragraph("---------------"));
                doc.Add(new Paragraph($"Turno: 23"));
                doc.Add(new Paragraph($"Modulo: C"));
                doc.Add(new Paragraph("---------------"));
                doc.Add(new Paragraph("Gracias por su visita"));

                doc.Close();

                // Llamar al método de impresión del ticket
                ImprimirTicket(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar ticket: {ex.Message}");
            }
        }



        private void ImprimirTicket(string filePath)
        {
            /* try
             {
                 // Crea un nuevo proceso para la impresión
                 Process printProcess = new Process();
                 printProcess.StartInfo.FileName = filePath;  // El archivo PDF generado
                 printProcess.StartInfo.Verb = "print";       // Acción de imprimir
                 printProcess.StartInfo.CreateNoWindow = true;
                 printProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                 // Nombre de la impresora donde se imprimirá
                 printProcess.StartInfo.Arguments = "/p /h \"" + filePath + "\" \"" + PrinterSettings.InstalledPrinters[0] + "\"";

                 // Inicia el proceso de impresión
                 printProcess.Start();
                 printProcess.WaitForExit(5000);  // Espera un máximo de 5 segundos
                 printProcess.Close();
             }
             catch (Exception ex)
             {
                 MessageBox.Show($"Error al imprimir ticket: {ex.Message}");
             }*/
            ProcessStartInfo psi = new ProcessStartInfo
            {
                Verb = "print",  // Verbo "print" para imprimir
                FileName = filePath,  // Ruta del archivo PDF
                UseShellExecute = true,  // Ejecutar con Shell para usar la aplicación predeterminada
                CreateNoWindow = true,  // No crear una ventana visible
            };

            try
            {
                Process process = new Process { StartInfo = psi };
                process.Start();

                // Esperar que el proceso finalice antes de continuar
                process.WaitForExit(10000); // Tiempo de espera en milisegundos (10 segundos) para procesar
                process.Close();
                MessageBox.Show("Impresión correcta");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir ticket: {ex.Message}");
                Console.WriteLine($"Error al imprimir: {ex.Message}");
            }
        }


        // Clase Modulo para representar los modulos en el ComboBox
        public class Modulo
        {
            public int Id { get; set; }
            public string Nombre { get; set; }

            public override string ToString()
            {
                return Nombre;
            }
        }
    }
}
}
