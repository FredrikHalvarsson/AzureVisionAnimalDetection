using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Extensions.Configuration;
namespace AzureVisionAnimalDetection
{

    class Program
    {
        private static string Endpoint;
        private static string PredictionKey;
        private static string ProjectId;
        private static string PublishedModelName;

        static async Task Main(string[] args)
        {
            LoadConfiguration();
            Console.ReadLine();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Cat or Dog Image Classifier ===");
                Console.WriteLine("1. Input Image URL");
                Console.WriteLine("2. Input Local File Path");
                Console.WriteLine("3. Exit");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter Image URL: ");
                        string imageUrl = Console.ReadLine();
                        await ClassifyImageFromUrl(imageUrl);
                        break;
                    case "2":
                        Console.Write("Enter Local File Path: ");
                        string filePath = Console.ReadLine();
                        await ClassifyImageFromFile(filePath);
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        private static void LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>(); // This loads the user secrets

            var configuration = builder.Build();

            // Retrieve the configuration values
            Endpoint = configuration["CustomVision:Endpoint"];
            PredictionKey = configuration["CustomVision:PredictionKey"];
            ProjectId = configuration["CustomVision:ProjectId"];
            PublishedModelName = configuration["CustomVision:PublishedModelName"];
        }

        private static async Task ClassifyImageFromUrl(string imageUrl)
        {
            var client = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(PredictionKey))
            {
                Endpoint = Endpoint
            };

            try
            {
                var result = await client.ClassifyImageUrlAsync(Guid.Parse(ProjectId), PublishedModelName, new ImageUrl(imageUrl));
                DisplayResult(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task ClassifyImageFromFile(string filePath)
        {
            var client = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(PredictionKey))
            {
                Endpoint = Endpoint
            };

            try
            {
                using (var imageStream = File.OpenRead(filePath))
                {
                    var result = await client.ClassifyImageAsync(Guid.Parse(ProjectId), PublishedModelName, imageStream);
                    DisplayResult(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void DisplayResult(ImagePrediction result)
        {
            if (result.Predictions.Count == 0)
            {
                Console.WriteLine("No predictions were made.");
                return;
            }

            var catPrediction = result.Predictions.FirstOrDefault(p => p.TagName.Equals("cat", StringComparison.OrdinalIgnoreCase));
            var dogPrediction = result.Predictions.FirstOrDefault(p => p.TagName.Equals("dog", StringComparison.OrdinalIgnoreCase));

            double catProbability = catPrediction?.Probability ?? 0;
            double dogProbability = dogPrediction?.Probability ?? 0;

            Console.WriteLine($"Cat Probability: {catProbability:P1}");
            Console.WriteLine($"Dog Probability: {dogProbability:P1}");

            if (Math.Abs(catProbability - dogProbability) <= 0.05)
            {
                Console.WriteLine("The result is very close to 50/50. It could be either or neither.");
            }
            else if (Math.Max(catProbability, dogProbability) < 0.65)
            {
                Console.WriteLine("The result is uncertain and might not be correct.");
            }
            else
            {
                string resultTag = catProbability > dogProbability ? "Cat" : "Dog";
                Console.WriteLine($"The image is classified as a {resultTag.ToLower()}.");
            }
        }
    }

}
