using Play.Inventory.Service.Dtos;

namespace Play.Inventory.Service.Clients
{
    public class CatalogClient(HttpClient httpClient)
    {
        public async Task<IReadOnlyCollection<CategoryItemDto>> GetCategoryItemsAsync()
        {
            var items = await httpClient.GetFromJsonAsync<IReadOnlyCollection<CategoryItemDto>>("/items");
            return items;
        }
    }
}
