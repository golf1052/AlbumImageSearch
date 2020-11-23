using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbumImageSearch
{
    public class HelperMethods
    {
        private Random random;

        public HelperMethods()
        {
            random = new Random();
        }

        public string GetRandomString(int length)
        {
            // 0-9 = 48-57
            // A-Z = 65-90
            // a-z = 97-122

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                int section = random.Next(3);
                if (section == 0)
                {
                    stringBuilder.Append((char)random.Next(48, 58));
                }
                else if (section == 1)
                {
                    stringBuilder.Append((char)random.Next(65, 91));
                }
                else
                {
                    stringBuilder.Append((char)random.Next(97, 123));
                }
            }

            return stringBuilder.ToString();
        }
    }
}
