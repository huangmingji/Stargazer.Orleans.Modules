namespace Stargazer.Orleans.ObjectStorage.Silo.Configuration;

public class StorageSettings
{
    public string Provider { get; set; } = "local";
    
    public int DefaultExpiryMinutes { get; set; } = 60;
    
    public LocalStorageSettings Local { get; set; } = new();
    
    public AliyunStorageSettings Aliyun { get; set; } = new();
    
    public AwsStorageSettings Aws { get; set; } = new();
    
    public AzureStorageSettings Azure { get; set; } = new();
    
    public TencentStorageSettings Tencent { get; set; } = new();
    
    public MinioStorageSettings Minio { get; set; } = new();
}

public class LocalStorageSettings
{
    public string BasePath { get; set; } = "/data/objects";
}

public class AliyunStorageSettings
{
    public string Endpoint { get; set; } = "";
    public string AccessKeyId { get; set; } = "";
    public string AccessKeySecret { get; set; } = "";
    public string BucketName { get; set; } = "";
}

public class AwsStorageSettings
{
    public string Region { get; set; } = "us-east-1";
    public string AccessKeyId { get; set; } = "";
    public string SecretAccessKey { get; set; } = "";
    public string BucketName { get; set; } = "";
}

public class AzureStorageSettings
{
    public string ConnectionString { get; set; } = "";
    public string ContainerName { get; set; } = "";
}

public class TencentStorageSettings
{
    public string Region { get; set; } = "ap-guangzhou";
    public string SecretId { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string BucketName { get; set; } = "";
}

public class MinioStorageSettings
{
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string BucketName { get; set; } = "";
    public bool UseSSL { get; set; } = false;
}
