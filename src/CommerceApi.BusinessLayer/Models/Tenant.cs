namespace CommerceApi.BusinessLayer.Models;

public record Tenant(Guid Id, string Name, string ConnectionString, string StorageConnectionString, string ContainerName);