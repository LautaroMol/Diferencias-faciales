using OpenCvSharp;
using Emgu.CV.Structure;

class Programa
{
    static void Main(string[] args)
    {
        string carpeta = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\diferencias_faciales";
        if (!System.IO.Directory.Exists(carpeta))
        {
            System.IO.Directory.CreateDirectory(carpeta);
        }
        HashSet<Rect> carasdist = new HashSet<Rect>();
        if (args.Length == 0)
        {
            Console.WriteLine("por favor arrastre un video y luego presione enter");
        }
        string video = Console.ReadLine();
        if (System.IO.File.Exists(video) == false)
        {
            Console.Write("el archivo del video no existe");
            return;
        }
        Dictionary<string, List<Rect>> carasPorPersona = new Dictionary<string, List<Rect>>();
        int identificador = 0;
        var captura = new VideoCapture(video);
        var detector = new CascadeClassifier("haarcascade_frontalface_default.xml");
        var ventana = new Window("Prueba de OpenCV deteccion de caras");
        if (!captura.IsOpened()) { Console.WriteLine("error al intentar abrir el video");
            Console.ReadLine();
                return;
            }
        while (true)
            {
            Mat frame = new Mat();
            captura.Read(frame);
            if(frame.Empty())
            {
                 break;
            }

            //convertimos la imagen a escala de grises
            var imgGris = new Mat();
            Cv2.CvtColor(frame, imgGris, ColorConversionCodes.BGR2GRAY);

            //detector de caras
            var caras = detector.DetectMultiScale(imgGris, 1.3, 5, HaarDetectionTypes.ScaleImage, new Size(30, 30));

            foreach (Rect cara in caras)
            {
                bool caraYaRegist = false;
                string persona = "identificador";
                if (carasPorPersona.ContainsKey(persona))
                {
                    foreach (Rect caraRegistrada in carasPorPersona[persona])
                    {
                        if (esIgual(caraRegistrada, cara))
                        {
                            caraYaRegist = true;
                            break;
                        }
                    }
                }
                else
                {
                    carasPorPersona[persona] = new List<Rect>();
                }
                if (!caraYaRegist)
                {
                    carasPorPersona[persona].Add(cara);
                    carasdist.Add(cara);

                    //dibujamos un rectangulo alrededor de la cara
                    Cv2.Rectangle(frame, cara, Scalar.Red);

                    //extraccion de la region facial
                    Mat region = new Mat(frame, cara);
                    
                    //dibujamos lineas para resaltar las diferencias
                    Point ojoizq = new Point(cara.Left + cara.Width / 4, cara.Top + cara.Height / 4);
                    Point ojoder = new Point(cara.Left + 3 * cara.Width / 4, cara.Top + cara.Height / 4);
                    Point nariz = new Point(cara.Left + cara.Width / 2, cara.Top * cara.Height / 3);
                    Point bocaIzq = new Point(cara.Left + cara.Width / 4, cara.Top + 3 * cara.Height / 4);
                    Point bocaDer = new Point(cara.Left + 3 * cara.Width / 4, cara.Top + 3 * cara.Height / 4);

                    Cv2.Line(frame, ojoizq, ojoder, Scalar.Blue);
                    Cv2.Line(frame, ojoizq, nariz, Scalar.Blue);
                    Cv2.Line(frame, ojoder, nariz, Scalar.Blue);
                    Cv2.Line(frame, bocaIzq, bocaDer, Scalar.Blue);
                    Cv2.Line(frame, bocaIzq, nariz, Scalar.Blue);
                    Cv2.Line(frame, bocaDer, nariz, Scalar.Blue);

                    //guardamos las diferentes caras
                    string nomimgdif = $"{carpeta}\\cara_{identificador}.jpg";
                    Cv2.ImWrite(nomimgdif, region);

                    //incremento en el identificador
                    identificador++;
                }
                
                //Mostramos las caras detectadas
                ventana.ShowImage(frame);
                if (Cv2.WaitKey(1) == 27) //presionamos esc para salir
                { return; }
            }
        }
    }
    //funcion para comparar 2 caras
    static bool esIgual(Rect cara1, Rect cara2)
    {
        // Calcula el área de las caras para compararlas
        int areaCara1 = cara1.Width * cara1.Height;
        int areaCara2 = cara2.Width * cara2.Height;

        // Usa un margen en el área para determinar si las caras son iguales
        double margenArea = 0.8; // Puedes ajustar este valor según sea necesario

        return Math.Abs(areaCara1 - areaCara2) < margenArea * Math.Min(areaCara1, areaCara2);
    }
}