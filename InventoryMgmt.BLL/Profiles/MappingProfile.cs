using InventoryMgmt.BLL.DTOs;
using InventoryMgmt.DAL.EF.TableModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Profiles
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Tag, TagDto>().ReverseMap();
            CreateMap<Comment, CommentDto>().ReverseMap();

            CreateMap<Inventory, InventoryDto>()
                .ForMember(dest => dest.CustomIdElementList, opt => opt.ConvertUsing(new CustomIdElementsResolver(), src => src.CustomIdElements))
                .ForMember(dest => dest.TextField1, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.TextField1Name,
                    Description = src.TextField1Description,
                    ShowInTable = src.TextField1ShowInTable
                }))
                .ForMember(dest => dest.TextField2, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.TextField2Name,
                    Description = src.TextField2Description,
                    ShowInTable = src.TextField2ShowInTable
                }))
                .ForMember(dest => dest.TextField3, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.TextField3Name,
                    Description = src.TextField3Description,
                    ShowInTable = src.TextField3ShowInTable
                }))
                .ForMember(dest => dest.MultiTextField1, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.MultiTextField1Name,
                    Description = src.MultiTextField1Description,
                    ShowInTable = src.MultiTextField1ShowInTable
                }))
                .ForMember(dest => dest.MultiTextField2, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.MultiTextField2Name,
                    Description = src.MultiTextField2Description,
                    ShowInTable = src.MultiTextField2ShowInTable
                }))
                .ForMember(dest => dest.MultiTextField3, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.MultiTextField3Name,
                    Description = src.MultiTextField3Description,
                    ShowInTable = src.MultiTextField3ShowInTable
                }))
                .ForMember(dest => dest.NumericField1, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.NumericField1Name,
                    Description = src.NumericField1Description,
                    ShowInTable = src.NumericField1ShowInTable,
                    NumericConfig = new NumericFieldConfig
                    {
                        IsInteger = src.NumericField1IsInteger,
                        MinValue = src.NumericField1MinValue,
                        MaxValue = src.NumericField1MaxValue,
                        StepValue = src.NumericField1StepValue,
                        DisplayFormat = src.NumericField1DisplayFormat ?? string.Empty
                    }
                }))
                .ForMember(dest => dest.NumericField2, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.NumericField2Name,
                    Description = src.NumericField2Description,
                    ShowInTable = src.NumericField2ShowInTable,
                    NumericConfig = new NumericFieldConfig
                    {
                        IsInteger = src.NumericField2IsInteger,
                        MinValue = src.NumericField2MinValue,
                        MaxValue = src.NumericField2MaxValue,
                        StepValue = src.NumericField2StepValue,
                        DisplayFormat = src.NumericField2DisplayFormat ?? string.Empty
                    }
                }))
                .ForMember(dest => dest.NumericField3, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.NumericField3Name,
                    Description = src.NumericField3Description,
                    ShowInTable = src.NumericField3ShowInTable,
                    NumericConfig = new NumericFieldConfig
                    {
                        IsInteger = src.NumericField3IsInteger,
                        MinValue = src.NumericField3MinValue,
                        MaxValue = src.NumericField3MaxValue,
                        StepValue = src.NumericField3StepValue,
                        DisplayFormat = src.NumericField3DisplayFormat ?? string.Empty
                    }
                }))
                .ForMember(dest => dest.DocumentField1, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.DocumentField1Name,
                    Description = src.DocumentField1Description,
                    ShowInTable = src.DocumentField1ShowInTable
                }))
                .ForMember(dest => dest.DocumentField2, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.DocumentField2Name,
                    Description = src.DocumentField2Description,
                    ShowInTable = src.DocumentField2ShowInTable
                }))
                .ForMember(dest => dest.DocumentField3, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.DocumentField3Name,
                    Description = src.DocumentField3Description,
                    ShowInTable = src.DocumentField3ShowInTable
                }))
                .ForMember(dest => dest.BooleanField1, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.BooleanField1Name,
                    Description = src.BooleanField1Description,
                    ShowInTable = src.BooleanField1ShowInTable
                }))
                .ForMember(dest => dest.BooleanField2, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.BooleanField2Name,
                    Description = src.BooleanField2Description,
                    ShowInTable = src.BooleanField2ShowInTable
                }))
                .ForMember(dest => dest.BooleanField3, opt => opt.MapFrom(src => new CustomFieldConfig
                {
                    Name = src.BooleanField3Name,
                    Description = src.BooleanField3Description,
                    ShowInTable = src.BooleanField3ShowInTable
                }))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.InventoryTags.Select(it => it.Tag)))
                .ForMember(dest => dest.AccessUsers, opt => opt.MapFrom(src => src.UserAccesses.Select(ua => ua.User)))
                .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.Items.Count));

            CreateMap<InventoryDto, Inventory>()
                .ForMember(dest => dest.TextField1Name, opt => opt.MapFrom(src => src.TextField1.Name))
                .ForMember(dest => dest.TextField1Description, opt => opt.MapFrom(src => src.TextField1.Description))
                .ForMember(dest => dest.TextField1ShowInTable, opt => opt.MapFrom(src => src.TextField1.ShowInTable))
                .ForMember(dest => dest.TextField2Name, opt => opt.MapFrom(src => src.TextField2.Name))
                .ForMember(dest => dest.TextField2Description, opt => opt.MapFrom(src => src.TextField2.Description))
                .ForMember(dest => dest.TextField2ShowInTable, opt => opt.MapFrom(src => src.TextField2.ShowInTable))
                .ForMember(dest => dest.TextField3Name, opt => opt.MapFrom(src => src.TextField3.Name))
                .ForMember(dest => dest.TextField3Description, opt => opt.MapFrom(src => src.TextField3.Description))
                .ForMember(dest => dest.TextField3ShowInTable, opt => opt.MapFrom(src => src.TextField3.ShowInTable))
                .ForMember(dest => dest.MultiTextField1Name, opt => opt.MapFrom(src => src.MultiTextField1.Name))
                .ForMember(dest => dest.MultiTextField1Description, opt => opt.MapFrom(src => src.MultiTextField1.Description))
                .ForMember(dest => dest.MultiTextField1ShowInTable, opt => opt.MapFrom(src => src.MultiTextField1.ShowInTable))
                .ForMember(dest => dest.MultiTextField2Name, opt => opt.MapFrom(src => src.MultiTextField2.Name))
                .ForMember(dest => dest.MultiTextField2Description, opt => opt.MapFrom(src => src.MultiTextField2.Description))
                .ForMember(dest => dest.MultiTextField2ShowInTable, opt => opt.MapFrom(src => src.MultiTextField2.ShowInTable))
                .ForMember(dest => dest.MultiTextField3Name, opt => opt.MapFrom(src => src.MultiTextField3.Name))
                .ForMember(dest => dest.MultiTextField3Description, opt => opt.MapFrom(src => src.MultiTextField3.Description))
                .ForMember(dest => dest.MultiTextField3ShowInTable, opt => opt.MapFrom(src => src.MultiTextField3.ShowInTable))
                .ForMember(dest => dest.NumericField1Name, opt => opt.MapFrom(src => src.NumericField1.Name))
                .ForMember(dest => dest.NumericField1Description, opt => opt.MapFrom(src => src.NumericField1.Description))
                .ForMember(dest => dest.NumericField1ShowInTable, opt => opt.MapFrom(src => src.NumericField1.ShowInTable))
                .ForMember(dest => dest.NumericField1IsInteger, opt => opt.MapFrom(src => src.NumericField1.NumericConfig != null && src.NumericField1.NumericConfig.IsInteger))
                .ForMember(dest => dest.NumericField1MinValue, opt => opt.MapFrom(src => src.NumericField1.NumericConfig != null ? src.NumericField1.NumericConfig.MinValue : null))
                .ForMember(dest => dest.NumericField1MaxValue, opt => opt.MapFrom(src => src.NumericField1.NumericConfig != null ? src.NumericField1.NumericConfig.MaxValue : null))
                .ForMember(dest => dest.NumericField1StepValue, opt => opt.MapFrom(src => src.NumericField1.NumericConfig != null ? src.NumericField1.NumericConfig.StepValue : 0.01m))
                .ForMember(dest => dest.NumericField1DisplayFormat, opt => opt.MapFrom(src => src.NumericField1.NumericConfig != null ? src.NumericField1.NumericConfig.DisplayFormat : null))
                
                .ForMember(dest => dest.NumericField2Name, opt => opt.MapFrom(src => src.NumericField2.Name))
                .ForMember(dest => dest.NumericField2Description, opt => opt.MapFrom(src => src.NumericField2.Description))
                .ForMember(dest => dest.NumericField2ShowInTable, opt => opt.MapFrom(src => src.NumericField2.ShowInTable))
                .ForMember(dest => dest.NumericField2IsInteger, opt => opt.MapFrom(src => src.NumericField2.NumericConfig != null && src.NumericField2.NumericConfig.IsInteger))
                .ForMember(dest => dest.NumericField2MinValue, opt => opt.MapFrom(src => src.NumericField2.NumericConfig != null ? src.NumericField2.NumericConfig.MinValue : null))
                .ForMember(dest => dest.NumericField2MaxValue, opt => opt.MapFrom(src => src.NumericField2.NumericConfig != null ? src.NumericField2.NumericConfig.MaxValue : null))
                .ForMember(dest => dest.NumericField2StepValue, opt => opt.MapFrom(src => src.NumericField2.NumericConfig != null ? src.NumericField2.NumericConfig.StepValue : 0.01m))
                .ForMember(dest => dest.NumericField2DisplayFormat, opt => opt.MapFrom(src => src.NumericField2.NumericConfig != null ? src.NumericField2.NumericConfig.DisplayFormat : null))
                
                .ForMember(dest => dest.NumericField3Name, opt => opt.MapFrom(src => src.NumericField3.Name))
                .ForMember(dest => dest.NumericField3Description, opt => opt.MapFrom(src => src.NumericField3.Description))
                .ForMember(dest => dest.NumericField3ShowInTable, opt => opt.MapFrom(src => src.NumericField3.ShowInTable))
                .ForMember(dest => dest.NumericField3IsInteger, opt => opt.MapFrom(src => src.NumericField3.NumericConfig != null && src.NumericField3.NumericConfig.IsInteger))
                .ForMember(dest => dest.NumericField3MinValue, opt => opt.MapFrom(src => src.NumericField3.NumericConfig != null ? src.NumericField3.NumericConfig.MinValue : null))
                .ForMember(dest => dest.NumericField3MaxValue, opt => opt.MapFrom(src => src.NumericField3.NumericConfig != null ? src.NumericField3.NumericConfig.MaxValue : null))
                .ForMember(dest => dest.NumericField3StepValue, opt => opt.MapFrom(src => src.NumericField3.NumericConfig != null ? src.NumericField3.NumericConfig.StepValue : 0.01m))
                .ForMember(dest => dest.NumericField3DisplayFormat, opt => opt.MapFrom(src => src.NumericField3.NumericConfig != null ? src.NumericField3.NumericConfig.DisplayFormat : null))
                .ForMember(dest => dest.DocumentField1Name, opt => opt.MapFrom(src => src.DocumentField1.Name))
                .ForMember(dest => dest.DocumentField1Description, opt => opt.MapFrom(src => src.DocumentField1.Description))
                .ForMember(dest => dest.DocumentField1ShowInTable, opt => opt.MapFrom(src => src.DocumentField1.ShowInTable))
                .ForMember(dest => dest.DocumentField2Name, opt => opt.MapFrom(src => src.DocumentField2.Name))
                .ForMember(dest => dest.DocumentField2Description, opt => opt.MapFrom(src => src.DocumentField2.Description))
                .ForMember(dest => dest.DocumentField2ShowInTable, opt => opt.MapFrom(src => src.DocumentField2.ShowInTable))
                .ForMember(dest => dest.DocumentField3Name, opt => opt.MapFrom(src => src.DocumentField3.Name))
                .ForMember(dest => dest.DocumentField3Description, opt => opt.MapFrom(src => src.DocumentField3.Description))
                .ForMember(dest => dest.DocumentField3ShowInTable, opt => opt.MapFrom(src => src.DocumentField3.ShowInTable))
                .ForMember(dest => dest.BooleanField1Name, opt => opt.MapFrom(src => src.BooleanField1.Name))
                .ForMember(dest => dest.BooleanField1Description, opt => opt.MapFrom(src => src.BooleanField1.Description))
                .ForMember(dest => dest.BooleanField1ShowInTable, opt => opt.MapFrom(src => src.BooleanField1.ShowInTable))
                .ForMember(dest => dest.BooleanField2Name, opt => opt.MapFrom(src => src.BooleanField2.Name))
                .ForMember(dest => dest.BooleanField2Description, opt => opt.MapFrom(src => src.BooleanField2.Description))
                .ForMember(dest => dest.BooleanField2ShowInTable, opt => opt.MapFrom(src => src.BooleanField2.ShowInTable))
                .ForMember(dest => dest.BooleanField3Name, opt => opt.MapFrom(src => src.BooleanField3.Name))
                .ForMember(dest => dest.BooleanField3Description, opt => opt.MapFrom(src => src.BooleanField3.Description))
                .ForMember(dest => dest.BooleanField3ShowInTable, opt => opt.MapFrom(src => src.BooleanField3.ShowInTable))
                .ForMember(dest => dest.InventoryTags, opt => opt.Ignore())
                .ForMember(dest => dest.UserAccesses, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore());

            CreateMap<Item, ItemDto>()
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.TextField1Value))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.MultiTextField1Value))
                .ForMember(dest => dest.InventoryTitle, opt => opt.MapFrom(src => src.Inventory.Title))
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => 
                    src.CreatedBy != null ? $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}" : "Unknown"));

            CreateMap<ItemDto, Item>()
                .ForMember(dest => dest.Likes, opt => opt.Ignore())
                .ForMember(dest => dest.TextField1Value, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.MultiTextField1Value, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.Inventory, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
        }
    }
}
