using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {

        static public string EncodeTo64(string toEncode)

        {

            byte[] toEncodeAsBytes

                  = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);

            string returnValue

                  = System.Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;

        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
            //Console.ReadKey();

            String myData = "Here is a string to encode.";
            string myDataEncoded = EncodeTo64(myData);

            Console.WriteLine(myDataEncoded);
            Console.ReadKey();




        }
    }
}
