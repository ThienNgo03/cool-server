using Refit;
using System.Diagnostics;
using Library.User;
using Library.User.Implementations.Version1.Models.Refit;

namespace Library.User.Implementations.Version1;

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

    public async Task<Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>> AllAsync(All.Parameters parameters)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Models.Refit.GET.Parameters refitParameters = new()
        {
            id = parameters.id,
            name = parameters.name,
            email = parameters.email,
            phoneNumber = parameters.phoneNumber,
            PageIndex = parameters.pageIndex,
            PageSize = parameters.pageSize

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

            List<Model> items = []; 
            var data = response.Content.Items;
            if(data is null || data.Count == 0)
            {

                return new Library.Models.Response.Model<Library.Models.PaginationResults.Model<Model>>
                {
                    Title = "Success",
                    Detail = $"Successfully fetched {items.Count} competition(s)",
                    Duration = duration,
                    IsSuccess = true,
                    Data = new Library.Models.PaginationResults.Model<Model>
                    {
                        Total = items.Count,
                        Index = parameters.pageIndex,
                        Size = parameters.pageSize,
                        Items = items
                    }
                };
            }

            foreach (var item in data)
            {
                items.Add(new()
                {
                    Id = (Guid)parameters.id,
                    Name = parameters.name,
                    Email = parameters.email,
                    PhoneNumber = parameters.phoneNumber,
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
                    Index = parameters.pageIndex,
                    Size = parameters.pageSize,
                    Items = items
                }
            };
        }
        catch (ApiException ex)
        {

            throw new NotImplementedException(ex.Message);
        }
    }

    public async Task CreateAsync(Create.Payload payload)
    {
        /*Stopwatch stopwatch = Stopwatch.StartNew();*/
        try
        {
            var refitPayload = new Models.Refit.POST.Payload
            {
                Name = payload.Name,
                Email = payload.Email,
                PhoneNumber = payload.PhoneNumber,
            };

            var response = await this.refitInterface.POST(refitPayload);

            /*stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;*/
        }
        catch (ApiException ex)
        {
            throw new NotImplementedException(ex.Message);
        }
    }

    public async Task UpdateAsync(Update.Payload payload)
    {
        try
        {
            var refitPayload = new Models.Refit.PUT.Payload
            {
                Id = payload.Id,
                Name = payload.Name,
                Email = payload.Email,
                PhoneNumber = payload.PhoneNumber
            };

            var response = await this.refitInterface.PUT(refitPayload);
        }
        catch (ApiException ex)
        {
            throw new NotImplementedException(ex.Message);
        }
    }

    public async Task DeleteAsync(Delete.Parameters parameters)
    {
        try
        {
            var refitParameters = new Models.Refit.DELETE.Parameters
            {
                Id = parameters.Id
            };

            var response = await this.refitInterface.DELETE(refitParameters);
        }
        catch (ApiException ex)
        {
            throw new NotImplementedException(ex.Message);
        }
    }
    #endregion
}
