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

            var addresses = File.ReadAllLines(filePath)
                                .Where(line => !string.IsNullOrWhiteSpace(line))
                                .ToList();

            if (addresses.Count == 0)
                throw new Exception("No addresses found in file.");

            return addresses[random.Next(addresses.Count)];
        }
    }
}