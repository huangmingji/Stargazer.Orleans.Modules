namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Authorization;

public static class StoragePolicies
{
    public static class Buckets
    {
        public const string View = "storage.bucket.view";
        public const string Create = "storage.bucket.create";
        public const string Update = "storage.bucket.update";
        public const string Delete = "storage.bucket.delete";
    }
    
    public static class Objects
    {
        public const string View = "storage.object.view";
        public const string Create = "storage.object.create";
        public const string Update = "storage.object.update";
        public const string Delete = "storage.object.delete";
    }
}

public static class StorageActions
{
    public const string Read = "Read";
    public const string Write = "Write";
}
