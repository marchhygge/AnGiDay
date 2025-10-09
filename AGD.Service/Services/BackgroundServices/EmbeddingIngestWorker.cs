using AGD.Repositories.DBContext;
using AGD.Repositories.Repositories;
using AGD.Service.Integrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AGD.Service.Services.BackgroundServices
{
    public class EmbeddingIngestWorker : BackgroundService
    {
        private readonly ILogger<EmbeddingIngestWorker> _logger;
        private readonly IConfiguration _cfg;
        private readonly OllamaEmbeddingClient _embedClient;
        private readonly IServiceProvider _serviceProvider;

        public EmbeddingIngestWorker(
            ILogger<EmbeddingIngestWorker> logger,
            IConfiguration cfg,
            OllamaEmbeddingClient embedClient,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _cfg = cfg;
            _embedClient = embedClient;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmbeddingIngestWorker started.");
            var batchSize = int.Parse(_cfg["Embedding:BatchSize"] ?? "50");
            var embedModel = _cfg["Ollama:EmbeddingModel"] ?? "nomic-embed-text:latest";

            using var scope = _serviceProvider.CreateScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var existingIds = await _unitOfWork.EmbeddingRepository.GetExistingRestaurantIdsAsync(stoppingToken);

                    var toEmbed = await _unitOfWork.RestaurantRepository.GetRestaurantsToEmbedAsync(existingIds, batchSize, stoppingToken);
                    if (toEmbed == null || toEmbed.Count == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
                        continue;
                    }

                    foreach (var r in toEmbed)
                    {
                        stoppingToken.ThrowIfCancellationRequested();

                        var text = await _unitOfWork.RestaurantRepository.BuildTextForEmbeddingAsync(r.Id, stoppingToken);
                        if (string.IsNullOrWhiteSpace(text))
                        {
                            _logger.LogWarning("Empty text for restaurant {Id}, skip.", r.Id);
                            continue;
                        }

                        float[] vec;
                        try
                        {
                            vec = await _embedClient.EmbedAsync(embedModel, text, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Embedding API error for restaurant {Id}", r.Id);
                            continue;
                        }

                        if (vec == null || vec.Length != 768)
                        {
                            _logger.LogWarning("Embedding for restaurant {Id} is invalid (null or not 768 dims), skip upsert.", r.Id);
                            continue;
                        }
                        try
                        {
                            await _unitOfWork.EmbeddingRepository.UpsertRestaurantEmbeddingAsync(r.Id, vec, embedModel, stoppingToken);
                            _logger.LogInformation("Upserted embedding for restaurant {Id}", r.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to upsert embedding for restaurant {Id}", r.Id);
                        }
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in embedding ingestion loop.");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            _logger.LogInformation("EmbeddingIngestWorker stopping.");
        }
    }
}
