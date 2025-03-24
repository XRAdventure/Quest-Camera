
namespace QRCodeVS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
            qRCodeGenerator.GenerateQRCode("UnityAdventure", "UA");
            qRCodeGenerator.GenerateQRCode("Suscribete", "Suscribete");
        }
    }
}
