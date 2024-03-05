using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForm
{
    public class A52 : EncryptionAlgoritm
    {
        private static string key { get; set; }

        private static string R1 { get; set; }
        private static string R2 { get; set; }
        private static string R3 { get; set; }
        private static string R4 { get; set; }

        public A52()
        {

        }
        public A52(string _key)
        {
            key = _key;
        }
        public string Encrypt(string wordToEncrypt)
        {
            string word = MakeBinaryNumberOutOfString(wordToEncrypt);
            word = Crypt(word);
            return MakeStringOutOfBinaryNumber(word);
        }
        public string Decrypt(string wordToDecrypt)
        {
            string word = MakeBinaryNumberOutOfString(wordToDecrypt);
            word = Crypt(word);
            return MakeStringOutOfBinaryNumber(word);
        }
        private static string Crypt(string wordToEncrypt)
        {
            string inputString = key;

            MakeAKey(inputString);

            string encodedWord = "";

            foreach (var bit in wordToEncrypt)
            {
                int R1M = MajorityVote(R1[12], R1[14], R1[15]);
                int R2M = MajorityVote(R2[9], R2[13], R2[16]);
                int R3M = MajorityVote(R3[13], R3[16], R3[18]);
                int R4M = MajorityVote(R4[3], R4[7], R4[10]);

                int R1newBitShift =
                    (((int.Parse(R1[13].ToString()) ^ int.Parse(R1[16].ToString())) ^ int.Parse(R1[17].ToString())) ^
                     int.Parse(R1[18].ToString()));
                int R2newBitShift = int.Parse(R2[20].ToString()) ^ int.Parse(R2[21].ToString());
                int R3newBitShift =
                    ((int.Parse(R3[7].ToString()) ^ int.Parse(R3[20].ToString())) ^ int.Parse(R3[21].ToString())) ^ int.Parse(R3[22].ToString());
                int R4newBitShift = int.Parse(R4[11].ToString()) ^ int.Parse(R4[16].ToString());

                //R1
                if (int.Parse(R4[10].ToString()) == R4M)
                {
                    R1 = ShiftRightRegister(R1, R1newBitShift);
                }
                R1 = SetNumberAtIndex(R1, 15, '1');

                //R2
                if (int.Parse(R4[3].ToString()) == R4M)
                {
                    R2 = ShiftRightRegister(R2, R2newBitShift);
                }
                R2 = SetNumberAtIndex(R2, 16, '1');

                //R3
                if (int.Parse(R4[7].ToString()) == R4M)
                {
                    R3 = ShiftRightRegister(R3, R3newBitShift);
                }
                R3 = SetNumberAtIndex(R3, 18, '1');

                //R4
                R4 = ShiftRightRegister(R4, R4newBitShift);
                R4 = SetNumberAtIndex(R4, 10, '1');



                var bitToCode =
                    (((((0 ^ int.Parse(R1[18].ToString())) ^ int.Parse(R2[21].ToString())) ^
                       int.Parse(R3[22].ToString())) ^ R1M) ^ R2M) ^ R3M;

                var bitToCodeFinal = bitToCode ^ int.Parse(bit.ToString());

                encodedWord += Convert.ToChar(bitToCodeFinal.ToString()[0]);
            }

            return encodedWord;
        }
        public static void MakeAKey(string inputString)
        {
            key = "";
            R1 = "";
            R2 = "";
            R3 = "";
            R4 = "";
            key = MakeBinaryNumberOutOfString(inputString);

            // Console.WriteLine($"Key = {key}");

            for (int i = 0; i < key.Length; i++)
            {
                if (i < 19)
                {
                    R1 += key[i];
                    if (i < 17)
                    {
                        R4 += key[i];
                    }
                }
                else if (i < 41)
                {
                    R2 += key[i];
                }
                else
                {
                    R3 += key[i];
                }
            }
            // Console.WriteLine($"R1: {R1} {R1.Length}");
            // Console.WriteLine($"R2: {R2} {R2.Length}");
            // Console.WriteLine($"R3: {R3} {R3.Length}");
            // Console.WriteLine($"R4: {R4} {R4.Length}");
        }
        private static string MakeBinaryNumberOutOfString(string StringToTransfer)
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
        private static string SetNumberAtIndex(string register, int index, char number)
        {
            char[] arrayRegisterChar = register.ToCharArray();
            arrayRegisterChar[index] = number;
            return new string(arrayRegisterChar);
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
        private static string ShiftRightRegister(string binaryString, int newBit)
        {
            // Convert binary string to integer
            // int intValue = Convert.ToInt32(binaryString, 2); //presalta ga u int 32
            //
            // // Perform right shift operation
            // int shiftedValue = intValue >> 1; //1 ->0110011
            //
            // // Convert back to binary string
            // string result = Convert.ToString(shiftedValue, 2).PadLeft(binaryString.Length, '0');
            //
            // return SetNumberAtIndex(result, 0,newBit.ToString()[0]);
            string value = binaryString.Substring(0, binaryString.Length - 1);
            value = newBit + value;
            return value;
        }
        private static int MajorityVote(char a, char b, char c)
        {
            var ab = int.Parse(a.ToString()) & int.Parse(b.ToString());
            var bc = int.Parse(b.ToString()) & int.Parse(c.ToString());
            var ca = int.Parse(c.ToString()) & int.Parse(a.ToString());
            var konacno = (ab ^ bc) ^ ca;
            return konacno;

        }

    }
}
