using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForm
{
    public class CFB : EncryptionAlgoritm
    {
        private static string initVector { get; set; }
        private A52 A52Coder;
        private string key;
        public CFB()
        {
        }

        public CFB(string _key, string _initVector)
        {
            initVector = _initVector;
            key = _key;
            A52Coder = new A52(_key);
        }
        public static string MakeStringOutOfBinaryNumber(string binaryString)
        {
            if (binaryString.Length % 8 != 0)
            {
                // Ensure the binary string has a length that is a multiple of 8
                throw new ArgumentException("Invalid binary string length.");
            }


            StringBuilder originalString = new StringBuilder();

            // Process the binary string in 8-character chunks
            for (int i = 0; i < binaryString.Length; i += 8)
            {
                // Extract each 8-character chunk
                string binaryChunk = binaryString.Substring(i, 8);

                // Convert the binary chunk to a character and append to the original string
                char character = (char)Convert.ToByte(binaryChunk, 2);
                originalString.Append(character);
            }

            return originalString.ToString();
        }
        public static string MakeBinaryNumberOutOfString(string StringToTransfer)
        {
            string transferedString = "";
            foreach (char c in StringToTransfer)
            {
                // Convert character to 8-digit binary
                string binaryString = Convert.ToString(c, 2).PadLeft(8, '0');
                transferedString += binaryString;
                // Display the result for each character
            }
            return transferedString;
        }
        // public 
        //code initVector 8bit with A52 64bit key -> 8bit string
        //8bit string xor 8bitplainText xor 8bit string -> 8bit cyphertext
        public string Encrypt(string wordToEncrypt)
        {
            string initVectorFake = initVector;


            string wordToEncryptBinary = MakeBinaryNumberOutOfString(wordToEncrypt);
            // Console.WriteLine("WrodToEncodeBinaryCFB :" +wordToEncryptBinary);
            //64 bit uzimamo prvih 8 samo
            string encodedFinal = "";
            string encoded = "";
            string wordCodedByA52 = "";
            for (int j = 0; j < wordToEncryptBinary.Length; j += 8)
            {
                var a52 = new A52(key);
                wordCodedByA52 = a52.Encrypt(initVectorFake); //key xor IV
                wordCodedByA52 = MakeBinaryNumberOutOfString(wordCodedByA52);
                for (int i = 0; i < 8; i++)
                {
                    var bitToCodeFinal = int.Parse(wordToEncryptBinary[i + j].ToString()) ^ int.Parse(wordCodedByA52[i].ToString());
                    encoded += Convert.ToChar(bitToCodeFinal.ToString()[0]);
                }

                initVectorFake = encoded;
                encodedFinal += encoded;
                encoded = "";
            }

            //encodedFinal = MakeStringOutOfBinaryNumber(encodedFinal);
            return encodedFinal;
        }

        public string Decrypt(string encodedWord)
        {
            string initVectorFake = initVector;

            string decodedFinal = "";
            string decoded = "";
            string wordDecodedByA52 = "";

            //encodedWord = MakeBinaryNumberOutOfString(encodedWord);

            for (int j = 0; j < encodedWord.Length; j += 8)
            {
                var a52 = new A52(key);
                wordDecodedByA52 = a52.Encrypt(initVectorFake);
                wordDecodedByA52 = MakeBinaryNumberOutOfString(wordDecodedByA52);
                initVectorFake = "";
                for (int i = 0; i < 8; i++)
                {
                    var bitToDecodeFinal = int.Parse(encodedWord[i + j].ToString()) ^ int.Parse(wordDecodedByA52[i].ToString());
                    decoded += Convert.ToChar(bitToDecodeFinal.ToString()[0]);
                    initVectorFake += encodedWord[i];
                }
            }

            decoded = MakeStringOutOfBinaryNumber(decoded);
            return decoded;
        }
    }
}
