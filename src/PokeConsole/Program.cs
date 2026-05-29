using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "https://pokeapi.co/api/v2/pokemon?limit=50";
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var data = JsonSerializer.Deserialize(
                        responseBody, AppJsonContext.Default.PokemonResponse);

                    Console.WriteLine($"API reports {data.count} total Pokemon.\n");
                    Console.WriteLine("Pokemon with names longer than 7 characters:\n");

                    int shown = 0;
                    foreach (var pokemon in data.results)
                    {
                        if (pokemon.name.Length > 7)
                        {
                            Console.WriteLine($"Name: {pokemon.name}, URL: {pokemon.url}");
                            shown++;
                        }
                    }

                    Console.WriteLine($"\nMatched {shown} Pokemon.");
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
        }
    }
}

public class PokemonResponse
{
    public int count { get; set; }
    public List<Pokemon> results { get; set; }
}

public class Pokemon
{
    public string name { get; set; }
    public string url { get; set; }
}

[JsonSerializable(typeof(PokemonResponse))]
internal partial class AppJsonContext : JsonSerializerContext
{
}