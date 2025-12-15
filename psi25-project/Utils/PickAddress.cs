namespace psi25_project.Utils
{
    public static class AddressProvider
    {
        public static string GetRandomAddress(string filePath = "addresses.txt")
        {
            var resolvedPath = ResolveAddressPath(filePath);
            if (resolvedPath == null)
                throw new FileNotFoundException($"Address file not found: {filePath}");

            string? selectedAddress = null;
            int count = 0;

            using (var reader = new StreamReader(resolvedPath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    count++;
                    if (Random.Shared.Next(count) == 0) //reservoir sampling
                        selectedAddress = line;
                }
            }
            if (selectedAddress == null)
                throw new Exception("No valid address found.");
            return selectedAddress!;
        }

        private static string? ResolveAddressPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            // Absolute path: use as-is.
            if (Path.IsPathRooted(filePath))
                return File.Exists(filePath) ? filePath : null;

            // 1) Current working directory (local dev)
            if (File.Exists(filePath))
                return Path.GetFullPath(filePath);

            // 2) Published app directory (Render/Docker)
            var baseDir = AppContext.BaseDirectory;
            var inBaseDir = Path.Combine(baseDir, filePath);
            if (File.Exists(inBaseDir))
                return inBaseDir;

            // 3) Content root dir (if set by hosting env)
            var contentRoot = Environment.GetEnvironmentVariable("ASPNETCORE_CONTENTROOT");
            if (!string.IsNullOrWhiteSpace(contentRoot))
            {
                var inContentRoot = Path.Combine(contentRoot, filePath);
                if (File.Exists(inContentRoot))
                    return inContentRoot;
            }

            return null;
        }
    }
}
