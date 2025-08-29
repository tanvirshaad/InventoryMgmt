using InventoryMgmt.DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryMgmt.DAL.Interfaces;
using InventoryMgmt.DAL.Repos;

namespace InventoryMgmt.DAL
{
    public class DataAccess
    {
        private readonly ApplicationDbContext db;

        public DataAccess(ApplicationDbContext dbContext)
        {
            db = dbContext;
        }

        public IUserRepo UserData => new UserRepo(db);
        public IInventoryRepo InventoryData => new InventoryRepo(db);
        public ICategoryRepo CategoryData => new CategoryRepo(db);
        public ITagRepo TagData => new TagRepo(db);
        public IItemRepo ItemData => new ItemRepo(db);
        public ICommentRepo CommentData => new CommentRepo(db);
        public IInventoryAccessRepo InventoryAccessData => new InventoryAccessRepo(db);
        public IInventoryTagRepo InventoryTagData => new InventoryTagRepo(db);
        public IItemLikeRepo ItemLikeData => new ItemLikeRepo(db);
    }
}
