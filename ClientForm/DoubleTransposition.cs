using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForm
{
    public class DoubleTransposition : EncryptionAlgoritm
    {

        private int[] columnKey;
        private int[] rowKey;     
        private int[] key;
        public DoubleTransposition(int[] key)
        {
            this.key = key;
            columnKey = key;
            rowKey = key;
        }
        public DoubleTransposition()
        {
            this.key = new int[0];
        }
        public string Encrypt(string enteredMessage)
        {
            if (this.key.Length == 0)
            {
                return enteredMessage;
            }
            string kodiranaRec = "";
            int velicinaKodiraneReciIzMatrice = columnKey.Length * rowKey.Length;
            if ( (enteredMessage.Length % velicinaKodiraneReciIzMatrice) != 0)
            {
                enteredMessage = DopuniRec(enteredMessage, velicinaKodiraneReciIzMatrice);
            }
            //Console.WriteLine(enteredMessage.Length);
            int brojMatrica = (int)Math.Ceiling(enteredMessage.Length / (float)velicinaKodiraneReciIzMatrice);
            //Console.WriteLine(brojMatrica);

            int k = 0;
            for (int i = 0; i < brojMatrica; i++)
            {
                string recOdDevetSlova = enteredMessage.Substring(k, velicinaKodiraneReciIzMatrice);
                char[,] matrica = new char[rowKey.Length, columnKey.Length];
                for (int p = 0; p < rowKey.Length; p++)
                {
                    for (int j = 0; j < columnKey.Length; j++)
                    {
                        matrica[p, j] = recOdDevetSlova[p * columnKey.Length + j];
                    }
                }
                k += velicinaKodiraneReciIzMatrice;
                matrica = EncodeMatrix(matrica);
                kodiranaRec += BackToWord(matrica);
            }

            return kodiranaRec;
        }
        public string Decrypt(string encodedMessage)
        {
            if (this.key.Length == 0)
            {
                return encodedMessage;
            }
            int velicinaKodiraneReciIzMatrice = columnKey.Length * rowKey.Length;
            if (encodedMessage.Length % velicinaKodiraneReciIzMatrice != 0)
            {
                encodedMessage = DopuniRec(encodedMessage, velicinaKodiraneReciIzMatrice);
            }
            string dekodiranaRec = "";
            int brojMatrica = (int)Math.Ceiling(encodedMessage.Length / (float)velicinaKodiraneReciIzMatrice);
            int k = 0;
            for (int i = 0; i < brojMatrica; i++)
            {
                string recOdDevetSlovaKojaSeDekodira = encodedMessage.Substring(k, velicinaKodiraneReciIzMatrice);
                char[,] matrica = new char[rowKey.Length, columnKey.Length];
                for (int p = 0; p < rowKey.Length; p++)
                {
                    for (int j = 0; j < columnKey.Length; j++)
                    {
                        matrica[p, j] = recOdDevetSlovaKojaSeDekodira[p * columnKey.Length + j];

                    }
                }
                k += velicinaKodiraneReciIzMatrice;
                matrica = DecodeMatrix(matrica);
                dekodiranaRec += BackToWord(matrica);
            }
            dekodiranaRec = UkloniUzastopneTildeSaKraja(dekodiranaRec);
            return dekodiranaRec;

        }
        public char[,] EncodeMatrix(char[,] originalMatrix)
        {
            char[,] encodedMatrix;

            encodedMatrix = RearrangeColumns(originalMatrix, columnKey);
            encodedMatrix = RearrangeRows(encodedMatrix, rowKey);

            return encodedMatrix;
        }
        public char[,] DecodeMatrix(char[,] encodedMatrix)
        {
            char[,] decodedMatrix;

            decodedMatrix = DecodeColumns(encodedMatrix, columnKey);
            decodedMatrix = DecodeRows(decodedMatrix, rowKey);

            return decodedMatrix;
        }
        public char[,] RearrangeColumns(char[,] originalMatrix, int[] columnOrder)
        {
            int rows = originalMatrix.GetLength(0);
            int columns = originalMatrix.GetLength(1);


            char[,] rearrangedMatrix = new char[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    int newColumn = columnOrder[j];
                    rearrangedMatrix[i, j] = originalMatrix[i, newColumn];
                }
            }

            return rearrangedMatrix;
        }
        public char[,] DecodeColumns(char[,] encodedMatrix, int[] columnKey)
        {

            int n = encodedMatrix.GetLength(0);
            int m = encodedMatrix.GetLength(1);

            char[,] decodedMatrix = new char[n, m];

            for (int i = 0; i < m; i++)
            {
                int keyColumn = columnKey[i];
                for (int j = 0; j < n; j++)
                {
                    decodedMatrix[j, keyColumn] = encodedMatrix[j, i];
                }
            }
            return decodedMatrix;
        }
        public char[,] RearrangeRows(char[,] originalMatrix, int[] rowOrder)
        {
            int rows = originalMatrix.GetLength(0);
            int columns = originalMatrix.GetLength(1);

            char[,] rearrangedMatrix = new char[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                int newRow = rowOrder[i];
                for (int j = 0; j < columns; j++)
                {
                    rearrangedMatrix[i, j] = originalMatrix[newRow, j];
                }
            }

            return rearrangedMatrix;
        }
        public char[,] DecodeRows(char[,] encodedMatrix, int[] rowKey)
        {
            int n = encodedMatrix.GetLength(0);
            int m = encodedMatrix.GetLength(1);

            char[,] decodedMatrix = new char[n, m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    decodedMatrix[rowKey[i], j] = encodedMatrix[i, j];
                }
            }
            return decodedMatrix;
        }
        public string BackToWord(char[,] decodedMatrix)
        {
            int n = decodedMatrix.GetLength(0);
            int m = decodedMatrix.GetLength(1);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    sb.Append(decodedMatrix[i, j]);
                }
            }

            string resultString = sb.ToString();

            return resultString;
        }
        public string DopuniRec(string rec, int delilac)
        {
            while ((rec.Length % delilac) != 0)
            {
                rec += "~";
            }
            return rec;
        }
        public string UkloniUzastopneTildeSaKraja(string rec)
        {
            // Pronađite indeks prvog karakter koji nije 'X' u reči
            int indeksPrvogNijeX = rec.Length - 1;

            while (indeksPrvogNijeX >= 0 && rec[indeksPrvogNijeX] == '~')
            {
                indeksPrvogNijeX--;
            }

            // Ako postoje uzastopne 'X' karakteri, uklonite ih
            if (indeksPrvogNijeX < rec.Length - 1)
            {
                rec = rec.Substring(0, indeksPrvogNijeX + 1);
            }

            return rec;
        }

        //pomocne funkcije
        public void PrintMatrix(char[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Console.Write(matrix[i, j] + " ");
                }
                Console.WriteLine();
            }

            Console.WriteLine();
        }
        public int[] GenerateRandomIntArray(int n)
        {
            if (n <= 0)
            {
                throw new ArgumentException("N must be a positive integer.");
            }

            int[] numbers = new int[n];

            // Initialize the array with sequential numbers
            for (int i = 0; i < n; i++)
            {
                numbers[i] = i;
            }

            // Use Fisher-Yates shuffle algorithm to shuffle the array
            Random random = new Random();
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                // Swap numbers[i] and numbers[j]
                int temp = numbers[i];
                numbers[i] = numbers[j];
                numbers[j] = temp;
            }

            return numbers;
        }
        public void PrintArray(int[] array)
        {
            foreach (var value in array)
            {
                Console.Write(value + " ");
            }
            Console.WriteLine();
        }
    }
}
