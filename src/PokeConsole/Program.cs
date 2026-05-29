using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;          // GetFromJsonAsync lives here
using System.Text.Json.Serialization; // JsonPropertyName attribute
using System.Threading.Tasks;

namespace PokeConsole;

// --- Models --------------------------------------------------------------
// These match the JSON shape returned by https://pokeapi.co/api/v2/pokemon
// The deserializer maps JSON keys -> C# properties. We use [JsonPropertyName]
// so our C# names can follow C# conventions (PascalCase) while still matching
// the lowercase JSON keys.

public class PokemonListResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("results")]
    public List<PokemonEntry> Results { get; set; } = new();
}

public class PokemonEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("url")]
    public string Url { get; set; } = "";
}

// --- Program -------------------------------------------------------------

public class Program
{
    // ONE HttpClient for the whole app. Do not 'new' one per request.
    private static readonly HttpClient _client = new();

    // 'async Task Main' lets us await directly in Main (C# 7.1+).
    public static async Task Main()
    {
        Console.WriteLine("Fetching Pokemon...\n");

        try
        {
            // 'await' yields here while the network round-trip happens.
            // The thread is free during the wait instead of blocking.
            PokemonListResponse? data = await GetPokemonAsync();

            if (data is null || data.Results.Count == 0)
            {
                Console.WriteLine("No data returned.");
                return;
            }

            Console.WriteLine(
                $"API reports {data.Count} total Pokemon. " +
                $"Showing the first {data.Results.Count}:\n");

            int i = 1;
            foreach (PokemonEntry p in data.Results)
            {
                // {i,3} right-aligns the number in a 3-char field.
                Console.WriteLine($"{i,3}. {p.Name}");
                i++;
            }
        }
        catch (HttpRequestException ex)
        {
            // Thrown for network failures or non-success status codes.
            Console.WriteLine($"Network/HTTP error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            // HttpClient throws this on timeout.
            Console.WriteLine("The request timed out.");
        }
    }

    // Returns Task<T> because it's async and produces a value.
    private static async Task<PokemonListResponse?> GetPokemonAsync()
    {
        // GetFromJsonAsync does GET + JSON deserialize in one awaited call.
        // It throws HttpRequestException on a non-success status code.
        return await _client.GetFromJsonAsync<PokemonListResponse>(
            "https://pokeapi.co/api/v2/pokemon?limit=20");
    }
}