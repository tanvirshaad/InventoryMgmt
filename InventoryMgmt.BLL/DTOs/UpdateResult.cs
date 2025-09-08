namespace InventoryMgmt.BLL.DTOs
{
    public class UpdateResult<T>
    {
        public bool IsSuccess { get; set; }
        public bool IsConcurrencyConflict { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }

        public static UpdateResult<T> Success(T data)
        {
            return new UpdateResult<T>
            {
                IsSuccess = true,
                IsConcurrencyConflict = false,
                Data = data
            };
        }

        public static UpdateResult<T> ConcurrencyConflict(T currentData)
        {
            return new UpdateResult<T>
            {
                IsSuccess = false,
                IsConcurrencyConflict = true,
                Data = currentData,
                ErrorMessage = "The item has been modified by another user. Please review the current data and try again."
            };
        }

        public static UpdateResult<T> Error(string errorMessage)
        {
            return new UpdateResult<T>
            {
                IsSuccess = false,
                IsConcurrencyConflict = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
