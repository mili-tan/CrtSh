namespace CrtSh
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var result = CrtshSharp.Search("example.com").Result;
            foreach (var cert in result)
            {
                Console.WriteLine(cert.IssuerName);
                Console.WriteLine(cert.NotAfter);
            }
        }
    }
}
