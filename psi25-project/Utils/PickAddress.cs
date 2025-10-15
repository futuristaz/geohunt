using System;
using System.Data;
using System.IO;
using System.Linq;

namespace psi25_project
{
    public static class AddressProvider
    {
        private static readonly Random random = new Random();

        public static string GetRandomAddress(string filePath = "addresses.txt")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Address file not found: {filePath}");

            string selectedAddress = null; 
            int count = 0;

            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    count++;
                    if (random.Next(count) == 0) //reservoir sampling
                        selectedAddress = line;
                }
            }
            if (selectedAddress == null)
                throw new Exception("No valid address found.");
            return selectedAddress;
        }
    }
}