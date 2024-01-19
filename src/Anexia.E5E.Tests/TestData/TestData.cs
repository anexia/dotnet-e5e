using System.IO;

namespace Anexia.E5E.Tests.TestData;

internal static class TestData
{
	private static string ReadTestDataFile(string path) => File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "TestData", path));

	internal static string BinaryRequestWithMultipleFiles => ReadTestDataFile("binary_request_with_multiple_files.json");
	internal static string BinaryRequestWithUnknownContentType => ReadTestDataFile("binary_request_unknown_content_type.json");
}
