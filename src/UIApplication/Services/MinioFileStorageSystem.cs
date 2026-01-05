using Minio;
using Microsoft.Extensions.Options;
using Minio.DataModel.Args;
using UIApplication.Services;

public class MinioOptions
{
    public string Endpoint { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string Bucket { get; set; }
    public bool UseSSL { get; set; }
}

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minio;
    private readonly MinioOptions _options;

    public MinioFileStorageService(IOptions<MinioOptions> options)
    {
        _options = options.Value;

        _minio = new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(_options.UseSSL)
            .Build();
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        bool found = await _minio.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_options.Bucket));

        if (!found)
            await _minio.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_options.Bucket));

        await _minio.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_options.Bucket)
                .WithObject(fileName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType)
        );

        return $"{_options.Endpoint}/{_options.Bucket}/{fileName}";
    }
}
