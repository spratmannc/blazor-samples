namespace BlazorWebAppEntra.Client.Services;

public class GetMeService
{
    private readonly HttpClient http;

    public GetMeService(HttpClient http)
    {
        this.http = http;
    }

    public async Task<string> GetMeAsync()
    {
        var response = await http.GetAsync("/me");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
