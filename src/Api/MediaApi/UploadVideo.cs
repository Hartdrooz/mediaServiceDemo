using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaWeb.Infrastructure.Factories;
using MediaWeb.Infrastructure.Model;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure.Authentication;

namespace MediaApi
{
    public class UploadVideo
    {
        private readonly IMediaServiceFactory _factory;
        private readonly ConfigWrapper _config;
        IAzureMediaServicesClient _client;

        public UploadVideo(IMediaServiceFactory factory,ConfigWrapper config)
        {
            _factory = factory;
            _config = config;
        }

        [FunctionName("createAsset")]
        public async Task Run([BlobTrigger("uploads/{name}", Connection = "MediaVideo")] Stream myBlob, string name, ILogger log)
        {
            try
            {
                log.LogInformation($"Received file {name}");

                _client = await _factory.CreateMediaServicesClientAsync(_config);

                string uniqueId = Guid.NewGuid().ToString();

                string jobName = $"job-{name}-{uniqueId}";
                string outputAssetName = $"output-{name}-{uniqueId}";
                string inputAssetName = $"input-{name}-{uniqueId}";
                string assetName = name;

                log.LogInformation("Get Transformer");

                await GetOrCreateTransformAsync(_client,
                                                _config.ResourceGroup,
                                                _config.AccountName,
                                                _config.AdaptiveStreamingTransformName);

                log.LogInformation("Create asset in Media Service");

                await CreateAssetAsync(inputAssetName);

                log.LogInformation("Upload File input to Media Service");

                await UploadToMediaService(myBlob, inputAssetName, assetName);

                log.LogInformation("Create the outputAsset");

                await CreateAssetAsync(outputAssetName);

                log.LogInformation("Submit job of encoding");

                await SubmitJobAsync(jobName, inputAssetName, outputAssetName);

                log.LogInformation("Finishing processing Azure Function");
            }
            catch (Exception ex)
            {
                log.LogError($"Cannot process file {ex.Message}");
                throw ex;
            }



        }

        private async Task UploadToMediaService(Stream myBlob, string inputAssetName, string assetName)
        {
            var response = await _client.Assets.ListContainerSasAsync
                                 (
                                    _config.ResourceGroup,
                                    _config.AccountName,
                                    inputAssetName,
                                    permissions: AssetContainerPermission.ReadWrite,
                                    expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime()
                                 );

            var sasUri = new Uri(response.AssetContainerSasUrls.First());
            var container = new CloudBlobContainer(sasUri);

            var blob = container.GetBlockBlobReference(assetName);
            
            await blob.UploadFromStreamAsync(myBlob);
        }

        private async Task<Transform> GetOrCreateTransformAsync(IAzureMediaServicesClient client,
                                                                string resourceGroupName,
                                                                string accountName,
                                                                string transformName)
        {
            // Does a Transform already exist with the desired name? Assume that an existing Transform with the desired name
            // also uses the same recipe or Preset for processing content.
            Transform transform = await client.Transforms.GetAsync(resourceGroupName, accountName, transformName);

            if (transform == null)
            {
                // You need to specify what you want it to produce as an output
                TransformOutput[] output = new TransformOutput[]
                {
                    new TransformOutput
                    {
                        // The preset for the Transform is set to one of Media Services built-in sample presets.
                        // You can  customize the encoding settings by changing this to use "StandardEncoderPreset" class.
                        Preset = new BuiltInStandardEncoderPreset()
                        {
                            // This sample uses the built-in encoding preset for Adaptive Bitrate Streaming.
                            PresetName = EncoderNamedPreset.AdaptiveStreaming
                        }
                    }
                };

                // Create the Transform with the output defined above
                transform = await client.Transforms.CreateOrUpdateAsync(resourceGroupName, accountName, transformName, output);
            }

            return transform;
        }

        private async Task<Asset> CreateAssetAsync(string assetName) 
        {
            return await _client.Assets.CreateOrUpdateAsync(_config.ResourceGroup,
                                                            _config.AccountName,
                                                            assetName,
                                                            new Asset());
        }

        private async Task<Job> SubmitJobAsync(string jobName, 
                                               string inputFilename, 
                                               string outputFilename)
        {
            var jobInput = new JobInputAsset(assetName: inputFilename);

            JobOutput[] jobOutputs =
            {
                new JobOutputAsset(outputFilename)
            };

            Job job = await _client.Jobs.CreateAsync(_config.ResourceGroup,
                                                                      _config.AccountName,
                                                                      _config.AdaptiveStreamingTransformName,
                                                                      jobName,
                                                                      new Job
                                                                      {
                                                                          Input = jobInput,
                                                                          Outputs = jobOutputs
                                                                      });

            return job;
        }
    }

}
