using Refit;
using System.Diagnostics;

namespace Library.Users.Implementations.Version1;

public class Implementation : Interface
{
    #region [ Fields ]

    private readonly IRefitInterface refitInterface;
    #endregion

    #region [ CTors ]

    public Implementation(IRefitInterface refitInterface)
    {
        this.refitInterface = refitInterface;
    }
    #endregion

    #region [ Methods ]

    public async Task<Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>>? AllAsync(All.Parameters? parameters = null)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Models.Refit.GET.Parameters refitParameters;
        if (parameters is null)
            refitParameters = new()
            {

            };
        else
            refitParameters = new()
            {
                Id = parameters.Id,
                PageIndex = parameters.PageIndex,
                PageSize = parameters.PageSize,
                SearchTerm = parameters.SearchTerm,
                Name = parameters.Name,
                Email = parameters.Email,
                PhoneNumber = parameters.PhoneNumber,
                IsSelf = parameters.IsSelf
            };

        try
        {
            var response = await this.refitInterface.GET(refitParameters);

            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;

            if(response is null || response.Content is null)
            {
                return new()
                {

                    Title = "Couldn't reach to the server",
                    Detail = $"Failed at {nameof(AllAsync)}, after make a request call through refit",
                    Data = null,
                    Duration = duration,
                    IsSuccess = false
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                return new()
                {
                    Title =$"Error: {response.StatusCode}",
                    Detail = response.Error.Message,
                    Data = null,
                    Duration = duration,
                    IsSuccess = false
                };
            }

            List<Model> items = new(); 
            var data = response.Content.Items;
            if(data is null || !data.Any())
            {

                return new Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>
                {
                    Title = "Success",
                    Detail = $"Successfully fetched {items.Count} exercise(s)",
                    Duration = duration,
                    IsSuccess = true,
                    Data = new Library.Models.PaginationResults.Model<Model>
                    {
                        Total = items.Count,
                        Index = parameters is null ? null : parameters.PageIndex,
                        Size = parameters is null ? null : parameters.PageSize,
                        Items = items
                    }
                };
            }

            foreach (var item in data)
            {
                items.Add(new()
                {
                    Id = item.Id,
                    Name = item.Name,
                    Email = item.Email,
                    PhoneNumber = item.PhoneNumber,
                    ProfilePicture = item.ProfilePicture,
                });
            }

            return new Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>
            {
                Title = "Success",
                Detail = $"Successfully fetched {items.Count} exercise(s)",
                Duration = duration,
                IsSuccess = true,
                Data = new Library.Models.PaginationResults.Model<Model>
                {
                    Total = items.Count,
                    Index = parameters is null ? null : parameters.PageIndex,
                    Size = parameters is null ? null : parameters.PageSize,
                    Items = items
                }
            };
        }
        catch (ApiException ex)
        {

            throw new NotImplementedException();
        }
    }

    public async Task CreateAsync(Create.Payload payload)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Update.Payload payload)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(Delete.Parameters parameters)
    {
        throw new NotImplementedException();
    }
    #endregion
}
