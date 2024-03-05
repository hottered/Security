using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForm
{
    public interface EncryptionAlgoritm
    {
        string Encrypt(string message);
        string Decrypt(string message);
    }
}
