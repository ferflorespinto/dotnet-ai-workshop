using Microsoft.Extensions.AI;
using System.Numerics.Tensors;

namespace Embeddings;

public class ManualSemanticSearch
{
    public async Task RunAsync()
    {
        // Note: First run "ollama pull all-minilm" then "ollama serve"
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
            new OllamaEmbeddingGenerator(new Uri("http://127.0.0.1:11434"), modelId: "all-minilm");

        // TODO: Add your code here
        var titlesWithEmbeddings = await embeddingGenerator.GenerateAndZipAsync(TestData.DocumentTitles.Values);
        Console.WriteLine($"Got {titlesWithEmbeddings.Length} title-embedding pairs");

        while (true)
        {
            Console.Write("\nQuery: ");
            var input = Console.ReadLine()!;
            if (input == "") break;

            // TODO: Compute embedding and search
            var inputEmbedding = await embeddingGenerator.GenerateVectorAsync(input);

            var closest =
                from candidate in titlesWithEmbeddings
                let similarity = TensorPrimitives.CosineSimilarity(
                    candidate.Embedding.Vector.Span, inputEmbedding.Span)
                orderby similarity descending
                select new { candidate.Value, Similarity = similarity };

            foreach (var result in closest.Take(3))
            {
                Console.WriteLine($"({result.Similarity:F2}): {result.Value}");
            }
        }
    }
}
