using Library.Queryable;
using Newtonsoft.Json;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;

namespace Library.Sets;

/// <summary>
/// DbSet-like implementation for API entities
/// Combines IQueryable functionality with CRUD operations
/// </summary>
public class ApiSet<T> : IApiSet<T>
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly ApiQueryProvider _queryProvider;

    public ApiSet(HttpClient httpClient, string endpoint)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _queryProvider = new ApiQueryProvider(_httpClient, _endpoint);
        Expression = Expression.Constant(this);
    }

    // IQueryable implementation - delegates to ApiQueryProvider
    public Type ElementType => typeof(T);
    public Expression Expression { get; }
    public IQueryProvider Provider => _queryProvider;

    public IEnumerator<T> GetEnumerator()
    {
        var result = _queryProvider.Execute<IEnumerable<T>>(Expression);
        return result.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // CRUD Operations

    /// <summary>
    /// Add a new entity (POST)
    /// </summary>
    public async Task<T> AddAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        try
        {
            Console.WriteLine($"📤 Adding new {typeof(T).Name}...");

            var json = JsonConvert.SerializeObject(entity, Formatting.Indented);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ {typeof(T).Name} created successfully");

                try
                {
                    var createdEntity = JsonConvert.DeserializeObject<T>(responseContent);
                    return createdEntity ?? entity;
                }
                catch
                {
                    return entity;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create {typeof(T).Name}. Status: {response.StatusCode}, Error: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error adding {typeof(T).Name}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Update an existing entity (PUT)
    /// </summary>
    public async Task<T> UpdateAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        try
        {
            Console.WriteLine($"📝 Updating {typeof(T).Name}...");

            var json = JsonConvert.SerializeObject(entity, Formatting.Indented);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // For PUT, we typically need entity ID in URL
            var entityId = GetEntityId(entity);
            var url = string.IsNullOrEmpty(entityId) ? _endpoint : $"{_endpoint}/{entityId}";

            var response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ {typeof(T).Name} updated successfully");

                try
                {
                    var updatedEntity = JsonConvert.DeserializeObject<T>(responseContent);
                    return updatedEntity ?? entity;
                }
                catch
                {
                    return entity;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to update {typeof(T).Name}. Status: {response.StatusCode}, Error: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error updating {typeof(T).Name}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Delete an entity by ID (DELETE)
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentException("ID cannot be null or empty", nameof(id));

        try
        {
            Console.WriteLine($"🗑️ Deleting {typeof(T).Name} with ID: {id}");

            var response = await _httpClient.DeleteAsync($"{_endpoint}/{id}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ {typeof(T).Name} deleted successfully");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to delete {typeof(T).Name}. Status: {response.StatusCode}, Error: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error deleting {typeof(T).Name}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Find an entity by ID (GET by ID) - returns null if not found
    /// </summary>
    public async Task<T?> FindAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return default(T);

        try
        {
            Console.WriteLine($"🔍 Finding {typeof(T).Name} with ID: {id}");

            var response = await _httpClient.GetAsync($"{_endpoint}/{id}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var entity = JsonConvert.DeserializeObject<T>(responseContent);
                Console.WriteLine($"✅ {typeof(T).Name} found");
                return entity;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"⚠️ {typeof(T).Name} with ID {id} not found");
                return default(T);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to find {typeof(T).Name}. Status: {response.StatusCode}, Error: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error finding {typeof(T).Name}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get entity by ID (throws if not found)
    /// </summary>
    public async Task<T> GetAsync(string id)
    {
        var entity = await FindAsync(id);
        if (entity == null)
        {
            throw new InvalidOperationException($"{typeof(T).Name} with ID '{id}' was not found");
        }
        return entity;
    }

    /// <summary>
    /// Helper method to extract ID from entity using reflection
    /// Assumes entity has either "Id" or "id" property
    /// </summary>
    private string? GetEntityId(T entity)
    {
        var idProperty = typeof(T).GetProperty("Id") ?? typeof(T).GetProperty("id");
        if (idProperty != null)
        {
            var value = idProperty.GetValue(entity);
            return value?.ToString();
        }
        return null;
    }
}
