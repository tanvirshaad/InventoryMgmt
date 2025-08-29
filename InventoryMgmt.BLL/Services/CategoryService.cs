using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL.EF.TableModels;
using InventoryMgmt.DAL.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryMgmt.DAL;

namespace InventoryMgmt.BLL.Services
{
    public class CategoryService
    {
        private readonly DataAccess _dataAccess;
        private readonly IMapper _mapper;

        public CategoryService(DataAccess _da, IMapper mapper)
        {
            _dataAccess = _da;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _dataAccess.CategoryData.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _dataAccess.CategoryData.GetByIdAsync(id);
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> CreateCategoryAsync(CategoryDto categoryDto)
        {
            var category = _mapper.Map<Category>(categoryDto);
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            await _dataAccess.CategoryData.AddAsync(category);
            await _dataAccess.CategoryData.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCategoryAsync(CategoryDto categoryDto)
        {
            var existingCategory = await _dataAccess.CategoryData.GetByIdAsync(categoryDto.Id);
            if (existingCategory == null) return false;

            existingCategory.Name = categoryDto.Name;
            existingCategory.Description = categoryDto.Description;
            existingCategory.UpdatedAt = DateTime.UtcNow;

            _dataAccess.CategoryData.Update(existingCategory);
            await _dataAccess.CategoryData.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _dataAccess.CategoryData.GetByIdAsync(id);
            if (category == null) return false;

            _dataAccess.CategoryData.Remove(category);
            await _dataAccess.CategoryData.SaveChangesAsync();
            return true;
        }
    }
}
