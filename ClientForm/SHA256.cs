using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientForm
{
    public class SHA256 
    {
        private uint[] initial_hash =
        {
            0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a,
            0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19
        };
        private readonly uint[] const_k = new uint[64]
        {
            0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5,
            0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5,
            0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3,
            0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174,
            0xE49B69C1, 0xEFBE4786, 0x0FC19DC6, 0x240CA1CC,
            0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA,
            0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7,
            0xC6E00BF3, 0xD5A79147, 0x06CA6351, 0x14292967,
            0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13,
            0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85,
            0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3,
            0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070,
            0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5,
            0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3,
            0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208,
            0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2
        };
        public SHA256() { }

        public static SHA256 Create()
        {
            return new SHA256();
        }
        public string ComputeHash(string plainText)
        {
            var p = Padding(plainText);

            var block_list = Parse(p);
            var s = new uint[8];
            Array.Copy(initial_hash, s, initial_hash.Length);

            foreach (var block in block_list)
            {

                var pair = MakePair(s);
                var expanded_block = ExpandBlock(block);

                for (int n = 0; n < 64; ++n)
                {
                    var CH = Ch(pair["e"], pair["f"], pair["g"]);
                    var MAJ = Maj(pair["a"], pair["b"], pair["c"]);
                    var SIG0 = Sigma0(pair["a"]);
                    var SIG1 = Sigma1(pair["e"]);

                    var WJ_KJ = (const_k[n] + expanded_block[n]);
                    var T1_TEMP = (pair["h"] + WJ_KJ + CH);
                    var T1 = (T1_TEMP + SIG1);
                    var T2 = (SIG0 + MAJ);

                    pair["h"] = pair["g"];
                    pair["g"] = pair["f"];
                    pair["f"] = pair["e"];
                    pair["e"] = (pair["d"] + T1);
                    pair["d"] = pair["c"];
                    pair["c"] = pair["b"];
                    pair["b"] = pair["a"];
                    pair["a"] = (T1 + T2);
                }

                s[0] = (pair["a"] + s[0]);
                s[1] = (pair["b"] + s[1]);
                s[2] = (pair["c"] + s[2]);
                s[3] = (pair["d"] + s[3]);
                s[4] = (pair["e"] + s[4]);
                s[5] = (pair["f"] + s[5]);
                s[6] = (pair["g"] + s[6]);
                s[7] = (pair["h"] + s[7]);
            }
            return MakeHash(s);
        }
        private uint[] ExpandBlock(uint[] block)
        {
            uint[] result = { };

            for (int x = 0; x < 16; x++)
            {
                uint[] chunk_array = new uint[32];
                Array.Copy(block, x * 32, chunk_array, 0, 32);
                var chunk_binary_str = ToBinary(chunk_array);
                var chunk_decimal = Convert.ToUInt32(chunk_binary_str, 2);
                SelfAppend(ref result, chunk_decimal);
            }
            for (int y = 16; y < 64; y++)
            {
                var T1 = Sub0(result[y - 15]) + result[y - 16];
                var T2 = T1 + result[y - 7];
                var T3 = T2 + Sub1(result[y - 2]);
                SelfAppend(ref result, T3);
            }
            return result;
        }
        private string ToBinary(uint[] chunk)
        {
            string result = string.Empty;

            foreach (var n in chunk)
            {
                result += n.ToString();
            }
            return result;
        }
        private string MakeHash(uint[] s)
        {
            var s_byte_array = s.SelectMany((v) => BitConverter.GetBytes(v).Reverse()).ToArray();
            var result_str = string.Join("", s_byte_array.Select(v => $"{v:X2}"));
            return result_str.ToLower();
        }
        private Dictionary<string, uint> MakePair(uint[] hash)
        {
            var dictionary = new Dictionary<string, uint>();
            dictionary.Add("a", hash[0]);
            dictionary.Add("b", hash[1]);
            dictionary.Add("c", hash[2]);
            dictionary.Add("d", hash[3]);
            dictionary.Add("e", hash[4]);
            dictionary.Add("f", hash[5]);
            dictionary.Add("g", hash[6]);
            dictionary.Add("h", hash[7]);

            return dictionary;
        }
        private List<uint[]> Parse(uint[] plain_bits)
        {
            var result = new List<uint[]>();
            const int BLOCK_SIZE = 512;
            var length = plain_bits.Length;
            var num_blocks = length / BLOCK_SIZE;

            for (int n = 0; n < num_blocks; n++)
            {
                var block = new uint[BLOCK_SIZE];
                Array.Copy(plain_bits, n * BLOCK_SIZE, block, 0, BLOCK_SIZE);
                result.Add(block);
            }
            return result;
        }
        private uint[] Padding(string plain_text)
        {
            var plain_bits = ToUInt32Array(plain_text);
            var length = plain_bits.Length;
            var k = CalculateK(plain_bits);
            uint[] buf = { };
            buf = Extend<uint>(plain_bits, 1);
            for (int r = 0; r < k; r++)
            {
                SelfAppend(ref buf, 0u);
            }
            var bytStr = Convert.ToString(length, 2);
            bytStr = bytStr.ToString().PadLeft(64, '0');
            uint[] bytStr_array = { };
            for (int x = 0; x <= 63; x++)
            {
                var num_str = bytStr.Substring(x, 1);
                var num = uint.Parse(num_str);
                SelfAppend(ref bytStr_array, num);
            }
            foreach (var b in bytStr_array)
            {
                SelfAppend(ref buf, b);
            }
            return buf;
        }
        private uint CalculateK(uint[] plain_bits)
        {
            uint k = 0;
            var length = plain_bits.Length;

            while ((length + 1 + k) % 512 != 448)
            {
                k += 1;
            }

            return k;
        }
        private uint[] ToUInt32Array(string plain_text)
        {
            var a = Encoding.ASCII.GetBytes(plain_text);
            uint[] result = { };

            foreach (var n in a)
            {

                var j = int.Parse(Convert.ToString(n, 2));
                var len = j.ToString().Length;
                var fill_len = 0;
                if (len < 8)
                {
                    fill_len = Math.Abs(8 - len);
                    while (fill_len > 0)
                    {
                        fill_len--;
                        SelfAppend(ref result, 0u);
                    }
                }

                SelfConcat(ref result, ToArray((uint)j));
            }
            return result;
        }
        private T[] Extend<T>(IList<T> source, T num)
        {
            var result = new T[source.Count + 1];
            for (int n = 0; n < source.Count; n++)
            {
                result[n] = source[n];
            }
            result[source.Count] = num;
            return result;
        }
        private uint[] ToArray(uint num)
        {
            var s = num.ToString();
            var l = s.Length;
            var r = new uint[l];
            int counter = -1;
            foreach (var n in s)
            {
                counter++;
                r[counter] = uint.Parse(n.ToString());
            }

            return r;
        }
        private void SelfAppend<T>(ref T[] source, T num)
        {
            source = source.Append(num).ToArray();
        }
        private void SelfConcat<T>(ref T[] source, T[] destination)
        {
            source = source.Concat(destination).ToArray();
        }
        private uint Rot_R(uint x, byte n)
        {
            return (x >> n) | (x << (32 - n));
        }
        private uint Ch(uint x, uint y, uint z)
        {
            return (x & y) ^ (~x & z);
        }
        private uint Maj(uint x, uint y, uint z)
        {
            return (x & y) ^ (x & z) ^ (y & z);
        }
        private uint Sigma0(uint x)
        {
            return Rot_R(x, 2) ^ Rot_R(x, 13) ^ Rot_R(x, 22);
        }
        private uint Sigma1(uint x)
        {
            return Rot_R(x, 6) ^ Rot_R(x, 11) ^ Rot_R(x, 25);
        }
        private uint Sub0(uint x)
        {
            return Rot_R(x, 7) ^ Rot_R(x, 18) ^ (x >> 3);
        }
        private uint Sub1(uint x)
        {
            return Rot_R(x, 17) ^ Rot_R(x, 19) ^ (x >> 10);
        }

    }
}