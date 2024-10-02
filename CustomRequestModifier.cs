namespace WireMockServerClientExamples
{
    using System.Threading.Tasks;

    internal static class CustomRequestModifier
    {
        public static async Task MyCustomRequestModifier(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //var contentString = await request?.Content?.ReadAsStringAsync(cancellationToken) ?? string.Empty;
            await Task.CompletedTask;
        }
    }
}
