namespace HW2Dumper
{
    public static class ResourceHelper
    {
        public static void ExtractResource(byte[] resource, string outputPath)
        {
            Console.WriteLine($"ExtractResource called for: {outputPath}");
            using FileStream fileStream = new(outputPath, FileMode.Create, FileAccess.Write);
            fileStream.Write(resource, 0, resource.Length);
            Console.WriteLine($"Resource extracted to: {outputPath}");
        }
    }
}
