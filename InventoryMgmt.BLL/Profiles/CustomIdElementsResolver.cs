using AutoMapper;
using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL.EF.TableModels;
using System.Text.Json;

namespace InventoryMgmt.BLL.Profiles
{
    public class CustomIdElementsResolver : IValueConverter<string, List<CustomIdElement>>
    {
        public List<CustomIdElement> Convert(string sourceMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember))
                return new List<CustomIdElement>();
            
            try
            {
                var elements = JsonSerializer.Deserialize<List<CustomIdElement>>(sourceMember);
                return elements ?? new List<CustomIdElement>();
            }
            catch (Exception)
            {
                return new List<CustomIdElement>();
            }
        }
    }
}
