using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SynthriderzMapUpdateTool
{
    public class SynthriderzApiData
    {
        private async Task<JsonPageData?> GetTotalPages()
        {
            using HttpClient client = new();

            var response = await client.GetAsync(Globals.SynthRidersApiEndpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<JsonPageData>();
        }

        private async Task<List<string>> GenerateLinks()
        {
            var totalPages = await GetTotalPages();
            var pageLinks = new List<string>();

            if (totalPages != null && totalPages.PageCount > 0)
            {
                for (int i = 1; i <= totalPages.PageCount; i++)
                {
                    pageLinks.Add($"{Globals.SynthRidersApiEndpoint}?page={i}");
                }
            }

            return pageLinks;
        }

        public async Task<List<JsonPageData?>> DownloadJsonPages()
        {
            try
            {
                using HttpClient client = new();

                var pageList = new List<JsonPageData?>();
                var downloadLinks = await GenerateLinks();

                int count = 0;
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };
                await Parallel.ForEachAsync(downloadLinks, parallelOptions, async (uri, token) =>
                {
                    count++;
                    var response = await client.GetFromJsonAsync<JsonPageData>(uri, token);
                    pageList.Add(response);
                });

                return pageList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return [];
            }
        }
    }

    public record JsonPageData()
    {
        [JsonPropertyName("data")]
        public List<Data>? Data { get; set; }

        [JsonPropertyName("pageCount")]
        public int? PageCount { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("filename")]
        public string? Filename { get; set; }

        [JsonPropertyName("download_url")]
        public string? DownloadUrl { get; set; }
    }
}